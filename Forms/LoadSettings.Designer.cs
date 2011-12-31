namespace TESVSnip.Forms
{
    partial class LoadSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadSettings));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.rtfWarning = new System.Windows.Forms.RichTextBox();
            this.chkApplyToAllESM = new System.Windows.Forms.CheckBox();
            this.chkDontAskAboutFiltering = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.listRecordFilter = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Image = global::TESVSnip.Properties.Resources.fileexport;
            this.btnOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOk.Location = new System.Drawing.Point(447, 422);
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
            this.btnCancel.Location = new System.Drawing.Point(520, 422);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // rtfWarning
            // 
            this.rtfWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtfWarning.Location = new System.Drawing.Point(13, 13);
            this.rtfWarning.Name = "rtfWarning";
            this.rtfWarning.ReadOnly = true;
            this.rtfWarning.Size = new System.Drawing.Size(593, 96);
            this.rtfWarning.TabIndex = 3;
            this.rtfWarning.Text = resources.GetString("rtfWarning.Text");
            this.rtfWarning.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // chkApplyToAllESM
            // 
            this.chkApplyToAllESM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkApplyToAllESM.AutoSize = true;
            this.chkApplyToAllESM.Location = new System.Drawing.Point(12, 363);
            this.chkApplyToAllESM.Name = "chkApplyToAllESM";
            this.chkApplyToAllESM.Size = new System.Drawing.Size(334, 17);
            this.chkApplyToAllESM.TabIndex = 4;
            this.chkApplyToAllESM.Text = "Apply to all ESM files (otherwise only Skyrim.ESM will be modified)";
            this.chkApplyToAllESM.UseVisualStyleBackColor = true;
            // 
            // chkDontAskAboutFiltering
            // 
            this.chkDontAskAboutFiltering.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkDontAskAboutFiltering.AutoSize = true;
            this.chkDontAskAboutFiltering.Checked = true;
            this.chkDontAskAboutFiltering.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDontAskAboutFiltering.Location = new System.Drawing.Point(12, 386);
            this.chkDontAskAboutFiltering.Name = "chkDontAskAboutFiltering";
            this.chkDontAskAboutFiltering.Size = new System.Drawing.Size(172, 17);
            this.chkDontAskAboutFiltering.TabIndex = 5;
            this.chkDontAskAboutFiltering.Text = "Don\'t Ask About Filtering Again";
            this.chkDontAskAboutFiltering.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 115);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(136, 17);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Enable Record Filtering";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // listRecordFilter
            // 
            this.listRecordFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listRecordFilter.CheckOnClick = true;
            this.listRecordFilter.ColumnWidth = 75;
            this.listRecordFilter.Location = new System.Drawing.Point(12, 138);
            this.listRecordFilter.MultiColumn = true;
            this.listRecordFilter.Name = "listRecordFilter";
            this.listRecordFilter.Size = new System.Drawing.Size(594, 214);
            this.listRecordFilter.TabIndex = 7;
            // 
            // LoadSettings
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(618, 460);
            this.Controls.Add(this.listRecordFilter);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.chkDontAskAboutFiltering);
            this.Controls.Add(this.chkApplyToAllESM);
            this.Controls.Add(this.rtfWarning);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "LoadSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File Load Settings";
            this.Load += new System.EventHandler(this.LoadSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RichTextBox rtfWarning;
        private System.Windows.Forms.CheckBox chkApplyToAllESM;
        private System.Windows.Forms.CheckBox chkDontAskAboutFiltering;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckedListBox listRecordFilter;

    }
}