using System.Threading;
using FalloutSnip.Domain.Data;
using FalloutSnip.Properties;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.Services;

namespace FalloutSnip.UI.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Media;
    using System.Windows.Forms;

    using Domain.Data.Structure;
    using FalloutSnip.Domain.Model;
    using FalloutSnip.Framework;

    using Settings = FalloutSnip.Properties.Settings;

    /// <summary>
    /// This file contains the incremental search related functionality for the main form.
    /// </summary>
    internal partial class MainView
    {

        internal Dictionary<string, ToolStripMenuItem> languageToolBarItems =
            new Dictionary<string, ToolStripMenuItem>(StringComparer.InvariantCultureIgnoreCase);

        private StringsEditor stringEditor;

        #region String Editor
        private void OpenStringEditor()
        {
            if (stringEditor == null)
            {
                var plugins = PluginList.All.Records.OfType<Plugin>().ToList();
                if (plugins.Count == 0)
                {
                    MessageBox.Show(this, "No plugins available to edit", Resources.ErrorText, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                stringEditor = new StringsEditor();
                stringEditor.FormClosed += delegate { CloseStringEditor(); };
                stringEditor.Plugins = plugins.ToArray();
                stringEditor.Show(this); // modeless. Close if the tree is modified.
            }
        }


        private void UpdateStringEditor()
        {
            if (stringEditor != null)
            {
                var plugins = PluginList.All.Records.OfType<Plugin>().ToList();
                if (plugins.Count == 0)
                {
                    CloseStringEditor();
                }
                else
                {
                    stringEditor.Reload(plugins.ToArray());
                }
            }
        }


        private void CloseStringEditor()
        {
            if (stringEditor != null)
            {
                var editor = stringEditor;
                stringEditor = null;
                try
                {
                    if (!editor.IsDisposed)
                    {
                        editor.Close();
                    }
                }
                catch
                {
                }
            }
        }
        #endregion

        private void InitializeLanguage()
        {
            languageToolBarItems.Add("English", englishToolStripMenuItem);
            languageToolBarItems.Add("Czech", czechToolStripMenuItem);
            languageToolBarItems.Add("French", frenchToolStripMenuItem);
            languageToolBarItems.Add("German", germanToolStripMenuItem);
            languageToolBarItems.Add("Italian", italianToolStripMenuItem);
            languageToolBarItems.Add("Spanish", spanishToolStripMenuItem);
            languageToolBarItems.Add("Russian", russianToolStripMenuItem);
            languageToolBarItems.Add("Polish", polishToolStripMenuItem);
        }

        private void languageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
            {
                if (e.ClickedItem == kvp.Value)
                {
                    if (Domain.Properties.Settings.Default.LocalizationName != kvp.Key)
                    {
                        Domain.Properties.Settings.Default.LocalizationName = kvp.Key;
                        ReloadLanguageFiles();
                    }

                    break;
                }
            }
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
            {
                kvp.Value.Checked =
                    string.Compare(kvp.Key, Domain.Properties.Settings.Default.LocalizationName,
                                   StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

    }
}
