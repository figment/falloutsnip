namespace TESVSnip.Forms
{
    partial class CompressSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.listRecordFilter = new System.Windows.Forms.CheckedListBox();
            this.btnToggleAll = new System.Windows.Forms.Button();
            this.rdoNeverCompressRecords = new System.Windows.Forms.RadioButton();
            this.rdoDefaultCompressRecords = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.chkEnableAutoCompress = new System.Windows.Forms.CheckBox();
            this.grpCompSettings = new System.Windows.Forms.GroupBox();
            this.txtCompressLimit = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkEnableCompressLimit = new System.Windows.Forms.CheckBox();
            this.grpCompSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Image = global::TESVSnip.Properties.Resources.fileexport;
            this.btnOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOk.Location = new System.Drawing.Point(447, 379);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(70, 26);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Save";
            this.btnOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::TESVSnip.Properties.Resources.cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(520, 379);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // listRecordFilter
            // 
            this.listRecordFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listRecordFilter.CheckOnClick = true;
            this.listRecordFilter.ColumnWidth = 75;
            this.listRecordFilter.Location = new System.Drawing.Point(6, 46);
            this.listRecordFilter.MultiColumn = true;
            this.listRecordFilter.Name = "listRecordFilter";
            this.listRecordFilter.Size = new System.Drawing.Size(564, 199);
            this.listRecordFilter.TabIndex = 7;
            // 
            // btnToggleAll
            // 
            this.btnToggleAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleAll.Location = new System.Drawing.Point(495, 17);
            this.btnToggleAll.Name = "btnToggleAll";
            this.btnToggleAll.Size = new System.Drawing.Size(75, 23);
            this.btnToggleAll.TabIndex = 8;
            this.btnToggleAll.Text = "Toggle All";
            this.btnToggleAll.UseVisualStyleBackColor = true;
            this.btnToggleAll.Click += new System.EventHandler(this.btnToggleAll_Click);
            // 
            // rdoNeverCompressRecords
            // 
            this.rdoNeverCompressRecords.AutoSize = true;
            this.rdoNeverCompressRecords.Location = new System.Drawing.Point(12, 41);
            this.rdoNeverCompressRecords.Name = "rdoNeverCompressRecords";
            this.rdoNeverCompressRecords.Size = new System.Drawing.Size(227, 17);
            this.rdoNeverCompressRecords.TabIndex = 10;
            this.rdoNeverCompressRecords.TabStop = true;
            this.rdoNeverCompressRecords.Text = "Never Compress Records (Historic Default)";
            this.rdoNeverCompressRecords.UseVisualStyleBackColor = true;
            this.rdoNeverCompressRecords.CheckedChanged += new System.EventHandler(this.rdoNeverCompressRecords_CheckedChanged);
            // 
            // rdoDefaultCompressRecords
            // 
            this.rdoDefaultCompressRecords.AutoSize = true;
            this.rdoDefaultCompressRecords.Location = new System.Drawing.Point(12, 64);
            this.rdoDefaultCompressRecords.Name = "rdoDefaultCompressRecords";
            this.rdoDefaultCompressRecords.Size = new System.Drawing.Size(343, 17);
            this.rdoDefaultCompressRecords.TabIndex = 11;
            this.rdoDefaultCompressRecords.TabStop = true;
            this.rdoDefaultCompressRecords.Text = "Use Default Compression (i.e. Compression Setting in Record Flags)";
            this.rdoDefaultCompressRecords.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(324, 25);
            this.label1.TabIndex = 12;
            this.label1.Text = "Record Compression Settings";
            // 
            // chkEnableAutoCompress
            // 
            this.chkEnableAutoCompress.AutoSize = true;
            this.chkEnableAutoCompress.Location = new System.Drawing.Point(6, 23);
            this.chkEnableAutoCompress.Name = "chkEnableAutoCompress";
            this.chkEnableAutoCompress.Size = new System.Drawing.Size(175, 17);
            this.chkEnableAutoCompress.TabIndex = 13;
            this.chkEnableAutoCompress.Text = "Enable Auto Compress By Type";
            this.chkEnableAutoCompress.UseVisualStyleBackColor = true;
            this.chkEnableAutoCompress.CheckedChanged += new System.EventHandler(this.chkEnableAutoCompress_CheckedChanged);
            // 
            // grpCompSettings
            // 
            this.grpCompSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCompSettings.Controls.Add(this.label2);
            this.grpCompSettings.Controls.Add(this.txtCompressLimit);
            this.grpCompSettings.Controls.Add(this.listRecordFilter);
            this.grpCompSettings.Controls.Add(this.chkEnableCompressLimit);
            this.grpCompSettings.Controls.Add(this.chkEnableAutoCompress);
            this.grpCompSettings.Controls.Add(this.btnToggleAll);
            this.grpCompSettings.Location = new System.Drawing.Point(18, 87);
            this.grpCompSettings.Name = "grpCompSettings";
            this.grpCompSettings.Size = new System.Drawing.Size(576, 283);
            this.grpCompSettings.TabIndex = 14;
            this.grpCompSettings.TabStop = false;
            this.grpCompSettings.Text = "Compression Settings";
            // 
            // txtCompressLimit
            // 
            this.txtCompressLimit.Location = new System.Drawing.Point(146, 258);
            this.txtCompressLimit.Mask = "000000";
            this.txtCompressLimit.Name = "txtCompressLimit";
            this.txtCompressLimit.PromptChar = ' ';
            this.txtCompressLimit.Size = new System.Drawing.Size(50, 20);
            this.txtCompressLimit.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(202, 261);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "bytes";
            // 
            // chkEnableCompressLimit
            // 
            this.chkEnableCompressLimit.AutoSize = true;
            this.chkEnableCompressLimit.Location = new System.Drawing.Point(6, 260);
            this.chkEnableCompressLimit.Name = "chkEnableCompressLimit";
            this.chkEnableCompressLimit.Size = new System.Drawing.Size(134, 17);
            this.chkEnableCompressLimit.TabIndex = 13;
            this.chkEnableCompressLimit.Text = "Compress records over";
            this.chkEnableCompressLimit.UseVisualStyleBackColor = true;
            this.chkEnableCompressLimit.CheckedChanged += new System.EventHandler(this.chkEnableCompressLimit_CheckedChanged);
            // 
            // CompressSettings
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(618, 417);
            this.Controls.Add(this.grpCompSettings);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rdoDefaultCompressRecords);
            this.Controls.Add(this.rdoNeverCompressRecords);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "CompressSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Record Compression Settings";
            this.Load += new System.EventHandler(this.LoadSettings_Load);
            this.grpCompSettings.ResumeLayout(false);
            this.grpCompSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckedListBox listRecordFilter;
        private System.Windows.Forms.Button btnToggleAll;
        private System.Windows.Forms.RadioButton rdoNeverCompressRecords;
        private System.Windows.Forms.RadioButton rdoDefaultCompressRecords;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkEnableAutoCompress;
        private System.Windows.Forms.GroupBox grpCompSettings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox txtCompressLimit;
        private System.Windows.Forms.CheckBox chkEnableCompressLimit;

    }
}