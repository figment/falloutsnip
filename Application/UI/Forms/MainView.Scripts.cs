using System.IO;
using FalloutSnip.Domain.Data;
using FalloutSnip.Properties;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.Services;
using WeifenLuo.WinFormsUI.Docking;

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

        #region Dynamic IronPython Plugin Scripts

        private void LoadDynamicScripts()
        {
            if (!PluginEngine.Default.Plugins.Any())
            {
                globalScriptsToolStripMenuItem.Enabled = false;
                selectionScriptsToolStripMenuItem.Enabled = false;
            }
            foreach (var plugin in PluginEngine.Default.Plugins)
            {
                // not valid 
                if (string.IsNullOrWhiteSpace(plugin.Name))
                    continue;

                if (plugin.SupportsSelection)
                {
                    var item = new ToolStripMenuItem
                    {
                        Name = plugin.Name,
                        Text = string.IsNullOrWhiteSpace(plugin.DisplayName) ? plugin.Name : plugin.DisplayName,
                        Image = plugin.DisplayImage,
                        ToolTipText = plugin.ToolTipText,
                        AutoToolTip = !string.IsNullOrWhiteSpace(plugin.ToolTipText),
                        Visible = true,
                        Enabled = true,
                        Tag = plugin.Name,
                    };
                    selectionScriptsToolStripMenuItem.DropDownItems.Add(item);
                }
                if (plugin.SupportGlobal)
                {
                    var item = new ToolStripMenuItem
                    {
                        Name = plugin.Name,
                        Text = string.IsNullOrWhiteSpace(plugin.DisplayName) ? plugin.Name : plugin.DisplayName,
                        Image = plugin.DisplayImage,
                        ToolTipText = plugin.ToolTipText,
                        AutoToolTip = !string.IsNullOrWhiteSpace(plugin.ToolTipText),
                        Visible = true,
                        Enabled = true,
                        Tag = plugin.Name,
                    };
                    globalScriptsToolStripMenuItem.DropDownItems.Add(item);
                }
            }

            var rootUri = new Uri(Path.Combine(PluginEngine.PluginsPyPath, "."), UriKind.Absolute);
            foreach (
                var filename in
                    Directory.EnumerateFiles(PluginEngine.PluginsPyPath, "*.py", SearchOption.TopDirectoryOnly))
            {
                var relativePath = rootUri.MakeRelativeUri(new Uri(filename, UriKind.Absolute)).ToString();
                // was going to show subdirectories but will leave that alone for now
                var item = new ToolStripMenuItem
                {
                    Name = Path.GetFileNameWithoutExtension(relativePath),
                    Text = Path.GetFileNameWithoutExtension(relativePath),
                    Image = Resources.PythonScript32x32,
                    ToolTipText = string.Format("Open in {0} in default python editor", relativePath),
                    AutoToolTip = true,
                    Visible = true,
                    Enabled = true,
                    Tag = Path.GetFullPath(filename),
                };
                editScriptsToolStripMenuItem.DropDownItems.Add(item);
            }

            selectionScriptsToolStripMenuItem.Enabled = selectionScriptsToolStripMenuItem.HasDropDownItems;
            globalScriptsToolStripMenuItem.Enabled = globalScriptsToolStripMenuItem.HasDropDownItems;
        }

        private void ClearDynamicScripts()
        {
            globalScriptsToolStripMenuItem.DropDownItems.Clear();
            selectionScriptsToolStripMenuItem.DropDownItems.Clear();
            editScriptsToolStripMenuItem.DropDownItems.Clear();
        }

        private void BuildDynamicScriptsMenu()
        {
            ClearDynamicScripts();
            var outputWindow = GetWindowByName<OutputTextContent>("Output");
            if (outputWindow != null)
                outputWindow.UpdateText("");
            PluginEngine.Default.Reinitialize();
            LoadDynamicScripts();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildDynamicScriptsMenu();
        }

        private void scriptsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            selectionScriptsToolStripMenuItem.Enabled =
                scriptsToolStripMenuItem.Enabled &&
                selectionScriptsToolStripMenuItem.HasDropDownItems &&
                PluginTree.SelectedRecord != null;
        }

        private void globalScriptsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag != null)
            {
                var name = e.ClickedItem.Tag as string;
                PluginEngine.Default.ExecuteByName(name);
            }
        }

        private IList BuildSelectionList()
        {
            var recs = PluginTree.SelectedRecords;
            return recs != null ? recs.ToArray() : new object[0];
        }

        private void selectionScriptsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var recs = BuildSelectionList();
            if (recs.Count == 0) return;
            foreach (ToolStripMenuItem menu in selectionScriptsToolStripMenuItem.DropDownItems)
            {
                var name = menu.Tag as string;
                menu.Enabled = !string.IsNullOrEmpty(name) &&
                               PluginEngine.Default.IsValidSelectionByName(name, recs);
            }
        }

        private void selectionScriptsToolStripMenuItem_DropDownItemClicked(object sender,
                                                                           ToolStripItemClickedEventArgs e)
        {
            var recs = BuildSelectionList();
            if (recs.Count == 0) return;

            var name = e.ClickedItem.Tag as string;
            if (e.ClickedItem.Enabled)
                PluginEngine.Default.ExecuteSelectionByName(name, recs);
        }

        #endregion


        public InterpreterConsole CreateInterpreterWindow()
        {
            int id = Application.OpenForms.OfType<InterpreterConsole>().Count() + 1;
            var form = new InterpreterConsole { Text = string.Format("Console {0}", id) };
            var console = Application.OpenForms.OfType<InterpreterConsole>().LastOrDefault(x => x.Visible);
            if (console != null)
            {
                if (console.Pane != null)
                {
                    // second item in list
                    form.Show(console.Pane, null);
                }
                else if (console.PanelPane != null)
                {
                    form.Show(console.PanelPane, null);
                }
            }
            else
            {
                form.Show(dockPanel, DockState.Document);
            }

            return form;
        }

    }
}
