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
        private static object s_clipboard;
        private static string mruRegKey = "SOFTWARE\\FalloutSnip\\MRU";
        private readonly SelectionContext Selection;




        private void PasteFromClipboard(bool recordOnly, bool asNew)
        {
            if (!HasClipboardData())
            {
                MessageBox.Show(Resources.TheClipboardIsEmpty, Resources.ErrorText);
                return;
            }

            if (PluginTree.ContainsFocus)
            {
                PluginTree.PasteFromClipboard(recordOnly, asNew);
            }
            else if (SubrecordList.ContainsFocus)
            {
                SubrecordList.PasteFromClipboard();
            }
        }

        private void PluginTree_OnSelectionUpdated(object sender, EventArgs e)
        {
            // fix EDID if relevant
            UpdateMainText(PluginTree.SelectedRecord);
            PluginTree.RefreshObject(PluginTree.SelectedRecord);
        }

        private void PluginTree_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(PluginTree.SelectedRecord);
        }

        private void RebuildSelection()
        {
            if (inRebuildSelection)
            {
                return;
            }

            bool oldInRebuildSelection = inRebuildSelection;
            try
            {
                inRebuildSelection = true;
                var rec = PluginTree.SelectedRecord;
                if (rec == null)
                {
                    SubrecordList.Record = null;
                    Selection.Record = null;
                    UpdateMainText(string.Empty);
                    return;
                }

                bool hasClipboard = HasClipboardData();

                if (rec is Plugin)
                {
                    SubrecordList.Record = null;
                    Selection.Record = null;
                    cutToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                    deleteToolStripMenuItem.Enabled = false;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    insertGroupToolStripMenuItem.Enabled = true;
                    insertRecordToolStripMenuItem.Enabled = true;
                    insertSubrecordToolStripMenuItem.Enabled = false;
                }
                else if (rec is Record)
                {
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    insertGroupToolStripMenuItem.Enabled = false;
                    insertRecordToolStripMenuItem.Enabled = true;
                    insertSubrecordToolStripMenuItem.Enabled = true;
                    Selection.Record = rec as Rec;
                    SubrecordList.Record = Selection.Record as Record;
                    MatchRecordStructureToRecord();
                }
                else if (rec is GroupRecord)
                {
                    Selection.Record = null;
                    SubrecordList.Record = null;
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    insertGroupToolStripMenuItem.Enabled = true;
                    insertRecordToolStripMenuItem.Enabled = true;
                    insertSubrecordToolStripMenuItem.Enabled = false;
                }
                else
                {
                    Selection.Record = null;
                    SubrecordList.Record = null;
                    cutToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                    deleteToolStripMenuItem.Enabled = false;
                    pasteToolStripMenuItem.Enabled = false;
                    pasteNewToolStripMenuItem.Enabled = false;
                    insertGroupToolStripMenuItem.Enabled = false;
                    insertRecordToolStripMenuItem.Enabled = false;
                    insertSubrecordToolStripMenuItem.Enabled = false;
                }

                Selection.SubRecord = GetSelectedSubrecord();
            }
            finally
            {
                inRebuildSelection = oldInRebuildSelection;
            }
        }

        public static object Clipboard
        {
            get { return GetClipboardData(); }

            set
            {
                SetClipboardData(value);
                if (ClipboardChanged != null)
                {
                    ClipboardChanged(null, EventArgs.Empty);
                }
            }
        }

        private void CopySelectedSubRecord()
        {
            var sr = GetSelectedSubrecords();
            if (sr == null)
            {
                return;
            }

            Clipboard = sr.Select(ss => (SubRecord)ss.Clone()).ToArray();
        }

        private void CopySelection()
        {
            // Route to focused control.
            if (PluginTree.ContainsFocus)
            {
                PluginTree.CopySelectedRecord();
            }
            else if (SubrecordList.ContainsFocus)
            {
                if (Selection.SelectedSubrecord)
                {
                    CopySelectedSubRecord();
                }
            }
        }


        internal static object GetClipboardData()
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                if (od != null)
                {
                    var cliptype = od.GetData("FalloutSnip");
                    if (cliptype is string)
                    {
                        return od.GetData(cliptype.ToString());
                    }
                }

                return null;
            }

            return s_clipboard;
        }

        internal static T GetClipboardData<T>() where T : class
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                if (od != null)
                {
                    var clip = od.GetData(typeof(T).FullName);
                    return clip as T;
                }

                return default(T);
            }

            return s_clipboard as T;
        }

        internal static bool HasClipboardData()
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent("FalloutSnip");
            }

            return Clipboard != null;
        }

        internal static bool HasClipboardData<T>()
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent(typeof(T).FullName);
            }

            return Clipboard is T;
        }

        internal static void SetClipboardData(object value)
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var cloneable = value as ICloneable;
                if (cloneable != null)
                {
                    var ido = new DataObject();
                    var srFormat = value.GetType().FullName;
                    ido.SetData(srFormat, cloneable.Clone());
                    ido.SetData("FalloutSnip", srFormat);
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetDataObject(ido, true);
                }
            }
            else
            {
                s_clipboard = value;
            }
        }
    }
}
