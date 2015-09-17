using FalloutSnip.UI.Rendering.Extensions;

namespace FalloutSnip.UI.Docking
{
    using System.Windows.Forms;

    using RTF;

    using FalloutSnip.Domain.Model;
    using FalloutSnip.Framework.Services;
    using FalloutSnip.Properties;

    public partial class RichTextContent : BaseDockContent
    {
        public RichTextContent()
        {
            this.InitializeComponent();
        }

        public RichTextBox RtfInfo
        {
            get
            {
                return this.rtfInfo;
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
            if (!Encoding.TryGetFontInfo(Domain.Properties.Settings.Default.LocalizationName, out defLang))
            {
                defLang = new FontLangInfo(1252, 1033, 0);
            }

            var rb = new RTFBuilder(RTFFont.Arial, 16, defLang.lcid, defLang.charset);
            sc.GetFormattedHeader(rb);
            sc.GetFormattedData(rb);
            this.rtfInfo.Rtf = rb.ToString();
        }

        public void UpdateRtf(string rtfText)
        {
            this.rtfInfo.Rtf = rtfText;
        }

        public void UpdateText(string text)
        {
            this.rtfInfo.Text = text;
        }
    }
}
