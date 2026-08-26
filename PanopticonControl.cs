using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using PanopticonAuditHistorySearch.Model;
using PanopticonAuditHistorySearch.Services;
using PanopticonAuditHistorySearch.UI;
using XrmToolBox.Extensibility;

namespace PanopticonAuditHistorySearch
{
    public partial class PanopticonControl : PluginControlBase
    {
        private const int WindowSize = 500;
        private const int BulkDetailWarningThreshold = 2000;

        private readonly System.Windows.Forms.Timer _detailTimer;
        private readonly object _detailLock = new object();

        private MetadataCatalog _catalog;
        private AuditCache _cache;
        private AuditSearch _search;
        private ThrottleGuard _guard;
        private AuditQueryService _query;
        private AuditDetailService _details;
        private EstimateService _estimates;
        private NameResolver _names;
        private SyncEngine _sync;

        private IList<EntityDescriptor> _allEntities = new List<EntityDescriptor>();
        private readonly HashSet<string> _checkedEntities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private CancellationTokenSource _cancellation;

        private SearchResult _result = new SearchResult();
        private List<AuditRow> _window = new List<AuditRow>();
        private int _windowStart = -1;
        private bool _maskFilteringEnabled = true;
        private bool _busy;

        public PanopticonControl()
        {
            InitializeComponent();
            _detailTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _detailTimer.Tick += DetailTimer_Tick;
            BuildGridColumns();
        }

        private void PanopticonControl_Load(object sender, EventArgs e)
        {
            PopulateStaticFilters();
            ResetRange(SyncScope.DefaultDays);
            UpdateConnectionState();
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail,
            string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);
            TearDownCache();
            UpdateConnectionState();
            if (Service != null) ExecuteMethod(InitializeConnection);
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            CancelWork();
            TearDownCache();
            base.ClosingPlugin(info);
        }

        private void UpdateConnectionState()
        {
            var connected = Service != null;
            grpScope.Enabled = connected;
            grpFilters.Enabled = connected && _cache != null;
            tsbSync.Enabled = connected && !_busy;
            tsbExport.Enabled = _result.Available > 0 && !_busy;
            tsbPurge.Enabled = _cache != null && !_busy;

            if (!connected)
            {
                lblStatus.Text = "Not connected. Use the connection button above.";
                tslCacheStats.Text = "No cache loaded.";
            }
        }

