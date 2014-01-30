using System;
using TESVSnip.UI.Rendering.Extensions;

namespace TESVSnip.UI.Docking
{
    using System.Windows.Forms;

    using TESVSnip.Domain.Model;
    using TESVSnip.Framework.Services;
    using TESVSnip.Properties;

    public partial class HtmlContent : BaseDockContent
    {
        public HtmlContent()
        {
            this.InitializeComponent();
            this.webBrowser1.DocumentText = "";
        }

        public void UpdateRecord(BaseRecord record)
        {
            if (record == null)
            {
                this.UpdateText(string.Empty);
                return;
            }

            try
            {
                string html = TESVSnip.UI.Rendering.HtmlRenderer.GetDescription(record);
                UpdateText(html);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.WarningText);
            }
        }

        public void UpdateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                this.webBrowser1.DocumentText = "";
            else
                this.webBrowser1.DocumentText = text;
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString() == "about:blank") return;
            e.Cancel = true;
            if (OnLinkClicked != null)
            {
                OnLinkClicked(this, new LinkClickedEventArgs(e.Url.ToString()));
            }
        }

        public event EventHandler<LinkClickedEventArgs> OnLinkClicked;

        private void asHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Clipboard.SetText(this.webBrowser1.DocumentText, System.Windows.TextDataFormat.Html);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = this.webBrowser1.Document;
            if (doc != null)
            {
                doc.ExecCommand("copy", false, null);
            }
            //System.Windows.Clipboard.SetText(this.webBrowser1.DocumentText, System.Windows.TextDataFormat.Html);
        }

        private void webBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && !e.Alt && !e.Shift && e.KeyCode == Keys.C)
            {
                var doc = this.webBrowser1.Document;
                if (doc != null)
                    doc.ExecCommand("copy", false, null);
            }
        }
    }

    //public class LinkClickedEventArgs : EventArgs
    //{
    //    public Uri Url { get; set; }
    //}
}
