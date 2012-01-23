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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlagsElement));
            this.cboFlags = new TESVSnip.Windows.Controls.FlagComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            this.SuspendLayout();
            // 
            // Error
            // 
            resources.ApplyResources(this.Error, "Error");
            // 
            // cboFlags
            // 
            resources.ApplyResources(this.cboFlags, "cboFlags");
            this.cboFlags.AllowResizeDropDown = true;
            this.cboFlags.CheckOnClick = true;
            this.cboFlags.ControlSize = new System.Drawing.Size(47, 16);
            this.cboFlags.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cboFlags.DropDownSizeMode = TESVSnip.Windows.Controls.CustomComboBox.SizeMode.UseDropDownSize;
            this.cboFlags.DropSize = new System.Drawing.Size(121, 106);
            this.Error.SetError(this.cboFlags, resources.GetString("cboFlags.Error"));
            this.cboFlags.FormattingEnabled = true;
            this.Error.SetIconAlignment(this.cboFlags, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cboFlags.IconAlignment"))));
            this.Error.SetIconPadding(this.cboFlags, ((int)(resources.GetObject("cboFlags.IconPadding"))));
            this.cboFlags.Name = "cboFlags";
            this.cboFlags.ValueSeparator = ",";
            this.cboFlags.TextUpdate += new System.EventHandler(this.cboFlags_TextUpdate);
            // 
            // FlagsElement
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboFlags);
            this.Error.SetError(this, resources.GetString("$this.Error"));
            this.Error.SetIconAlignment(this, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("$this.IconAlignment"))));
            this.Error.SetIconPadding(this, ((int)(resources.GetObject("$this.IconPadding"))));
            this.MinimumSize = new System.Drawing.Size(200, 48);
            this.Name = "FlagsElement";
            this.Controls.SetChildIndex(this.cboFlags, 0);
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FlagComboBox cboFlags;


    }
}
