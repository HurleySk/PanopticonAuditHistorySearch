namespace MyXrmToolBoxPlugin
{
    partial class MyPluginControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnDoSomething = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(12, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(576, 23);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Not connected. Use the connection button above to connect.";
            //
            // btnDoSomething
            //
            this.btnDoSomething.Enabled = false;
            this.btnDoSomething.Location = new System.Drawing.Point(12, 48);
            this.btnDoSomething.Name = "btnDoSomething";
            this.btnDoSomething.Size = new System.Drawing.Size(150, 30);
            this.btnDoSomething.TabIndex = 1;
            this.btnDoSomething.Text = "Do Something";
            this.btnDoSomething.UseVisualStyleBackColor = true;
            this.btnDoSomething.Click += new System.EventHandler(this.btnDoSomething_Click);
            //
            // txtResult
            //
            this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResult.Location = new System.Drawing.Point(12, 94);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(576, 306);
            this.txtResult.TabIndex = 2;
            //
            // MyPluginControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnDoSomething);
            this.Controls.Add(this.lblStatus);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(600, 412);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnDoSomething;
        private System.Windows.Forms.TextBox txtResult;
    }
}
