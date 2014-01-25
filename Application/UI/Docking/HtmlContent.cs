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

        public HtmlRenderer.HtmlPanel Html
        {
            get
            {
                return this.htmlInfo;
            }
        }

        public void UpdateRecord(BaseRecord sc)
        {
            if (sc == null)
            {
                this.UpdateText(string.Empty);
                return;
            }

            FontLangInfo defLang;
            if (!Encoding.TryGetFontInfo(Settings.Default.LocalizationName, out defLang))
            {
                defLang = new FontLangInfo(1252, 1033, 0);
            }

            //var writer = new System.Web.UI.HtmlTextWriter();
            
            //var rb = new System.Web.UI;
            //var rb = new RTFBuilder(RTFFont.Arial, 16, defLang.lcid, defLang.charset);
            //sc.GetFormattedHeader(rb);
            //sc.GetFormattedData(rb);
            //this.rtfInfo.Text = rb.ToString();
        }

        public void UpdateText(string text)
        {
            this.htmlInfo.Text = text;
        }
    }
}
