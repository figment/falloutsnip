using TESVSnip.Windows.Controls;
namespace TESVSnip.RecordControls
{
    partial class FlagsElement
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
            this.cboFlags = new TESVSnip.Windows.Controls.FlagComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            this.SuspendLayout();
            // 
            // cboFlags
            // 
            this.cboFlags.AllowResizeDropDown = true;
            this.cboFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFlags.CheckOnClick = true;
            this.cboFlags.ControlSize = new System.Drawing.Size(47, 16);
            this.cboFlags.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cboFlags.DropDownSizeMode = TESVSnip.Windows.Controls.CustomComboBox.SizeMode.UseDropDownSize;
            this.cboFlags.DropSize = new System.Drawing.Size(121, 106);
            this.cboFlags.FormattingEnabled = true;
            this.cboFlags.Location = new System.Drawing.Point(66, 25);
            this.cboFlags.Name = "cboFlags";
            this.cboFlags.Size = new System.Drawing.Size(331, 21);
            this.cboFlags.TabIndex = 2;
            this.cboFlags.ValueSeparator = ",";
            this.cboFlags.TextUpdate += new System.EventHandler(this.cboFlags_TextUpdate);
            // 
            // FlagsElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboFlags);
            this.MinimumSize = new System.Drawing.Size(200, 48);
            this.Name = "FlagsElement";
            this.Size = new System.Drawing.Size(400, 48);
            this.Controls.SetChildIndex(this.cboFlags, 0);
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FlagComboBox cboFlags;


    }
}
