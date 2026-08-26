using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PanopticonAuditHistorySearch.Model;
using PanopticonAuditHistorySearch.Services;

namespace PanopticonAuditHistorySearch.UI
{
    public class RecordTimelineForm : Form
    {
        private const int MaxPages = 20;

        private readonly AuditDetailService _details;
        private readonly AuditRow _row;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        private readonly TreeView _tree;
        private readonly DataGridView _grid;
        private readonly Label _status;

        private IList<TimelineEntry> _entries = new List<TimelineEntry>();

        public RecordTimelineForm(AuditDetailService details, AuditRow row)
        {
            _details = details;
            _row = row;

            Text = string.Format("Timeline - {0} ({1})",
                row.ObjectName ?? row.ObjectId.ToString(), row.EntityDisplayName ?? row.EntityLogicalName);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 560);
            MinimumSize = new Size(640, 400);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 330,
                FixedPanel = FixedPanel.Panel1
            };

            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
            _tree.AfterSelect += Tree_AfterSelect;
            split.Panel1.Controls.Add(_tree);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            split.Panel2.Controls.Add(_grid);

            _status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                Padding = new Padding(6, 5, 0, 0),
                Text = "Loading change history..."
            };

            var close = new Button { Text = "Close", Dock = DockStyle.Bottom, Height = 30 };
            close.Click += (s, e) => Close();

            Controls.Add(split);
            Controls.Add(_status);
            Controls.Add(close);

            Load += RecordTimelineForm_Load;
            FormClosed += (s, e) => _cancellation.Cancel();
        }

        private void RecordTimelineForm_Load(object sender, EventArgs e)
        {
            var token = _cancellation.Token;
            var logicalName = _row.EntityLogicalName;
            var recordId = _row.ObjectId;

            Task.Run(() =>
            {
                IList<TimelineEntry> entries;
                string error = null;
                try
                {
                    entries = _details.Timeline(logicalName, recordId, MaxPages, token);
                }
                catch (OperationCanceledException) { return; }
                catch (Exception ex)
                {
                    entries = new List<TimelineEntry>();
                    error = ex.Message;
                }

                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke((Action)(() => Render(entries, error)));
            }, token);
        }

        private void Render(IList<TimelineEntry> entries, string error)
        {
            _entries = entries;

            if (error != null)
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = error;
                return;
            }

            _tree.BeginUpdate();
            _tree.Nodes.Clear();

            foreach (var group in entries.GroupBy(x => x.CreatedOn.ToLocalTime().Date).OrderByDescending(g => g.Key))
            {
                var dayNode = _tree.Nodes.Add(group.Key.ToString("D"));
                foreach (var entry in group.OrderByDescending(x => x.CreatedOn))
                {
                    var label = string.Format("{0:HH:mm:ss}  {1}  by {2}",
                        entry.CreatedOn.ToLocalTime(),
                        entry.ActionLabel ?? "(event)",
                        entry.UserName ?? "(unknown)");
                    var node = dayNode.Nodes.Add(label);
                    node.Tag = entry;
                }
                dayNode.Expand();
            }

            _tree.EndUpdate();

            _status.ForeColor = SystemColors.ControlText;
            _status.Text = entries.Count == 0
                ? "No audit history is recorded for this record."
                : string.Format("{0:N0} change event(s){1}.", entries.Count,
                    entries.Count >= MaxPages * AuditDetailService.TimelinePageSize
                        ? string.Format("; showing the first {0} pages", MaxPages)
                        : string.Empty);

            if (_tree.Nodes.Count > 0 && _tree.Nodes[0].Nodes.Count > 0)
                _tree.SelectedNode = _tree.Nodes[0].Nodes[0];
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var entry = e.Node.Tag as TimelineEntry;
            if (entry == null || entry.Detail == null) { _grid.DataSource = null; return; }

            var changes = entry.Detail.Changes ?? new List<FieldChange>();
            if (changes.Count == 0)
            {
                _grid.DataSource = new[]
                {
                    new
                    {
                        Field = "(no field values)",
                        Old = string.Empty,
                        New = entry.Detail.Narrative ?? entry.Detail.Error ?? string.Empty
                    }
                }.ToList();
                return;
            }

            _grid.DataSource = changes
                .Select(c => new
                {
                    Field = c.DisplayName ?? c.LogicalName,
                    Old = c.OldValue,
                    New = c.NewValue
                })
                .ToList();

            if (_grid.Columns.Count == 0) return;
            _grid.Columns[0].Width = 180;
            _grid.Columns[1].Width = 190;
            _grid.Columns[2].Width = 190;
        }
    }
}
