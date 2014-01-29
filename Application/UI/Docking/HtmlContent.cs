using System;
using TESVSnip.UI.Rendering.Extensions;

namespace TESVSnip.UI.Docking
{
    using System.Windows.Forms;

    using RTF;

    using TESVSnip.Domain.Model;
    using TESVSnip.Framework.Services;
    using TESVSnip.Properties;

    public partial class HtmlContent : BaseDockContent
    {
        public HtmlContent()
        {
            this.InitializeComponent();
        }

        public void UpdateRecord(BaseRecord sc)
        {
            if (sc == null)
            {
                this.UpdateText(string.Empty);
                return;
            }

            FontLangInfo defLang;
            if (!Encoding.TryGetFontInfo(Domain.Properties.Settings.Default.LocalizationName, out defLang))
            {
                defLang = new FontLangInfo(1252, 1033, 0);
            }

            //var writer = new System.Web.UI.HtmlTextWriter();
            
            //var rb = new System.Web.UI;
            //var rb = new RTFBuilder(RTFFont.Arial, 16, defLang.lcid, defLang.charset);
            //sc.GetFormattedHeader(rb);
            //sc.GetFormattedData(rb);
            //this.rtfInfo.Text = rb.ToString();
            //this.webBrowser1.DocumentText = text;
        }

        public void UpdateText(string text)
        {
            this.webBrowser1.DocumentText = text;
            //this.htmlInfo.Text = text;
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
    }

    //public class LinkClickedEventArgs : EventArgs
    //{
    //    public Uri Url { get; set; }
    //}
}
