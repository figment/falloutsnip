using System.Windows.Forms;
namespace TESVSnip.Docking
{
    public partial class RichTextContent : BaseDockContent
    {
        public RichTextContent()
        {
            InitializeComponent();
        }

        public void UpdateRtf(string rtfText)
        {
            this.rtfInfo.Rtf = rtfText;
        }

        public void UpdateText(string text)
        {
            this.rtfInfo.Text = text;
        }

        public RichTextBox RtfInfo
        {
            get { return this.rtfInfo; }
        }
    }
}