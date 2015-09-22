using System.Threading;
using FalloutSnip.Domain.Data;
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
        private OutputTextContent outputTextContent;
        private System.Threading.Timer statusTimer;


        #region Scripting Output Window

        private void outputWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (outputTextContent == null)
            {
                outputTextContent = GetWindowByName<OutputTextContent>("Output");
                if (outputTextContent == null)
                {
                    outputTextContent = GetOrCreateWindowByName<OutputTextContent>("Output");
                    outputTextContent.AppendText(PluginEngine.Default.GetOutputText());
                    outputTextContent.Closed += outputWindow_Closed;
                    PluginEngine.Default.OnConsoleMessage += pluginEngine_OnConsoleMessage;
                }
            }
        }

        private void outputWindow_Closed(object sender, EventArgs e)
        {
            outputTextContent = null;
            PluginEngine.Default.OnConsoleMessage -= pluginEngine_OnConsoleMessage;
        }

        private void pluginEngine_OnConsoleMessage(object sender, PluginEngine.MessageEventArgs e)
        {
            if (outputTextContent != null)
            {
                outputTextContent.AppendText(e.Text);
            }
        }

        #endregion


        /// <summary>
        ///     Send text to status and then clear 5 seconds later.
        /// </summary>
        /// <param name="text">
        /// </param>
        public void SendStatusText(string text)
        {
            SendStatusText(text, SystemColors.ControlText);
        }

        public void SendStatusText(string text, Color color)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string, Color>(SendStatusText), new object[] { text, color });
                }
                else
                {
                    toolStripStatusLabel.ForeColor = color;
                    toolStripStatusLabel.Text = text;
                    if (statusTimer == null)
                    {
                        statusTimer = new System.Threading.Timer(
                            o =>
                            Invoke(new TimerCallback(o2 => { toolStripStatusLabel.Text = string.Empty; }),
                                   new object[] { string.Empty }), string.Empty, TimeSpan.FromSeconds(15),
                            TimeSpan.FromMilliseconds(-1));
                    }
                    else
                    {
                        statusTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void PostStatusText(string text)
        {
            PostStatusText(text, SystemColors.ControlText);
        }

        public static void PostStatusText(string text, Color color)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault();
            form?.SendStatusText(text, color);
        }

        public static void PostStatusWarning(string text)
        {
            PostStatusText(text, Color.OrangeRed);
        }

    }
}
