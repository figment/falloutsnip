using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.Forms;

namespace FalloutSnip.UI.Hosting
{
    public static class ScriptSupport
    {
        public static void SendStatusText(string text)
        {
            FalloutSnip.UI.Forms.MainView.PostStatusText(text);
        }

        public static void SendStatusText(string text, Color color)
        {
            FalloutSnip.UI.Forms.MainView.PostStatusText(text, color);
        }

        public static OutputTextContent CreateTextWindow(string name)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault() as MainView;
            if (form != null)
            {
                return form.GetOrCreateWindowByName<OutputTextContent>(name);
            }
            return null;
        }

        public static FalloutSnip.Domain.Model.Plugin CreateNewPlugin()
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault() as MainView;
            if (form != null)
            {
                return form.NewPlugin();
            }
            return null;
        }
    }
}
