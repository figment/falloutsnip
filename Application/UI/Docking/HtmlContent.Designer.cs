namespace TESVSnip.UI.Docking
{
    partial class HtmlContent
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
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.htmlInfo = new HtmlRenderer.HtmlPanel();
            this.SuspendLayout();
            // 
            // htmlInfo
            // 
            this.htmlInfo.AutoScroll = true;
            this.htmlInfo.AvoidGeometryAntialias = false;
            this.htmlInfo.AvoidImagesLateLoading = false;
            this.htmlInfo.BackColor = System.Drawing.SystemColors.Window;
            this.htmlInfo.BaseStylesheet = null;
            this.htmlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlInfo.Location = new System.Drawing.Point(0, 0);
            this.htmlInfo.Name = "htmlInfo";
            this.htmlInfo.Size = new System.Drawing.Size(376, 504);
            this.htmlInfo.TabIndex = 4;
            // 
            // HtmlContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 504);
            this.ControlBox = false;
            this.Controls.Add(this.htmlInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HtmlContent";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ShowInTaskbar = false;
            this.Text = "Report";
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlRenderer.HtmlPanel htmlInfo;
    }
}