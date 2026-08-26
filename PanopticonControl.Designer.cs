namespace PanopticonAuditHistorySearch
{
    partial class PanopticonControl
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton tsbSync;
        private System.Windows.Forms.ToolStripButton tsbCancel;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton tsbExport;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton tsbPurge;
        private System.Windows.Forms.ToolStripSeparator tsSep3;
        private System.Windows.Forms.ToolStripLabel tslCacheStats;

        private System.Windows.Forms.SplitContainer splitOuter;
        private System.Windows.Forms.SplitContainer splitRight;

        private System.Windows.Forms.Panel pnlSide;
        private System.Windows.Forms.GroupBox grpFilters;
        private System.Windows.Forms.CheckedListBox clbFields;
        private System.Windows.Forms.Label lblFields;
        private System.Windows.Forms.TextBox txtRecord;
        private System.Windows.Forms.Label lblRecord;
        private System.Windows.Forms.ComboBox cboOperation;
        private System.Windows.Forms.Label lblOperation;
        private System.Windows.Forms.ComboBox cboAction;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.ComboBox cboUser;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnResetFilters;

        private System.Windows.Forms.GroupBox grpScope;
        private System.Windows.Forms.Label lblEntities;
        private System.Windows.Forms.TextBox txtEntityFilter;
        private System.Windows.Forms.CheckedListBox clbEntities;
        private System.Windows.Forms.Label lblRange;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.LinkLabel lnk30;
        private System.Windows.Forms.LinkLabel lnk90;
        private System.Windows.Forms.LinkLabel lnk365;
        private System.Windows.Forms.Button btnPreflight;
        private System.Windows.Forms.Label lblEstimate;
        private System.Windows.Forms.CheckBox chkForceRefresh;

        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Panel pnlResultHeader;
        private System.Windows.Forms.Label lblResults;

        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailHeader;
        private System.Windows.Forms.DataGridView gridDetail;
        private System.Windows.Forms.Label lblNarrative;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar progress;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbSync = new System.Windows.Forms.ToolStripButton();
            this.tsbCancel = new System.Windows.Forms.ToolStripButton();
            this.tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExport = new System.Windows.Forms.ToolStripButton();
            this.tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPurge = new System.Windows.Forms.ToolStripButton();
            this.tsSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.tslCacheStats = new System.Windows.Forms.ToolStripLabel();
            this.splitOuter = new System.Windows.Forms.SplitContainer();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.pnlSide = new System.Windows.Forms.Panel();
            this.grpFilters = new System.Windows.Forms.GroupBox();
            this.btnResetFilters = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.clbFields = new System.Windows.Forms.CheckedListBox();
            this.lblFields = new System.Windows.Forms.Label();
            this.txtRecord = new System.Windows.Forms.TextBox();
            this.lblRecord = new System.Windows.Forms.Label();
            this.cboOperation = new System.Windows.Forms.ComboBox();
            this.lblOperation = new System.Windows.Forms.Label();
            this.cboAction = new System.Windows.Forms.ComboBox();
            this.lblAction = new System.Windows.Forms.Label();
            this.cboUser = new System.Windows.Forms.ComboBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.grpScope = new System.Windows.Forms.GroupBox();
            this.chkForceRefresh = new System.Windows.Forms.CheckBox();
            this.lblEstimate = new System.Windows.Forms.Label();
            this.btnPreflight = new System.Windows.Forms.Button();
            this.lnk365 = new System.Windows.Forms.LinkLabel();
            this.lnk90 = new System.Windows.Forms.LinkLabel();
            this.lnk30 = new System.Windows.Forms.LinkLabel();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblRange = new System.Windows.Forms.Label();
            this.clbEntities = new System.Windows.Forms.CheckedListBox();
            this.txtEntityFilter = new System.Windows.Forms.TextBox();
            this.lblEntities = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.pnlResultHeader = new System.Windows.Forms.Panel();
            this.lblResults = new System.Windows.Forms.Label();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.gridDetail = new System.Windows.Forms.DataGridView();
            this.lblNarrative = new System.Windows.Forms.Label();
            this.lblDetailHeader = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.progress = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)(this.splitOuter)).BeginInit();
            this.splitOuter.Panel1.SuspendLayout();
            this.splitOuter.Panel2.SuspendLayout();
            this.splitOuter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.pnlSide.SuspendLayout();
            this.grpFilters.SuspendLayout();
            this.grpScope.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.pnlResultHeader.SuspendLayout();
            this.pnlDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDetail)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // toolStrip
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsbSync, this.tsbCancel, this.tsSep1, this.tsbExport,
                this.tsSep2, this.tsbPurge, this.tsSep3, this.tslCacheStats});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1100, 25);
            this.toolStrip.TabIndex = 0;

            this.tsbSync.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbSync.Name = "tsbSync";
            this.tsbSync.Text = "Sync audit data";
            this.tsbSync.ToolTipText = "Pull audit metadata for the selected tables and date range into the local cache";
            this.tsbSync.Click += new System.EventHandler(this.tsbSync_Click);

            this.tsbCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbCancel.Enabled = false;
            this.tsbCancel.Name = "tsbCancel";
            this.tsbCancel.Text = "Cancel";
            this.tsbCancel.Click += new System.EventHandler(this.tsbCancel_Click);

            this.tsbExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbExport.Name = "tsbExport";
            this.tsbExport.Text = "Export CSV";
            this.tsbExport.Click += new System.EventHandler(this.tsbExport_Click);

            this.tsbPurge.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbPurge.Name = "tsbPurge";
            this.tsbPurge.Text = "Purge cache";
            this.tsbPurge.Click += new System.EventHandler(this.tsbPurge_Click);

            this.tslCacheStats.Name = "tslCacheStats";
            this.tslCacheStats.Text = "No cache loaded.";

            // splitOuter
            this.splitOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitOuter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitOuter.Location = new System.Drawing.Point(0, 25);
            this.splitOuter.Name = "splitOuter";
            this.splitOuter.Panel1.Controls.Add(this.pnlSide);
            this.splitOuter.Panel2.Controls.Add(this.splitRight);
            this.splitOuter.Size = new System.Drawing.Size(1100, 597);
            this.splitOuter.SplitterDistance = 320;
            this.splitOuter.TabIndex = 1;

            // splitRight
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitRight.Panel1.Controls.Add(this.grid);
            this.splitRight.Panel1.Controls.Add(this.pnlResultHeader);
            this.splitRight.Panel2.Controls.Add(this.pnlDetail);
            this.splitRight.Size = new System.Drawing.Size(776, 597);
            this.splitRight.SplitterDistance = 360;
            this.splitRight.TabIndex = 0;

            // pnlSide
            this.pnlSide.AutoScroll = true;
            this.pnlSide.Controls.Add(this.grpFilters);
            this.pnlSide.Controls.Add(this.grpScope);
            this.pnlSide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSide.Location = new System.Drawing.Point(0, 0);
            this.pnlSide.Name = "pnlSide";
            this.pnlSide.Padding = new System.Windows.Forms.Padding(6);
            this.pnlSide.Size = new System.Drawing.Size(320, 597);
            this.pnlSide.TabIndex = 0;

            // grpScope
            this.grpScope.Controls.Add(this.lblEntities);
            this.grpScope.Controls.Add(this.txtEntityFilter);
            this.grpScope.Controls.Add(this.clbEntities);
            this.grpScope.Controls.Add(this.lblRange);
            this.grpScope.Controls.Add(this.dtpFrom);
            this.grpScope.Controls.Add(this.dtpTo);
            this.grpScope.Controls.Add(this.lnk30);
            this.grpScope.Controls.Add(this.lnk90);
            this.grpScope.Controls.Add(this.lnk365);
            this.grpScope.Controls.Add(this.btnPreflight);
            this.grpScope.Controls.Add(this.lblEstimate);
            this.grpScope.Controls.Add(this.chkForceRefresh);
            this.grpScope.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpScope.Location = new System.Drawing.Point(6, 6);
            this.grpScope.Name = "grpScope";
            this.grpScope.Size = new System.Drawing.Size(300, 330);
            this.grpScope.TabIndex = 0;
            this.grpScope.TabStop = false;
            this.grpScope.Text = "Scope";

            this.lblEntities.AutoSize = true;
            this.lblEntities.Location = new System.Drawing.Point(8, 20);
            this.lblEntities.Name = "lblEntities";
            this.lblEntities.Size = new System.Drawing.Size(40, 13);
            this.lblEntities.TabIndex = 0;
            this.lblEntities.Text = "Tables";

            this.txtEntityFilter.Location = new System.Drawing.Point(11, 38);
            this.txtEntityFilter.Name = "txtEntityFilter";
            this.txtEntityFilter.Size = new System.Drawing.Size(278, 20);
            this.txtEntityFilter.TabIndex = 1;
            this.txtEntityFilter.TextChanged += new System.EventHandler(this.txtEntityFilter_TextChanged);

            this.clbEntities.CheckOnClick = true;
            this.clbEntities.IntegralHeight = false;
            this.clbEntities.Location = new System.Drawing.Point(11, 64);
            this.clbEntities.Name = "clbEntities";
            this.clbEntities.Size = new System.Drawing.Size(278, 120);
            this.clbEntities.TabIndex = 2;
            this.clbEntities.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbEntities_ItemCheck);

            this.lblRange.AutoSize = true;
            this.lblRange.Location = new System.Drawing.Point(8, 192);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(90, 13);
            this.lblRange.TabIndex = 3;
            this.lblRange.Text = "Date range (UTC)";

            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(11, 209);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(130, 20);
            this.dtpFrom.TabIndex = 4;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.range_ValueChanged);

            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(159, 209);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(130, 20);
            this.dtpTo.TabIndex = 5;
            this.dtpTo.ValueChanged += new System.EventHandler(this.range_ValueChanged);

            this.lnk30.AutoSize = true;
            this.lnk30.Location = new System.Drawing.Point(11, 236);
            this.lnk30.Name = "lnk30";
            this.lnk30.Size = new System.Drawing.Size(52, 13);
            this.lnk30.TabIndex = 6;
            this.lnk30.TabStop = true;
            this.lnk30.Text = "Last 30 d";
            this.lnk30.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRange_LinkClicked);

            this.lnk90.AutoSize = true;
            this.lnk90.Location = new System.Drawing.Point(80, 236);
            this.lnk90.Name = "lnk90";
            this.lnk90.Size = new System.Drawing.Size(52, 13);
            this.lnk90.TabIndex = 7;
            this.lnk90.TabStop = true;
            this.lnk90.Text = "Last 90 d";
            this.lnk90.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRange_LinkClicked);

            this.lnk365.AutoSize = true;
            this.lnk365.Location = new System.Drawing.Point(149, 236);
            this.lnk365.Name = "lnk365";
            this.lnk365.Size = new System.Drawing.Size(58, 13);
            this.lnk365.TabIndex = 8;
            this.lnk365.TabStop = true;
            this.lnk365.Text = "Last 365 d";
            this.lnk365.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRange_LinkClicked);

            this.btnPreflight.Location = new System.Drawing.Point(11, 256);
            this.btnPreflight.Name = "btnPreflight";
            this.btnPreflight.Size = new System.Drawing.Size(130, 24);
            this.btnPreflight.TabIndex = 9;
            this.btnPreflight.Text = "Estimate cost";
            this.btnPreflight.UseVisualStyleBackColor = true;
            this.btnPreflight.Click += new System.EventHandler(this.btnPreflight_Click);

            this.chkForceRefresh.AutoSize = true;
            this.chkForceRefresh.Location = new System.Drawing.Point(159, 261);
            this.chkForceRefresh.Name = "chkForceRefresh";
            this.chkForceRefresh.Size = new System.Drawing.Size(94, 17);
            this.chkForceRefresh.TabIndex = 10;
            this.chkForceRefresh.Text = "Force refresh";
            this.chkForceRefresh.UseVisualStyleBackColor = true;

            this.lblEstimate.Location = new System.Drawing.Point(8, 286);
            this.lblEstimate.Name = "lblEstimate";
            this.lblEstimate.Size = new System.Drawing.Size(281, 38);
            this.lblEstimate.TabIndex = 11;
            this.lblEstimate.Text = "Defaults to the last 30 days. Estimate before widening.";

            // grpFilters
            this.grpFilters.Controls.Add(this.lblUser);
            this.grpFilters.Controls.Add(this.cboUser);
            this.grpFilters.Controls.Add(this.lblAction);
            this.grpFilters.Controls.Add(this.cboAction);
            this.grpFilters.Controls.Add(this.lblOperation);
            this.grpFilters.Controls.Add(this.cboOperation);
            this.grpFilters.Controls.Add(this.lblRecord);
            this.grpFilters.Controls.Add(this.txtRecord);
            this.grpFilters.Controls.Add(this.lblFields);
            this.grpFilters.Controls.Add(this.clbFields);
            this.grpFilters.Controls.Add(this.btnSearch);
            this.grpFilters.Controls.Add(this.btnResetFilters);
            this.grpFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpFilters.Location = new System.Drawing.Point(6, 336);
            this.grpFilters.Name = "grpFilters";
            this.grpFilters.Size = new System.Drawing.Size(300, 340);
            this.grpFilters.TabIndex = 1;
            this.grpFilters.TabStop = false;
            this.grpFilters.Text = "Filters";

            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(8, 22);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(63, 13);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "Changed by";

            this.cboUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUser.Location = new System.Drawing.Point(11, 38);
            this.cboUser.Name = "cboUser";
            this.cboUser.Size = new System.Drawing.Size(278, 21);
            this.cboUser.TabIndex = 1;

            this.lblAction.AutoSize = true;
            this.lblAction.Location = new System.Drawing.Point(8, 66);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(35, 13);
            this.lblAction.TabIndex = 2;
            this.lblAction.Text = "Event";

            this.cboAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAction.Location = new System.Drawing.Point(11, 82);
            this.cboAction.Name = "cboAction";
            this.cboAction.Size = new System.Drawing.Size(278, 21);
            this.cboAction.TabIndex = 3;

            this.lblOperation.AutoSize = true;
            this.lblOperation.Location = new System.Drawing.Point(8, 110);
            this.lblOperation.Name = "lblOperation";
            this.lblOperation.Size = new System.Drawing.Size(53, 13);
            this.lblOperation.TabIndex = 4;
            this.lblOperation.Text = "Operation";

            this.cboOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOperation.Location = new System.Drawing.Point(11, 126);
            this.cboOperation.Name = "cboOperation";
            this.cboOperation.Size = new System.Drawing.Size(278, 21);
            this.cboOperation.TabIndex = 5;

            this.lblRecord.AutoSize = true;
            this.lblRecord.Location = new System.Drawing.Point(8, 154);
            this.lblRecord.Name = "lblRecord";
            this.lblRecord.Size = new System.Drawing.Size(120, 13);
            this.lblRecord.TabIndex = 6;
            this.lblRecord.Text = "Record name contains";

            this.txtRecord.Location = new System.Drawing.Point(11, 170);
            this.txtRecord.Name = "txtRecord";
            this.txtRecord.Size = new System.Drawing.Size(278, 20);
            this.txtRecord.TabIndex = 7;

            this.lblFields.AutoSize = true;
            this.lblFields.Location = new System.Drawing.Point(8, 198);
            this.lblFields.Name = "lblFields";
            this.lblFields.Size = new System.Drawing.Size(78, 13);
            this.lblFields.TabIndex = 8;
            this.lblFields.Text = "Changed fields";

            this.clbFields.CheckOnClick = true;
            this.clbFields.IntegralHeight = false;
            this.clbFields.Location = new System.Drawing.Point(11, 214);
            this.clbFields.Name = "clbFields";
            this.clbFields.Size = new System.Drawing.Size(278, 88);
            this.clbFields.TabIndex = 9;

            this.btnSearch.Location = new System.Drawing.Point(11, 308);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(130, 24);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "Search cache";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);

            this.btnResetFilters.Location = new System.Drawing.Point(159, 308);
            this.btnResetFilters.Name = "btnResetFilters";
            this.btnResetFilters.Size = new System.Drawing.Size(130, 24);
            this.btnResetFilters.TabIndex = 11;
            this.btnResetFilters.Text = "Reset filters";
            this.btnResetFilters.UseVisualStyleBackColor = true;
            this.btnResetFilters.Click += new System.EventHandler(this.btnResetFilters_Click);

            // pnlResultHeader
            this.pnlResultHeader.Controls.Add(this.lblResults);
            this.pnlResultHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlResultHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlResultHeader.Name = "pnlResultHeader";
            this.pnlResultHeader.Size = new System.Drawing.Size(776, 24);
            this.pnlResultHeader.TabIndex = 0;

            this.lblResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResults.Location = new System.Drawing.Point(0, 0);
            this.lblResults.Name = "lblResults";
            this.lblResults.Padding = new System.Windows.Forms.Padding(6, 5, 0, 0);
            this.lblResults.Size = new System.Drawing.Size(776, 24);
            this.lblResults.TabIndex = 0;
            this.lblResults.Text = "Connect, choose tables, then sync.";

            // grid
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToOrderColumns = true;
            this.grid.AllowUserToResizeRows = false;
            this.grid.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.grid.Location = new System.Drawing.Point(0, 24);
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(776, 336);
            this.grid.TabIndex = 1;
            this.grid.VirtualMode = true;
            this.grid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.grid_CellValueNeeded);
            this.grid.SelectionChanged += new System.EventHandler(this.grid_SelectionChanged);
            this.grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);

            // pnlDetail
            this.pnlDetail.Controls.Add(this.gridDetail);
            this.pnlDetail.Controls.Add(this.lblNarrative);
            this.pnlDetail.Controls.Add(this.lblDetailHeader);
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.Location = new System.Drawing.Point(0, 0);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Size = new System.Drawing.Size(776, 233);
            this.pnlDetail.TabIndex = 0;

            this.lblDetailHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailHeader.Location = new System.Drawing.Point(0, 0);
            this.lblDetailHeader.Name = "lblDetailHeader";
            this.lblDetailHeader.Padding = new System.Windows.Forms.Padding(6, 5, 0, 0);
            this.lblDetailHeader.Size = new System.Drawing.Size(776, 24);
            this.lblDetailHeader.TabIndex = 0;
            this.lblDetailHeader.Text = "Select a row to load its old and new values.";

            this.lblNarrative.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblNarrative.Location = new System.Drawing.Point(0, 213);
            this.lblNarrative.Name = "lblNarrative";
            this.lblNarrative.Padding = new System.Windows.Forms.Padding(6, 3, 0, 0);
            this.lblNarrative.Size = new System.Drawing.Size(776, 20);
            this.lblNarrative.TabIndex = 1;

            this.gridDetail.AllowUserToAddRows = false;
            this.gridDetail.AllowUserToDeleteRows = false;
            this.gridDetail.AllowUserToResizeRows = false;
            this.gridDetail.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDetail.Location = new System.Drawing.Point(0, 24);
            this.gridDetail.Name = "gridDetail";
            this.gridDetail.ReadOnly = true;
            this.gridDetail.RowHeadersVisible = false;
            this.gridDetail.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridDetail.Size = new System.Drawing.Size(776, 189);
            this.gridDetail.TabIndex = 2;

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.progress, this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 622);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip.TabIndex = 2;

            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(160, 16);
            this.progress.Visible = false;

            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Not connected.";

            // PanopticonControl
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitOuter);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Name = "PanopticonControl";
            this.Size = new System.Drawing.Size(1100, 644);
            this.Load += new System.EventHandler(this.PanopticonControl_Load);

            this.splitOuter.Panel1.ResumeLayout(false);
            this.splitOuter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitOuter)).EndInit();
            this.splitOuter.ResumeLayout(false);
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            this.pnlSide.ResumeLayout(false);
            this.grpFilters.ResumeLayout(false);
            this.grpFilters.PerformLayout();
            this.grpScope.ResumeLayout(false);
            this.grpScope.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.pnlResultHeader.ResumeLayout(false);
            this.pnlDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridDetail)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
