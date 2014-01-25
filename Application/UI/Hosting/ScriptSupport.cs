using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESVSnip.UI.Docking;
using TESVSnip.UI.Forms;

namespace TESVSnip.UI.Hosting
{
    public static class ScriptSupport
    {
        public static void SendStatusText(string text)
        {
            TESVSnip.UI.Forms.MainView.PostStatusText(text);
        }

        public static void SendStatusText(string text, Color color)
        {
            TESVSnip.UI.Forms.MainView.PostStatusText(text, color);
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

        public static TESVSnip.Domain.Model.Plugin CreateNewPlugin()
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