        private void InitializeConnection()
        {
            var organizationKey = ConnectionDetail == null
                ? "unknown"
                : (ConnectionDetail.OrganizationUrlName ?? ConnectionDetail.OrganizationFriendlyName ?? "unknown");

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Checking audit access and loading table metadata...",
                Work = (worker, args) =>
                {
                    var guard = new ThrottleGuard(m => { });
                    var details = new AuditDetailService(Service, guard);
                    var probe = new AuditAccessProbe(Service, details);
                    var report = probe.Run(CancellationToken.None);
                    if (!report.IsUsable)
                    {
                        args.Result = report;
                        return;
                    }

                    var catalog = new MetadataCatalog(Service);
                    var entities = catalog.AuditedEntities();
                    args.Result = new ConnectionSetup
                    {
                        Report = report,
                        Catalog = catalog,
                        Entities = entities,
                        OrganizationKey = organizationKey
                    };
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                    {
                        ShowError("Could not initialize against this environment.", args.Error);
                        return;
                    }

                    var blocked = args.Result as AccessReport;
                    if (blocked != null)
                    {
                        MessageBox.Show(blocked.BlockingMessage(), "Audit access unavailable",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        lblStatus.Text = "Audit access unavailable for this account.";
                        return;
                    }

                    ApplySetup((ConnectionSetup)args.Result);
                }
            });
        }

        private void ApplySetup(ConnectionSetup setup)
        {
            _catalog = setup.Catalog;
            _allEntities = setup.Entities;
            _maskFilteringEnabled = setup.Report.MaskUsable;

            _guard = new ThrottleGuard(message => BeginInvoke((Action)(() => lblStatus.Text = message)));
            _query = new AuditQueryService(Service, _guard);
            _details = new AuditDetailService(Service, _guard);
            _estimates = new EstimateService(Service, _guard);
            _names = new NameResolver(Service, _guard);

            OpenCache(setup.OrganizationKey);
            _sync = new SyncEngine(_cache, _query, _catalog);

            RenderEntityList();

            clbFields.Enabled = _maskFilteringEnabled;
            lblFields.Text = _maskFilteringEnabled ? "Changed fields" : "Changed fields (unavailable)";

            var warnings = setup.Report.Warnings();
            lblStatus.Text = warnings.Count > 0
                ? warnings[0]
                : string.Format("{0} audit-enabled table(s) available.", _allEntities.Count);

            UpdateConnectionState();
            RefreshCacheStats();
        }

        private void OpenCache(string organizationKey)
        {
            var path = CacheLocator.DatabasePath(organizationKey);
            var isNew = !File.Exists(path);

            _cache = new AuditCache(path);
            _search = new AuditSearch(_cache);

            if (isNew)
            {
                MessageBox.Show(
                    "Panopticon caches audit data on this machine so searches stay fast.\r\n\r\n" +
                    "Location:\r\n" + path + "\r\n\r\n" +
                    "Use Purge cache to remove it at any time.",
                    "Local audit cache", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TearDownCache()
        {
            if (_cache == null) return;
            _cache.Dispose();
            _cache = null;
            _search = null;
            _result = new SearchResult();
            _window.Clear();
            _windowStart = -1;
            grid.RowCount = 0;
        }

        private void RenderEntityList()
        {
            var filter = txtEntityFilter.Text.Trim();
            var visible = _allEntities.Where(e =>
                filter.Length == 0
                || e.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || e.LogicalName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            clbEntities.BeginUpdate();
            clbEntities.Items.Clear();
            foreach (var entity in visible)
            {
                var index = clbEntities.Items.Add(entity);
                if (_checkedEntities.Contains(entity.LogicalName))
                    clbEntities.SetItemChecked(index, true);
            }
            clbEntities.EndUpdate();
        }

        private void txtEntityFilter_TextChanged(object sender, EventArgs e)
        {
            RenderEntityList();
        }

        private void clbEntities_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var entity = clbEntities.Items[e.Index] as EntityDescriptor;
            if (entity == null) return;

            if (e.NewValue == CheckState.Checked) _checkedEntities.Add(entity.LogicalName);
            else _checkedEntities.Remove(entity.LogicalName);

            BeginInvoke((Action)RenderFieldList);
        }

        private void range_ValueChanged(object sender, EventArgs e)
        {
            var scope = CurrentScope();
            if (scope.SpanDays > SyncScope.ConfirmationRequiredDays)
                lblEstimate.Text = string.Format(
                    "{0} days selected. Ranges over {1} days need confirmation before syncing.",
                    scope.SpanDays, SyncScope.ConfirmationRequiredDays);
            else if (scope.SpanDays > SyncScope.PreflightRequiredDays)
                lblEstimate.Text = string.Format(
                    "{0} days selected. Estimate the cost before syncing a range this wide.", scope.SpanDays);
            else
                lblEstimate.Text = string.Format("{0} days selected.", scope.SpanDays);
        }

        private void lnkRange_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender == lnk30) ResetRange(30);
            else if (sender == lnk90) ResetRange(90);
            else ResetRange(365);
        }

        private void ResetRange(int days)
        {
            dtpTo.Value = DateTime.Today;
            dtpFrom.Value = DateTime.Today.AddDays(-days);
            range_ValueChanged(this, EventArgs.Empty);
        }

        private SyncScope CurrentScope()
        {
            return new SyncScope
            {
                Entities = SelectedEntities().Select(e => e.ToScope()).ToList(),
                FromUtc = dtpFrom.Value.Date.ToUniversalTime(),
                ToUtc = dtpTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime()
            };
        }

        private IList<EntityDescriptor> SelectedEntities()
        {
            return _allEntities.Where(e => _checkedEntities.Contains(e.LogicalName)).ToList();
        }

        private void btnPreflight_Click(object sender, EventArgs e)
        {
            ExecuteMethod(RunPreflight);
        }

        private void RunPreflight()
        {
            var scope = CurrentScope();
            var problem = scope.Validate();
            if (problem != null) { Warn(problem); return; }

            SetBusy(true);
            var token = NewCancellation();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Estimating audit volume...",
                IsCancelable = true,
                Work = (worker, args) => args.Result = _estimates.Estimate(scope, token),
                PostWorkCallBack = args =>
                {
                    SetBusy(false);
                    if (args.Cancelled) { lblStatus.Text = "Estimate cancelled."; return; }
                    if (args.Error != null) { ShowError("Estimate failed.", args.Error); return; }

                    var estimate = (SyncEstimate)args.Result;
                    lblEstimate.Text = estimate.Summary();
                    lblStatus.Text = string.Join("   |   ", estimate.Entities.Select(x =>
                        string.Format("{0}: {1:N0}{2}", x.Entity.DisplayName, x.Rows,
                            x.Sampled ? " (est.)" : string.Empty)));
                }
            });
        }

        private void tsbSync_Click(object sender, EventArgs e)
        {
            ExecuteMethod(RunSync);
        }

        private void RunSync()
        {
            var scope = CurrentScope();
            var problem = scope.Validate();
            if (problem != null) { Warn(problem); return; }

            if (scope.RequiresConfirmation)
            {
                var answer = MessageBox.Show(
                    string.Format(
                        "This range covers {0} days. Pulling that much audit history can take a long time and " +
                        "use significant disk space.\r\n\r\nRun Estimate cost first if you have not already." +
                        "\r\n\r\nContinue?", scope.SpanDays),
                    "Wide date range", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) return;
            }

            SetBusy(true);
            progress.Visible = true;
            progress.Value = 0;

            var token = NewCancellation();
            var forceRefresh = chkForceRefresh.Checked;
            var reporter = new Progress<SyncProgress>(p =>
            {
                progress.Value = Math.Min(100, p.Percent);
                lblStatus.Text = string.Format("{0}   ({1:N0} rows so far)", p.Message, p.RowsLoaded);
            });

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Syncing audit data...",
                IsCancelable = true,
                Work = (worker, args) => args.Result = _sync.Run(scope, forceRefresh, reporter, token),
                PostWorkCallBack = args =>
                {
                    SetBusy(false);
                    progress.Visible = false;

                    if (args.Cancelled || args.Error is OperationCanceledException)
                    {
                        lblStatus.Text = "Sync cancelled. Completed windows are kept and skipped next time.";
                        RefreshCacheStats();
                        RenderFieldList();
                        return;
                    }

                    if (args.Error != null)
                    {
                        ShowError("Sync failed.", args.Error);
                        RefreshCacheStats();
                        return;
                    }

                    var outcome = (SyncOutcome)args.Result;
                    lblStatus.Text = "Sync complete: " + outcome.Describe();
                    grpFilters.Enabled = true;
                    RefreshCacheStats();
                    RenderFieldList();
                    RunSearch();
                }
            });
        }

        private void tsbCancel_Click(object sender, EventArgs e)
        {
            CancelWork();
            CancelWorker();
        }

        private void PopulateStaticFilters()
        {
            cboAction.Items.Add(new FilterOption("(any event)", null));
            foreach (var pair in AuditLabels.AllActions.OrderBy(p => p.Value))
                cboAction.Items.Add(new FilterOption(pair.Value, pair.Key));
            cboAction.SelectedIndex = 0;

            cboOperation.Items.Add(new FilterOption("(any operation)", null));
            foreach (var pair in AuditLabels.AllOperations.OrderBy(p => p.Value))
                cboOperation.Items.Add(new FilterOption(pair.Value, pair.Key));
            cboOperation.SelectedIndex = 0;

            cboUser.Items.Add(new FacetValue { Key = Guid.Empty, Label = "(anyone)" });
            cboUser.SelectedIndex = 0;
        }

        private void RenderFieldList()
        {
            if (_catalog == null || !_maskFilteringEnabled) return;

            var options = new Dictionary<string, FieldFilterOption>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in SelectedEntities())
            {
                IList<ColumnInfo> columns;
                try { columns = _catalog.AuditedColumns(entity.ToScope()); }
                catch (Exception) { continue; }

                foreach (var column in columns)
                {
                    FieldFilterOption option;
                    if (!options.TryGetValue(column.LogicalName, out option))
                    {
                        option = new FieldFilterOption
                        {
                            LogicalName = column.LogicalName,
                            DisplayName = column.DisplayName,
                            Selectors = new List<FieldSelector>()
                        };
                        options[column.LogicalName] = option;
                    }
                    option.Selectors.Add(new FieldSelector
                    {
                        ObjectTypeCode = entity.ObjectTypeCode,
                        ColumnNumber = column.ColumnNumber
                    });
                }
            }

            var previouslyChecked = new HashSet<string>(
                clbFields.CheckedItems.Cast<FieldFilterOption>().Select(o => o.LogicalName),
                StringComparer.OrdinalIgnoreCase);

            clbFields.BeginUpdate();
            clbFields.Items.Clear();
            foreach (var option in options.Values
                .OrderBy(o => o.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                var index = clbFields.Items.Add(option);
                if (previouslyChecked.Contains(option.LogicalName))
                    clbFields.SetItemChecked(index, true);
            }
            clbFields.EndUpdate();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            RunSearch();
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            cboUser.SelectedIndex = 0;
            cboAction.SelectedIndex = 0;
            cboOperation.SelectedIndex = 0;
            txtRecord.Clear();
            for (var i = 0; i < clbFields.Items.Count; i++) clbFields.SetItemChecked(i, false);
            RunSearch();
        }

        private SearchCriteria BuildCriteria()
        {
            var criteria = new SearchCriteria
            {
                ObjectTypeCodes = SelectedEntities().Select(e => e.ObjectTypeCode).ToList(),
                FromUtc = dtpFrom.Value.Date.ToUniversalTime(),
                ToUtc = dtpTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime(),
                ObjectNameContains = txtRecord.Text
            };

            var user = cboUser.SelectedItem as FacetValue;
            if (user != null && user.Key != Guid.Empty)
                criteria.UserIds = new List<Guid> { user.Key };

            var action = cboAction.SelectedItem as FilterOption;
            if (action != null && action.Value.HasValue)
                criteria.Actions = new List<int> { action.Value.Value };

            var operation = cboOperation.SelectedItem as FilterOption;
            if (operation != null && operation.Value.HasValue)
                criteria.Operations = new List<int> { operation.Value.Value };

            if (_maskFilteringEnabled && clbFields.CheckedItems.Count > 0)
            {
                criteria.ChangedFields = clbFields.CheckedItems
                    .Cast<FieldFilterOption>()
                    .SelectMany(o => o.Selectors)
                    .ToList();
            }

            return criteria;
        }

        private void RunSearch()
        {
            if (_search == null) return;

            var criteria = BuildCriteria();
            SetBusy(true);

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Searching cache...",
                Work = (worker, args) => args.Result = new SearchOutput
                {
                    Result = _search.Run(criteria),
                    UserFacet = _search.UserFacet(criteria)
                },
                PostWorkCallBack = args =>
                {
                    SetBusy(false);
                    if (args.Error != null) { ShowError("Search failed.", args.Error); return; }

                    var output = (SearchOutput)args.Result;
                    _result = output.Result;
                    _window.Clear();
                    _windowStart = -1;

                    grid.RowCount = (int)_result.Available;
                    grid.Invalidate();
                    lblResults.Text = _result.Describe();

                    ApplyUserFacet(output.UserFacet);
                    ClearDetail();
                    ResolveVisibleNames();
                    UpdateConnectionState();
                }
            });
        }

        private void ApplyUserFacet(IList<FacetValue> facet)
        {
            var previous = cboUser.SelectedItem as FacetValue;

            cboUser.BeginUpdate();
            cboUser.Items.Clear();
            cboUser.Items.Add(new FacetValue { Key = Guid.Empty, Label = "(anyone)" });
            foreach (var value in facet) cboUser.Items.Add(value);
            cboUser.EndUpdate();

            var index = 0;
            if (previous != null && previous.Key != Guid.Empty)
            {
                for (var i = 1; i < cboUser.Items.Count; i++)
                {
                    if (((FacetValue)cboUser.Items[i]).Key != previous.Key) continue;
                    index = i;
                    break;
                }
            }
            cboUser.SelectedIndex = index;
        }

        private void BuildGridColumns()
        {
            grid.Columns.Clear();
            grid.Columns.Add(NewColumn("Changed Date", 145));
            grid.Columns.Add(NewColumn("Table", 110));
            grid.Columns.Add(NewColumn("Record", 170));
            grid.Columns.Add(NewColumn("Event", 90));
            grid.Columns.Add(NewColumn("Operation", 80));
            grid.Columns.Add(NewColumn("Changed By", 140));
            grid.Columns.Add(NewColumn("Changed Fields", 260));
        }

        private static DataGridViewTextBoxColumn NewColumn(string header, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Width = width,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void grid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            var row = RowAt(e.RowIndex);
            if (row == null) { e.Value = string.Empty; return; }

            switch (e.ColumnIndex)
            {
                case 0: e.Value = row.CreatedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"); break;
                case 1: e.Value = row.EntityDisplayName ?? row.EntityLogicalName; break;
                case 2: e.Value = row.ObjectName; break;
                case 3: e.Value = row.ActionLabel; break;
                case 4: e.Value = row.OperationLabel; break;
                case 5: e.Value = row.UserName; break;
                case 6: e.Value = row.ChangedFieldsLabel; break;
                default: e.Value = string.Empty; break;
            }
        }

        private AuditRow RowAt(int index)
        {
            if (_search == null || index < 0 || index >= _result.Available) return null;

            if (_windowStart < 0 || index < _windowStart || index >= _windowStart + _window.Count)
            {
                _windowStart = Math.Max(0, index - WindowSize / 2);
                _window = _search.Page(_windowStart, WindowSize).ToList();
                ResolveVisibleNames();
            }

            var offset = index - _windowStart;
            return offset >= 0 && offset < _window.Count ? _window[offset] : null;
        }

        private void ResolveVisibleNames()
        {
            if (_search == null || _names == null || _busy) return;

            var start = _windowStart < 0 ? 0 : _windowStart;
            var pending = _search.UnresolvedObjectIds(start, WindowSize);
            var users = _search.UnresolvedUserIds(200);
            if (pending.Count == 0 && users.Count == 0) return;

            var token = _cancellation == null ? CancellationToken.None : _cancellation.Token;

            Task.Run(() =>
            {
                try
                {
                    if (users.Count > 0)
                        _cache.SavePrincipals(_names.ResolveUsers(users, token));

                    foreach (var pair in pending)
                    {
                        var descriptor = _catalog.Describe(pair.Key);
                        if (descriptor == null) continue;
                        var resolved = _names.ResolveRecords(descriptor.LogicalName,
                            descriptor.LogicalName + "id", descriptor.PrimaryNameAttribute, pair.Value, token);
                        _cache.SaveObjectNames(pair.Key, resolved);
                    }
                }
                catch (Exception) { return; }

                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke((Action)(() =>
                {
                    _windowStart = -1;
                    _window.Clear();
                    grid.Invalidate();
                }));
            }, token);
        }

        private void grid_SelectionChanged(object sender, EventArgs e)
        {
            _detailTimer.Stop();
            _detailTimer.Start();
        }

        private void DetailTimer_Tick(object sender, EventArgs e)
        {
            _detailTimer.Stop();
            LoadDetailForSelection();
        }

        private AuditRow SelectedRow()
        {
            return grid.SelectedRows.Count == 0 ? null : RowAt(grid.SelectedRows[0].Index);
        }

        private void LoadDetailForSelection()
        {
            var row = SelectedRow();
            if (row == null) { ClearDetail(); return; }

            var cached = _cache.GetDetail(row.AuditId);
            if (cached != null) { ShowDetail(row, cached); return; }

            lblDetailHeader.Text = "Loading values...";
            gridDetail.DataSource = null;

            var auditId = row.AuditId;
            var token = _cancellation == null ? CancellationToken.None : _cancellation.Token;

            Task.Run(() =>
            {
                AuditDetailPayload payload;
                lock (_detailLock)
                {
                    payload = _details.Fetch(auditId, token);
                    if (payload.Error == null) _cache.SaveDetail(payload);
                }

                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke((Action)(() =>
                {
                    var current = SelectedRow();
                    if (current == null || current.AuditId != auditId) return;
                    ShowDetail(current, payload);
                }));
            }, token);
        }

        private void ShowDetail(AuditRow row, AuditDetailPayload payload)
        {
            lblDetailHeader.Text = string.Format("{0}  -  {1}  by {2}  on {3}",
                row.EntityDisplayName ?? row.EntityLogicalName,
                row.ObjectName ?? row.ObjectId.ToString(),
                row.UserName ?? "(unknown user)",
                row.CreatedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

            if (payload.Error != null)
            {
                gridDetail.DataSource = null;
                lblNarrative.ForeColor = Color.Firebrick;
                lblNarrative.Text = payload.Error;
                return;
            }

            lblNarrative.ForeColor = SystemColors.ControlText;
            lblNarrative.Text = payload.Narrative ?? string.Empty;

            var changes = payload.Changes ?? new List<FieldChange>();
            var labels = ColumnLabels(row.ObjectTypeCode);
            foreach (var change in changes)
            {
                string label;
                change.DisplayName = labels.TryGetValue(change.LogicalName, out label)
                    ? label
                    : change.LogicalName;
            }

            gridDetail.DataSource = changes
                .Select(c => new
                {
                    Field = c.DisplayName,
                    Old = c.OldValue,
                    New = c.NewValue,
                    Note = c.Truncated ? "value truncated at 5 KB by Dataverse" : string.Empty
                })
                .ToList();

            if (gridDetail.Columns.Count == 0) return;
            gridDetail.Columns[0].Width = 200;
            gridDetail.Columns[1].Width = 260;
            gridDetail.Columns[2].Width = 260;
        }

        private Dictionary<string, string> ColumnLabels(int objectTypeCode)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var descriptor = _catalog == null ? null : _catalog.Describe(objectTypeCode);
            if (descriptor == null) return map;

            try
            {
                foreach (var column in _catalog.Columns(descriptor.ToScope()).Values)
                    map[column.LogicalName] = column.DisplayName;
            }
            catch (Exception) { }

            return map;
        }

        private void ClearDetail()
        {
            gridDetail.DataSource = null;
            lblNarrative.Text = string.Empty;
            lblDetailHeader.Text = "Select a row to load its old and new values.";
        }

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = RowAt(e.RowIndex);
            if (row == null || row.ObjectId == Guid.Empty || row.EntityLogicalName == null) return;

            using (var form = new RecordTimelineForm(_details, row))
            {
                form.ShowDialog(this);
            }
        }

        private void tsbExport_Click(object sender, EventArgs e)
        {
            if (_search == null || _result.Available == 0) { Warn("Run a search first."); return; }

            using (var dialog = new ExportOptionsForm((int)_result.Available, BulkDetailWarningThreshold))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                ExportRows(dialog.RowLimit, dialog.IncludeValues, dialog.TargetPath);
            }
        }

        private void ExportRows(int limit, bool includeValues, string path)
        {
            SetBusy(true);
            progress.Visible = includeValues;
            progress.Value = 0;

            var token = NewCancellation();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Exporting...",
                IsCancelable = true,
                Work = (worker, args) =>
                {
                    var rows = _search.All(limit);
                    Dictionary<Guid, AuditDetailPayload> details = null;

                    if (includeValues)
                    {
                        details = new Dictionary<Guid, AuditDetailPayload>();
                        var missing = new List<Guid>();

                        foreach (var row in rows)
                        {
                            var cached = _cache.GetDetail(row.AuditId);
                            if (cached != null) details[row.AuditId] = cached;
                            else missing.Add(row.AuditId);
                        }

                        if (missing.Count > 0)
                        {
                            foreach (var payload in _details.FetchBatch(missing, token))
                            {
                                details[payload.AuditId] = payload;
                                if (payload.Error == null) _cache.SaveDetail(payload);
                            }
                        }
                    }

                    args.Result = ExportService.Write(path, rows, details);
                },
                PostWorkCallBack = args =>
                {
                    SetBusy(false);
                    progress.Visible = false;

                    if (args.Cancelled || args.Error is OperationCanceledException)
                    {
                        lblStatus.Text = "Export cancelled.";
                        return;
                    }
                    if (args.Error != null) { ShowError("Export failed.", args.Error); return; }

                    var written = (int)args.Result;
                    lblStatus.Text = string.Format("Exported {0:N0} line(s) to {1}", written, path);

                    if (MessageBox.Show("Export complete. Open it now?", "Export",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
            });
        }

        private void tsbPurge_Click(object sender, EventArgs e)
        {
            if (_cache == null) return;

            var path = _cache.DatabasePath;
            var size = SyncEstimate.FormatBytes(CacheLocator.SizeOnDisk(path));

            var answer = MessageBox.Show(
                "Delete the local audit cache for this environment?\r\n\r\n" + path +
                "\r\n\r\nCurrent size: " + size +
                "\r\n\r\nThis cannot be undone; the next search will need a fresh sync.",
                "Purge cache", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            TearDownCache();
            try { CacheLocator.Delete(path); }
            catch (IOException ex) { ShowError("Could not delete every cache file.", ex); }

            _cache = new AuditCache(path);
            _search = new AuditSearch(_cache);
            _sync = new SyncEngine(_cache, _query, _catalog);

            lblResults.Text = "Cache purged. Sync again to search.";
            ClearDetail();
            RefreshCacheStats();
            UpdateConnectionState();
        }

        private void RefreshCacheStats()
        {
            tslCacheStats.Text = _cache == null ? "No cache loaded." : _cache.Stats().Describe();
        }

        private CancellationToken NewCancellation()
        {
            CancelWork();
            _cancellation = new CancellationTokenSource();
            return _cancellation.Token;
        }

        private void CancelWork()
        {
            if (_cancellation == null) return;
            try { _cancellation.Cancel(); }
            catch (ObjectDisposedException) { }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            tsbCancel.Enabled = busy;
            btnPreflight.Enabled = !busy;
            btnSearch.Enabled = !busy;
            UpdateConnectionState();
        }

        private void ShowError(string context, Exception error)
        {
            lblStatus.Text = context;
            MessageBox.Show(context + "\r\n\r\n" + error.Message, "Panopticon",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Warn(string message)
        {
            lblStatus.Text = message;
            MessageBox.Show(message, "Panopticon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private class ConnectionSetup
        {
            public AccessReport Report { get; set; }
            public MetadataCatalog Catalog { get; set; }
            public IList<EntityDescriptor> Entities { get; set; }
            public string OrganizationKey { get; set; }
        }

        private class SearchOutput
        {
            public SearchResult Result { get; set; }
            public IList<FacetValue> UserFacet { get; set; }
        }

        private class FilterOption
        {
            public string Label { get; private set; }
            public int? Value { get; private set; }

            public FilterOption(string label, int? value)
            {
                Label = label;
                Value = value;
            }

            public override string ToString() { return Label; }
        }
    }
}
