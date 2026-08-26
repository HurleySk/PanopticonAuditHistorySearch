using System;
using System.Windows.Forms;

namespace PanopticonAuditHistorySearch.UI
{
    public class ExportOptionsForm : Form
    {
        private readonly int _availableRows;
        private readonly int _valueWarningThreshold;

        private readonly NumericUpDown _limit;
        private readonly CheckBox _includeValues;
        private readonly Label _cost;
        private readonly TextBox _path;

        public int RowLimit { get; private set; }
        public bool IncludeValues { get; private set; }
        public string TargetPath { get; private set; }

        public ExportOptionsForm(int availableRows, int valueWarningThreshold)
        {
            _availableRows = availableRows;
            _valueWarningThreshold = valueWarningThreshold;

            Text = "Export to CSV";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new System.Drawing.Size(470, 232);

            Controls.Add(new Label
            {
                Text = "Rows to export (most recent first)",
                Location = new System.Drawing.Point(12, 15),
                AutoSize = true
            });

            _limit = new NumericUpDown
            {
                Location = new System.Drawing.Point(12, 34),
                Width = 120,
                Minimum = 1,
                Maximum = Math.Max(1, availableRows),
                Value = Math.Max(1, availableRows),
                ThousandsSeparator = true
            };
            _limit.ValueChanged += (s, e) => UpdateCost();
            Controls.Add(_limit);

            Controls.Add(new Label
            {
                Text = string.Format("of {0:N0} available", availableRows),
                Location = new System.Drawing.Point(140, 38),
                AutoSize = true
            });

            _includeValues = new CheckBox
            {
                Text = "Include old and new values",
                Location = new System.Drawing.Point(12, 68),
                AutoSize = true
            };
            _includeValues.CheckedChanged += (s, e) => UpdateCost();
            Controls.Add(_includeValues);

            _cost = new Label
            {
                Location = new System.Drawing.Point(12, 92),
                Size = new System.Drawing.Size(446, 46)
            };
            Controls.Add(_cost);

            Controls.Add(new Label
            {
                Text = "Save to",
                Location = new System.Drawing.Point(12, 144),
                AutoSize = true
            });

            _path = new TextBox
            {
                Location = new System.Drawing.Point(12, 162),
                Width = 360,
                Text = DefaultPath()
            };
            Controls.Add(_path);

            var browse = new Button
            {
                Text = "...",
                Location = new System.Drawing.Point(380, 160),
                Width = 40
            };
            browse.Click += Browse_Click;
            Controls.Add(browse);

            var ok = new Button
            {
                Text = "Export",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(280, 196),
                Width = 84
            };
            ok.Click += Ok_Click;
            Controls.Add(ok);

            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(372, 196),
                Width = 84
            };
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
            UpdateCost();
        }

        private static string DefaultPath()
        {
            var name = string.Format("audit-{0:yyyyMMdd-HHmmss}.csv", DateTime.Now);
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), name);
        }

        private void UpdateCost()
        {
            var rows = (int)_limit.Value;

            if (!_includeValues.Checked)
            {
                _cost.Text = string.Format(
                    "{0:N0} row(s) will be written from the local cache. No Dataverse calls.", rows);
                _cost.ForeColor = System.Drawing.SystemColors.ControlText;
                return;
            }

            var minutes = rows / 1500.0;
            _cost.Text = string.Format(
                "Values are fetched one audit row at a time. {0:N0} row(s) means up to {0:N0} Dataverse calls, " +
                "roughly {1}. Cached values are reused.",
                rows, minutes < 1 ? "under a minute" : string.Format("{0:N0} minutes", minutes));
            _cost.ForeColor = rows > _valueWarningThreshold
                ? System.Drawing.Color.Firebrick
                : System.Drawing.SystemColors.ControlText;
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = System.IO.Path.GetFileName(_path.Text),
                InitialDirectory = System.IO.Path.GetDirectoryName(_path.Text)
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) _path.Text = dialog.FileName;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_path.Text))
            {
                MessageBox.Show("Choose a destination file.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
                return;
            }

            var rows = (int)_limit.Value;
            if (_includeValues.Checked && rows > _valueWarningThreshold)
            {
                var answer = MessageBox.Show(
                    string.Format(
                        "Fetching values for {0:N0} rows will issue about that many Dataverse calls and may run " +
                        "into service protection limits.\r\n\r\nContinue?", rows),
                    "Large value export", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) { DialogResult = DialogResult.None; return; }
            }

            RowLimit = rows;
            IncludeValues = _includeValues.Checked;
            TargetPath = _path.Text.Trim();
        }
    }
}
