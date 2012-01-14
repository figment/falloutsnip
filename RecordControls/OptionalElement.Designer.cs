namespace TESVSnip.RecordControls
{
    partial class OptionalElement
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
            this.chkUseValue = new System.Windows.Forms.CheckBox();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // chkUseValue
            // 
            this.chkUseValue.AutoSize = true;
            this.chkUseValue.Location = new System.Drawing.Point(4, 4);
            this.chkUseValue.Name = "chkUseValue";
            this.chkUseValue.Size = new System.Drawing.Size(99, 17);
            this.chkUseValue.TabIndex = 0;
            this.chkUseValue.Text = "Use this value?";
            this.chkUseValue.UseVisualStyleBackColor = true;
            this.chkUseValue.CheckedChanged += new System.EventHandler(this.chkUseValue_CheckedChanged);
            // 
            // controlPanel
            // 
            this.controlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.controlPanel.Location = new System.Drawing.Point(0, 21);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(400, 25);
            this.controlPanel.TabIndex = 1;
            // 
            // OptionalElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.chkUseValue);
            this.MinimumSize = new System.Drawing.Size(250, 46);
            this.Name = "OptionalElement";
            this.Size = new System.Drawing.Size(400, 46);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUseValue;
        private System.Windows.Forms.Panel controlPanel;
    }
}
