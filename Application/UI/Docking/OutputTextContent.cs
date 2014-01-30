using System.Text.RegularExpressions;

namespace TESVSnip.UI.Docking
{
    using System.Windows.Forms;

    using TESVSnip.Domain.Model;
    using TESVSnip.Framework.Services;
    using TESVSnip.Properties;

    public partial class OutputTextContent : BaseDockContent
    {
        System.Text.RegularExpressions.Regex reWhiteSpace = new Regex("\r?\n");
        public OutputTextContent()
        {
            this.InitializeComponent();
        }

        public System.Windows.Forms.TextBox TextBox
        {
            get
            {
                return this.textBox;
            }
        }

        public void UpdateText(string text)
        {
            this.textBox.Text = reWhiteSpace.Replace(text, "\r\n");
        }

        public void SetText(string text)
        {
            this.textBox.Text = reWhiteSpace.Replace(text, "\r\n");
        }

        public void ClearText()
        {
            this.textBox.Text = "";
        }

        public void AppendText(string text)
        {
            this.textBox.AppendText( reWhiteSpace.Replace(text, "\r\n") );
        }
    }
}
