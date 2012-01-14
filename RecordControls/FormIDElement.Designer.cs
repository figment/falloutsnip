namespace TESVSnip.RecordControls
{
    partial class FormIDElement
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
            this.cboFormID = new System.Windows.Forms.ComboBox();
            this.cboRecType = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            this.SuspendLayout();
            // 
            // cboFormID
            // 
            this.cboFormID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFormID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFormID.Location = new System.Drawing.Point(69, 23);
            this.cboFormID.Name = "cboFormID";
            this.cboFormID.Size = new System.Drawing.Size(328, 21);
            this.cboFormID.TabIndex = 3;
            this.cboFormID.SelectedIndexChanged += new System.EventHandler(this.cboFormID_SelectedIndexChanged);
            // 
            // cboRecType
            // 
            this.cboRecType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRecType.FormattingEnabled = true;
            this.cboRecType.Location = new System.Drawing.Point(3, 23);
            this.cboRecType.Name = "cboRecType";
            this.cboRecType.Size = new System.Drawing.Size(60, 21);
            this.cboRecType.TabIndex = 2;
            this.cboRecType.SelectedIndexChanged += new System.EventHandler(this.cboRecType_SelectedIndexChanged);
            // 
            // FormIDElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboFormID);
            this.Controls.Add(this.cboRecType);
            this.MinimumSize = new System.Drawing.Size(200, 48);
            this.Name = "FormIDElement";
            this.Size = new System.Drawing.Size(400, 48);
            this.SizeChanged += new System.EventHandler(this.FormIDElement_SizeChanged);
            this.Controls.SetChildIndex(this.cboRecType, 0);
            this.Controls.SetChildIndex(this.cboFormID, 0);
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboFormID;
        private System.Windows.Forms.ComboBox cboRecType;
    }
}
