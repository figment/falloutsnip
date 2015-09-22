using System.IO;
using FalloutSnip.Domain.Data;
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
        private DeserializeDockContent mDeserializeDockContent;
        private readonly HtmlContent htmlContent = new HtmlContent();
        private readonly PluginTreeContent pluginTreeContent = new PluginTreeContent();
        private readonly SubrecordListContent subrecordListContent = new SubrecordListContent();

        private void InitializeDockingWindows()
        {
            mDeserializeDockContent = GetContentFromPersistString;
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(PluginTreeContent).ToString())
            {
                return pluginTreeContent;
            }

            if (persistString == typeof(SubrecordListContent).ToString())
            {
                return subrecordListContent;
            }


            if (persistString == typeof(HtmlContent).ToString())
            {
                return htmlContent;
            }

            return null;
        }

        public T GetWindowByName<T>(string name) where T : BaseDockContent
        {
            return Application.OpenForms.OfType<T>().FirstOrDefault(x => string.Compare(x.Text, name, false) == 0);
        }

        public T GetOrCreateWindowByName<T>(string name) where T : BaseDockContent, new()
        {
            var form = Application.OpenForms.OfType<T>().FirstOrDefault(x => string.Compare(x.Text, name, false) == 0);
            if (form == null)
            {
                form = new T { Text = name };
                form.Show(dockPanel, DockState.Document);
            }
            if (!form.Visible)
            {
                form.Show(dockPanel, DockState.Document);
            }
            return form;
        }

        private void LoadDockingWindows()
        {
            string configFile = Path.Combine(Options.Value.SettingsDirectory, @"DockPanel.config");
            if (File.Exists(configFile))
            {
                try
                {
                    dockPanel.SuspendLayout(true);
                    dockPanel.LoadFromXml(configFile, mDeserializeDockContent);
                }
                catch
                {
                    if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
                    {
                        try
                        {
                            File.Delete(configFile);
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    dockPanel.ResumeLayout(true, true);
                }
            }

            LayoutDockingWindows(force: false);
        }

        private void LayoutDockingWindows(bool force)
        {
            try
            {
                if (!force && IsVisible(pluginTreeContent) && IsVisible(subrecordListContent) &&
                    IsVisible(htmlContent))
                {
                    return;
                }

                dockPanel.SuspendLayout(true);
                if (force)
                {
                    pluginTreeContent.DockPanel = null;
                    subrecordListContent.DockPanel = null;
                    htmlContent.DockPanel = null;
                }

                if (!IsVisible(pluginTreeContent) || force)
                {
                    pluginTreeContent.Show(dockPanel, DockState.DockLeft);
                    dockPanel.Width = Math.Max(dockPanel.Width, pluginTreeContent.MinimumSize.Width);
                }

                if (!IsVisible(subrecordListContent) || force)
                {
                    subrecordListContent.Show(pluginTreeContent.Pane, DockAlignment.Bottom, 0.5);
                }

                if (!IsVisible(htmlContent) || force)
                {
                    htmlContent.Show(dockPanel, DockState.Document);
                }
            }
            catch
            {
            }
            finally
            {
                dockPanel.ResumeLayout(true, true);
            }
        }


        private void SaveDockingWindows()
        {
            string configFile = null;
            try
            {
                configFile = Path.Combine(Options.Value.SettingsDirectory, "DockPanel.config");
                dockPanel.SaveAsXml(configFile);
            }
            catch
            {
                if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
                {
                    try
                    {
                        File.Delete(configFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void ShowDockingWindows()
        {
            pluginTreeContent.CloseButtonVisible = false;
            subrecordListContent.CloseButtonVisible = false;
            htmlContent.MdiParent = this;
            htmlContent.CloseButtonVisible = false;
            htmlContent.CloseButton = false;
            htmlContent.HideOnClose = true;
            LayoutDockingWindows(force: false);
        }


        private void CloseAllContents()
        {
            // we don't want to create another instance of tool window, set DockPanel to null
            pluginTreeContent.DockPanel = null;
            subrecordListContent.DockPanel = null;
        }

        private static bool IsVisible(IDockContent content)
        {
            return content.DockHandler.DockState != DockState.Hidden &&
                   content.DockHandler.DockState != DockState.Unknown;
        }

    }
}
