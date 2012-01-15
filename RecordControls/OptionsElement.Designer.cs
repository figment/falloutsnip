namespace TESVSnip.RecordControls
{
    partial class OptionsElement
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
            this.cboOptions = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            this.SuspendLayout();
            // 
            // cboOptions
            // 
            this.cboOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOptions.FormattingEnabled = true;
            this.cboOptions.Location = new System.Drawing.Point(66, 25);
            this.cboOptions.Name = "cboOptions";
            this.cboOptions.Size = new System.Drawing.Size(331, 21);
            this.cboOptions.TabIndex = 2;
            this.cboOptions.SelectedIndexChanged += new System.EventHandler(this.cboOptions_SelectedIndexChanged);
            this.cboOptions.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboOptions_KeyPress);
            // 
            // OptionsElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboOptions);
            this.MinimumSize = new System.Drawing.Size(200, 48);
            this.Name = "OptionsElement";
            this.Size = new System.Drawing.Size(400, 48);
            this.Controls.SetChildIndex(this.cboOptions, 0);
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboOptions;


    }
}
