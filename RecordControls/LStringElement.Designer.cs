namespace TESVSnip.RecordControls
{
    partial class LStringElement
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
            this.chkUseText = new System.Windows.Forms.CheckBox();
            this.txtString = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            this.SuspendLayout();
            // 
            // chkUseText
            // 
            this.chkUseText.AutoSize = true;
            this.chkUseText.Location = new System.Drawing.Point(26, 30);
            this.chkUseText.Name = "chkUseText";
            this.chkUseText.Size = new System.Drawing.Size(15, 14);
            this.chkUseText.TabIndex = 5;
            this.chkUseText.UseVisualStyleBackColor = true;
            // 
            // txtString
            // 
            this.txtString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtString.Location = new System.Drawing.Point(66, 27);
            this.txtString.Name = "txtString";
            this.txtString.Size = new System.Drawing.Size(331, 20);
            this.txtString.TabIndex = 6;
            this.txtString.Validated += new System.EventHandler(this.txtString_Validated);
            // 
            // LStringElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtString);
            this.Controls.Add(this.chkUseText);
            this.MinimumSize = new System.Drawing.Size(200, 48);
            this.Name = "LStringElement";
            this.Size = new System.Drawing.Size(400, 48);
            this.Controls.SetChildIndex(this.chkUseText, 0);
            this.Controls.SetChildIndex(this.txtString, 0);
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUseText;
        private System.Windows.Forms.TextBox txtString;

    }
}
