using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Crownwood.Magic.Docking;
using Crownwood.Magic.Menus;
using RTF;
using TESVSnip.Data;
using TESVSnip.Forms;
using TESVSnip.Properties;
using Timer = System.Threading.Timer;

namespace TESVSnip
{
    internal delegate string dFormIDLookupS(string id);
    internal delegate string dFormIDLookupI(uint id);
    internal delegate Record dFormIDLookupR(uint id);
    internal delegate string dLStringLookup(uint id);
    internal delegate string[] dFormIDScan(string type);
    internal delegate Record[] dFormIDScanR(string type);
    internal delegate IEnumerable<KeyValuePair<uint, Record>> dFormIDScanRec(string type);

    internal partial class MainView : Form
    {
        private static object s_clipboard;
        private readonly SelectionContext Selection;
        private StringsEditor stringEditor;
        private Timer statusTimer;
        private bool inRebuildSelection;

        public MainView()
        {
            if (!RecordStructure.Loaded)
            {
                try
                {
                    RecordStructure.Load();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Resources.CannotParseRecordStructure + ex.Message, Resources.WarningText);
                }
            }

            InitializeComponent();

            dockingManagerExtender.AutomaticStatePersistence = Properties.Settings.Default.AutoSaveDockingState;
            dockingManagerExtender.DockingManager.ContextMenu += delegate(PopupMenu pm, CancelEventArgs cea)
            {
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
            };

            InitializeToolStripFind();

            PluginTree.SelectionChanged += (o, e) => RebuildSelection();

            if (string.IsNullOrEmpty(Properties.Settings.Default.DefaultSaveFolder)
                || !Directory.Exists(Properties.Settings.Default.DefaultSaveFolder))
            {
                SaveModDialog.InitialDirectory = Program.gameDataDir;
            }
            else
            {
                SaveModDialog.InitialDirectory = Properties.Settings.Default.DefaultSaveFolder;
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.DefaultOpenFolder)
                || !Directory.Exists(Properties.Settings.Default.DefaultOpenFolder))
            {
                OpenModDialog.InitialDirectory = Program.gameDataDir;
            }
            else
            {
                OpenModDialog.InitialDirectory = Properties.Settings.Default.DefaultOpenFolder;
            }


            Icon = Resources.tesv_ico;
            try
            {
                if (!Properties.Settings.Default.IsFirstTimeOpening)
                {
                    Settings.GetWindowPosition("TESsnip", this);
                }
                else
                {
                    Settings.SetWindowPosition("TESsnip", this);
                    Properties.Settings.Default.IsFirstTimeOpening = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
            }

            useWindowsClipboardToolStripMenuItem.Checked = Properties.Settings.Default.UseWindowsClipboard;
            noWindowsSoundsToolStripMenuItem.Checked = Properties.Settings.Default.NoWindowsSounds;
            disableHyperlinksToolStripMenuItem.Checked = Properties.Settings.Default.DisableHyperlinks;
            rtfInfo.DetectUrls = !Properties.Settings.Default.DisableHyperlinks;
            saveStringsFilesToolStripMenuItem.Checked = Properties.Settings.Default.SaveStringsFiles;

            useNewSubrecordEditorToolStripMenuItem.Checked = !Properties.Settings.Default.UseOldSubRecordEditor;
            hexModeToolStripMenuItem.Checked = Properties.Settings.Default.UseHexSubRecordEditor;


            Selection = new SelectionContext();
            Selection.formIDLookup = LookupFormIDI;
            Selection.strLookup = LookupFormStrings;
            Selection.formIDLookupR = GetRecordByID;

            subrecordPanel.SetContext(Selection);
            InitializeLanguage();

            ClipboardChanged += (o, e) => RebuildSelection();
            Selection.RecordChanged += (o, a) => RebuildSelection();
            Selection.SubRecordChanged += (o, a) => RebuildSelection();

            PluginTree.OnSelectionUpdated += PluginTree_OnSelectionUpdated;

            PluginTree.SelectionChanged += PluginTree_SelectionChanged;
            subrecordPanel.SelectionChanged += subrecordPanel_SelectionChanged;
            subrecordPanel.OnSubrecordChanged += subrecordPanel_OnSubrecordChanged;
            subrecordPanel.DataChanged += subrecordPanel_DataChanged;
        }

        private void PluginTree_OnSelectionUpdated(object sender, EventArgs e)
        {
            // fix EDID if relevant
            UpdateMainText(PluginTree.SelectedRecord);
            PluginTree.RefreshObject(PluginTree.SelectedRecord);
        }

        private void subrecordPanel_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(subrecordPanel.SubRecord);
        }

        private void PluginTree_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(PluginTree.SelectedRecord);
        }

        internal static bool HasClipboardData()
        {
            if (Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent("TESVSnip");
            }
            return Clipboard != null;
        }

        internal static bool HasClipboardData<T>()
        {
            if (Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent(typeof (T).FullName);
            }
            return Clipboard is T;
        }

        internal static object GetClipboardData()
        {
            if (Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                if (od != null)
                {
                    var cliptype = od.GetData("TESVSnip");
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
            if (Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                if (od != null)
                {
                    var clip = od.GetData(typeof (T).FullName);
                    return clip as T;
                }
                return default(T);
            }
            return s_clipboard as T;
        }

        internal static void SetClipboardData(object value)
        {
            if (Properties.Settings.Default.UseWindowsClipboard)
            {
                var cloneable = value as ICloneable;
                if (cloneable != null)
                {
                    var ido = new DataObject();
                    var srFormat = value.GetType().FullName;
                    ido.SetData(srFormat, (cloneable).Clone());
                    ido.SetData("TESVSnip", srFormat);
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetDataObject(ido, true);
                }
            }
            else
            {
                s_clipboard = value;
            }
        }

        public static object Clipboard
        {
            get { return GetClipboardData(); }
            set
            {
                SetClipboardData(value);
                if (ClipboardChanged != null)
                    ClipboardChanged(null, EventArgs.Empty);
            }
        }

        public static event EventHandler ClipboardChanged;

        internal void LoadPlugin(string s)
        {
            var p = new Plugin(s, false, GetRecordFilter(s));
            PluginList.All.AddRecord(p);
            UpdateStringEditor();
            FixMasters();
            PluginTree.UpdateRoots();
            GC.Collect();
        }

        private string[] GetRecordFilter(string s)
        {
            string[] recFilter = null;
            bool bAskToApplyFilter = true;
            if (Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM)
            {
                if (String.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    DialogResult result = MessageBox.Show(this,
                                                          Resources.MainView_FirstTimeSkyrimLoad_ExcludeInquiry,
                                                          Resources.FirstLoadOptions
                                                          , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                          MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        Properties.Settings.Default.EnableESMFilter = true;
                        Properties.Settings.Default.DontAskUserAboutFiltering = true;
                        using (var settings = new LoadSettings())
                        {
                            result = settings.ShowDialog(this);
                            if (result == DialogResult.Cancel) // cancel will be same as No
                            {
                                Properties.Settings.Default.EnableESMFilter = false;
                                Properties.Settings.Default.DontAskUserAboutFiltering = true;
                            }
                        }
                        Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                    }
                    else if (result == DialogResult.No)
                    {
                        Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        Properties.Settings.Default.DontAskUserAboutFiltering = true;
                    }
                    else
                    {
                        Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        return null;
                    }
                }
                bAskToApplyFilter = false;
            }
            if (Properties.Settings.Default.EnableESMFilter)
            {
                bool applyfilter;
                if (Properties.Settings.Default.ApplyFilterToAllESM)
                    applyfilter = String.Compare(Path.GetExtension(s), ".esm", StringComparison.OrdinalIgnoreCase) == 0;
                else
                    applyfilter =
                        String.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0;

                if (applyfilter && bAskToApplyFilter && !Properties.Settings.Default.DontAskUserAboutFiltering)
                {
                    DialogResult result = MessageBox.Show(this, Resources.ESM_Large_File_Size_Inquiry,
                                                          Resources.Filter_Options_Text
                                                          , MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                          MessageBoxDefaultButton.Button1);
                    applyfilter = (result == DialogResult.Yes);
                }
                if (applyfilter)
                {
                    recFilter = Properties.Settings.Default.FilteredESMRecords.Trim().Split(new[] {';', ','},
                                                                                            StringSplitOptions.
                                                                                                RemoveEmptyEntries);
                }
            }
            return recFilter;
        }

        private void openNewPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenModDialog.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string s in OpenModDialog.FileNames)
                {
                    LoadPlugin(s);
                }
                FixMasters();
                PluginTree.UpdateRoots();
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.CloseAllLoseChangesInquiry, Resources.WarningText, MessageBoxButtons.YesNo) !=
                DialogResult.Yes) return;
            PluginList.All.Records.Clear();
            PluginTree.UpdateRoots();
            subrecordPanel.Record = null;
            Clipboard = null;
            CloseStringEditor();
            UpdateMainText("");
            RebuildSelection();
            PluginTree.UpdateRoots();
            GC.Collect();
        }

        /// <summary>
        /// This routine assigns Structure definitions to subrecords
        /// </summary>
        private bool MatchRecordStructureToRecord()
        {
            var rec = Selection.Record as Record;
            if (rec == null) return false;
            return rec.MatchRecordStructureToRecord();
        }

        private void RebuildSelection()
        {
            if (inRebuildSelection)
                return;

            bool oldInRebuildSelection = inRebuildSelection;
            try
            {
                inRebuildSelection = true;
                var rec = PluginTree.SelectedRecord;
                if (rec == null)
                {
                    subrecordPanel.Record = null;
                    Selection.Record = null;
                    UpdateMainText("");
                    return;
                }

                bool hasClipboard = HasClipboardData();

                if (rec is Plugin)
                {
                    subrecordPanel.Record = null;
                    Selection.Record = null;
                    cutToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                    deleteToolStripMenuItem.Enabled = false;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    insertRecordToolStripMenuItem.Enabled = true;
                    insertSubrecordToolStripMenuItem.Enabled = false;
                }
                else if (rec is Record)
                {
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = false;
                    insertRecordToolStripMenuItem.Enabled = false;
                    insertSubrecordToolStripMenuItem.Enabled = true;
                    Selection.Record = rec as Rec;
                    subrecordPanel.Record = Selection.Record as Record;
                    MatchRecordStructureToRecord();
                }
                else
                {
                    Selection.Record = null;
                    subrecordPanel.Record = null;
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    insertRecordToolStripMenuItem.Enabled = true;
                    insertSubrecordToolStripMenuItem.Enabled = false;
                }
                Selection.SubRecord = GetSelectedSubrecord();
            }
            finally
            {
                inRebuildSelection = oldInRebuildSelection;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.ContainsFocus)
            {
                PluginTree.DeleteSelection();
            }
            else if (subrecordPanel.ContainsFocus)
            {
                subrecordPanel.DeleteSelection();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }
            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            if (p.Filtered)
            {
                DialogResult result = MessageBox.Show(this, Resources.SavePluginWithFilterAppliedInquiry,
                                                      Resources.WarningText, MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            if (SaveModDialog.ShowDialog(this) == DialogResult.OK)
            {
                p.Save(SaveModDialog.FileName);
                FixMasters();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Selection.SelectedSubrecord && PluginTree.SelectedRecord != null && PluginTree.SelectedRecord is Plugin)
            {
                MessageBox.Show(Resources.Cannot_cut_a_plugin, Resources.ErrorText);
                return;
            }
            copyToolStripMenuItem_Click(null, null);
            deleteToolStripMenuItem_Click(null, null);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySelection();
        }

        private void CopySelection()
        {
            // Route to focused control.
            if (PluginTree.ContainsFocus)
            {
                PluginTree.CopySelectedRecord();
            }
            else if (subrecordPanel.ContainsFocus)
            {
                if (Selection.SelectedSubrecord)
                {
                    CopySelectedSubRecord();
                }
            }
        }

        private void CopySelectedSubRecord()
        {
            var sr = GetSelectedSubrecords();
            if (sr == null) return;
            Clipboard = sr.Select(ss => (SubRecord) ss.Clone()).ToArray();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false);
        }

        private void PasteFromClipboard(bool recordOnly)
        {
            if (!HasClipboardData())
            {
                MessageBox.Show(Resources.TheClipboardIsEmpty, Resources.ErrorText);
                return;
            }

            if (PluginTree.ContainsFocus)
            {
                PluginTree.PasteFromClipboard(recordOnly);
            }
            else if (subrecordPanel.ContainsFocus)
            {
                subrecordPanel.PasteFromClipboard();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = new Plugin();
            PluginList.All.AddRecord(p);
            var r = new Record();
            r.Name = "TES4";
            var sr = new SubRecord();
            sr.Name = "HEDR";
            sr.SetData(new byte[] {0xD7, 0xA3, 0x70, 0x3F, 0xFA, 0x56, 0x0C, 0x00, 0x19, 0xEA, 0x07, 0xFF});
            r.AddRecord(sr);
            sr = new SubRecord();
            sr.Name = "CNAM";
            sr.SetData(Encoding.CP1252.GetBytes("Default\0"));
            r.AddRecord(sr);
            p.AddRecord(r);

            RebuildSelection();
            UpdateStringEditor();
            FixMasters();
            PluginTree.UpdateRoots();
        }

        private Plugin GetPluginFromNode(BaseRecord node)
        {
            BaseRecord tn = node;
            if (tn is Plugin) return (Plugin) tn;
            while (!(tn is Plugin) && tn != null) tn = tn.Parent;
            if (tn != null) return tn as Plugin;
            return new Plugin();
        }

        private SelectionContext GetSelectedContext()
        {
            return Selection;
            //context.Record = this.parentRecord
            //context.SubRecord = GetSelectedSubrecord();
        }

        private SubRecord GetSelectedSubrecord()
        {
            return subrecordPanel.GetSelectedSubrecord();
        }

        private IEnumerable<SubRecord> GetSelectedSubrecords()
        {
            return subrecordPanel.GetSelectedSubrecords();
        }

        private void insertRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = PluginTree.SelectedRecord;
            var p = new Record();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
        }

        private void insertSubrecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseRecord node = PluginTree.SelectedRecord;
            var p = new SubRecord();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
            RebuildSelection();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!toolStripIncrFind.Visible)
            {
                toolStripIncrFind.Visible = true;
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.SelectAll();
                toolStripIncrFindText.Focus();
            }
            else
            {
                toolStripIncrFind.Visible = false;
            }
        }

        private void TESsnip_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.UseWindowsClipboard = useWindowsClipboardToolStripMenuItem.Checked;
            PluginList.All.Clear();
            PluginTree.UpdateRoots();
            Clipboard = null;
            Selection.Record = null;
            RebuildSelection();
            CloseStringEditor();
            Settings.SetWindowPosition("TESsnip", this);
        }

        private void tbInfo_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var tbBase = sender as TextBoxBase;
            if (tbBase != null)
            {
                if (e.Control)
                {
                    if (e.KeyCode == Keys.A)
                    {
                        tbBase.SelectAll();
                    }
                    else if (e.KeyCode == Keys.C)
                    {
                        tbBase.Copy();
                    }
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }
            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            PluginList.All.DeleteRecord(p);
            UpdateStringEditor();
            UpdateMainText("");
            FixMasters();
            PluginTree.UpdateRoots();
            RebuildSelection();
            GC.Collect();
        }

        private void FixMasters()
        {
            PluginList.FixMasters();
        }


        private string LookupFormIDI(uint id)
        {
            return LookupFormIDI(Selection, id);
        }

        private string LookupFormIDI(SelectionContext context, uint id)
        {
            if (context != null && context.Record != null)
            {
                var p = GetPluginFromNode(context.Record);
                if (p != null) p.LookupFormID(id);
            }
            return "No selection";
        }

        private Record GetRecordByID(uint id)
        {
            if (Selection != null && Selection.Record != null)
            {
                var p = GetPluginFromNode(Selection.Record);
                if (p != null) return p.GetRecordByID(id);
            }
            return null;
        }

        private string LookupFormStrings(uint id)
        {
            if (Selection != null && Selection.Record != null)
            {
                var p = GetPluginFromNode(Selection.Record);
                if (p != null) return p.LookupFormStrings(id);
            }
            return null;
        }

        private void reloadXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                RecordStructure.Load();
                RebuildSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.CannotParseRecordStructure + ex.Message, Resources.WarningText);
            }
        }

        private void editStringsToolStripMenuItem_Click(object sender, EventArgs e)
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
                StringsEditor editor = stringEditor;
                stringEditor = null;
                try
                {
                    if (!editor.IsDisposed)
                        editor.Close();
                }
                catch
                {
                }
            }
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            FixMasters();
        }

        #region Enable Disable User Interface

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void ControlEnable(Control control, bool enable)
        {
            var ipenable = new IntPtr(enable ? 1 : 0);
            SendMessage(control.Handle, WM_SETREDRAW, ipenable, IntPtr.Zero);
        }

        private const int WM_SETREDRAW = 0x0b;

        private void EnableUserInterface(bool enable)
        {
            //ControlEnable(this.splitHorizontal, enable);
            //ControlEnable(this.splitVertical, enable);
            ControlEnable(menuStrip1, enable);
            ControlEnable(toolStripIncrFind, enable);
            ControlEnable(toolStripIncrInvalidRec, enable);
        }

        #endregion

        #region Action

        private volatile bool backgroundWorkCanceled;

        public void StartBackgroundWork(Action workAction, Action completedAction)
        {
            if (backgroundWorker1.IsBusy)
                return;

            EnableUserInterface(false);
            backgroundWorkCanceled = false;
            toolStripStatusProgressBar.ProgressBar.Value = toolStripStatusProgressBar.Minimum;
            toolStripStatusProgressBar.Visible = true;
            toolStripStopProgress.Visible = true;
            backgroundWorker1.RunWorkerAsync(new[] {workAction, completedAction});
        }

        public void UpdateBackgroundProgress(int percentProgress)
        {
            backgroundWorker1.ReportProgress(percentProgress);
        }

        public void CancelBackgroundProcess()
        {
            backgroundWorkCanceled = true;
            backgroundWorker1.CancelAsync();
        }

        public bool IsBackroundProcessCanceled()
        {
            return backgroundWorkCanceled;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var actions = e.Argument as Action[];
            if (actions.Length > 0)
            {
                actions[0]();
            }
            if (actions.Length > 1)
            {
                e.Result = actions[1];
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusProgressBar.Value = e.ProgressPercentage%toolStripStatusProgressBar.Maximum;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableUserInterface(true);
            toolStripStatusProgressBar.Visible = false;
            toolStripStopProgress.Visible = false;
            if (e.Cancelled || e.Error != null)
                return;
            var completedAction = e.Result as Action;
            if (completedAction != null) completedAction();
        }

        private void toolStripStopProgress_Click(object sender, EventArgs e)
        {
            CancelBackgroundProcess();
        }


        private void mergeRecordsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Records baseRecords;
            Records updateRecords;

            var xs = new XmlSerializer(typeof (Records));
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Base Record Structure";
                dlg.InitialDirectory = Program.settingsDir;
                dlg.FileName = "RecordStructure.xml";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                using (FileStream fs = File.OpenRead(dlg.FileName))
                {
                    baseRecords = xs.Deserialize(fs) as Records;
                }
            }
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Record Structure XML To Merge";
                dlg.InitialDirectory = Path.GetTempPath();
                dlg.FileName = "RecordStructure.xml";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                using (FileStream fs = File.OpenRead(dlg.FileName))
                {
                    updateRecords = xs.Deserialize(fs) as Records;
                }
            }
            if (updateRecords != null && baseRecords != null)
            {
                var builder = new RecordBuilder();
                builder.MergeRecords(baseRecords.Items.OfType<RecordsRecord>()
                                     , updateRecords.Items.OfType<RecordsRecord>());

                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title = "Select Record Structure To Save";
                    dlg.InitialDirectory = Path.GetTempPath();
                    dlg.FileName = "RecordStructure.xml";
                    dlg.OverwritePrompt = false;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        using (StreamWriter fs = File.CreateText(dlg.FileName))
                        {
                            xs.Serialize(fs, updateRecords);
                        }
                    }
                }
            }
        }

        #endregion

        private void eSMFilterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Update the global list
            bool modified = false;
            var groups = Properties.Settings.Default.AllESMRecords != null
                ? Properties.Settings.Default.AllESMRecords.Trim()
                .Split(new[] {';', ','},StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
            groups.Sort();
            foreach (var plugin in PluginList.All.Records.OfType<Plugin>())
            {
                plugin.ForEach(r =>
                    {
                        if (r is GroupRecord)
                        {
                            var g = (GroupRecord) r;
                            var s = g.ContentsType;
                            if (!string.IsNullOrEmpty(s))
                            {
                                int idx = groups.BinarySearch(s);
                                if (idx < 0)
                                {
                                    groups.Insert(~idx, s);
                                    modified = true;
                                }
                            }
                        }
                    });
            }
            RecordStructure.Load();
            var allRecords = RecordStructure.Records.Select(kvp => kvp.Key).ToList();
            foreach (var str in allRecords)
            {
                int idx = groups.BinarySearch(str);
                if (idx < 0)
                {
                    groups.Insert(~idx, str);
                    modified = true;
                }
            }

            if (modified)
            {
                Properties.Settings.Default.AllESMRecords = string.Join(";", groups.ToArray());
            }

            using (var settings = new LoadSettings())
            {
                settings.ShowDialog(this);
            }
        }

        #region ToolStrip Check Toggle

        private void toolStripCheck_CheckStateChanged(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;
            button.Image = button.Checked
                               ? Resources.checkedbox
                               : Resources.emptybox
                ;
        }

        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lookupFormidsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lookupFormidsToolStripMenuItem.Checked)
            {
                Selection.formIDLookup = LookupFormIDI;
                Selection.strLookup = LookupFormStrings;
                Selection.formIDLookupR = GetRecordByID;
            }
            else
            {
                Selection.formIDLookup = null;
                Selection.strLookup = null;
                Selection.formIDLookupR = null;
            }
        }

        #region Increment Invalid Record Search

        private void toolStripIncrInvalidRecNext_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;
            BackgroundNonConformingRecordIncrementalSearch(PluginTree.SelectedRecord, true,
                                                           toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecPrev_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;
            BackgroundNonConformingRecordIncrementalSearch(PluginTree.SelectedRecord, false,
                                                           toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecRestart_Click(object sender, EventArgs e)
        {
            var rec = PluginList.All.Records.OfType<BaseRecord>().FirstOrDefault();
            if (rec == null) return;
            BackgroundNonConformingRecordIncrementalSearch(rec, true, false);
        }

        private void toolStripIncrInvalidRecCancel_Click(object sender, EventArgs e)
        {
            toolStripIncrInvalidRec.Visible = false;
        }

        private void toolStripIncrInvalidRec_VisibleChanged(object sender, EventArgs e)
        {
            findNonconformingRecordToolStripMenuItem.Checked = toolStripIncrInvalidRec.Visible;
            toolStripIncrInvalidRecStatus.Text = "Select Next or Prev to start search.";
            toolStripIncrInvalidRecStatus.ForeColor = Color.DarkGray;
        }

        #endregion

        private void toolStripIncrInvalidRec_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }


        private void useWindowsClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseWindowsClipboard =
                useWindowsClipboardToolStripMenuItem.Checked = !useWindowsClipboardToolStripMenuItem.Checked;
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            pasteToolStripMenuItem.Enabled = HasClipboardData();
        }

        private void UpdateMainText(BaseRecord rec)
        {
            if (rec == null)
            {
                UpdateMainText("");
            }
            else
            {
                FontLangInfo defLang;
                if (!Encoding.TryGetFontInfo(Properties.Settings.Default.LocalizationName, out defLang))
                    defLang = new FontLangInfo(1252, 1033, 0);

                var rb = new RTFBuilder(RTFFont.Arial, 16, defLang.lcid, defLang.charset);
                var sc = GetSelectedContext();
                rec.GetFormattedHeader(rb, sc);
                rec.GetFormattedData(rb, sc);
                rtfInfo.Rtf = rb.ToString();
            }
        }

        private void UpdateMainText(string text)
        {
            //tbInfo.Text = text;
            rtfInfo.Text = text;
        }

        private static readonly Regex linkRegex =
            new Regex(
                "^(?:(?<text>[^#]*)#)?(?<type>[0-z][A-Z][A-Z][A-Z_]):(?<id>[0-9a-zA-Z]+)$"
                , RegexOptions.None);

        private void rtfInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                var m = linkRegex.Match(e.LinkText);
                if (m.Success)
                {
                    var n = PluginTree.SelectedRecord ?? PluginTree.TopRecord;

                    // Search current plugin and then wrap around.  
                    //   Should do it based on master plugin list first.
                    var type = m.Groups["type"].Value;
                    var searchContext = new SearchSettings();
                    searchContext.rectype = type == "XXXX" ? null : type;
                    searchContext.text = m.Groups["id"].Value;
                    searchContext.type = SearchType.FormID;
                    searchContext.startNode = n;
                    searchContext.wrapAround = true;
                    searchContext.partial = false;
                    searchContext.forward = true;
                    searchContext.first = true;
                    var node = PerformSearch(searchContext);
                    if (node != null)
                        PluginTree.SelectedRecord = node;
                }
            }
            catch
            {
            }
        }

        private void noWindowsSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.NoWindowsSounds =
                noWindowsSoundsToolStripMenuItem.Checked = !noWindowsSoundsToolStripMenuItem.Checked;
        }

        private void disableHyperlinksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisableHyperlinks =
                disableHyperlinksToolStripMenuItem.Checked = !disableHyperlinksToolStripMenuItem.Checked;
            rtfInfo.DetectUrls = !Properties.Settings.Default.DisableHyperlinks;
        }

        #region String Tools

        internal Dictionary<string, ToolStripMenuItem> languageToolBarItems =
            new Dictionary<string, ToolStripMenuItem>(StringComparer.InvariantCultureIgnoreCase);

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

        private void ReloadLanguageFiles()
        {
            foreach (Plugin p in PluginList.All.Records)
                p.ReloadStrings();
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
                kvp.Value.Checked =
                    String.Compare(kvp.Key, Properties.Settings.Default.LocalizationName,
                                   StringComparison.OrdinalIgnoreCase) == 0;
        }

        private void languageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
            {
                if (e.ClickedItem == kvp.Value)
                {
                    if (Properties.Settings.Default.LocalizationName != kvp.Key)
                    {
                        Properties.Settings.Default.LocalizationName = kvp.Key;
                        ReloadLanguageFiles();
                    }
                    break;
                }
            }
        }

        private void stringLocalizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.SkyrimLocalizerPath) ||
                !File.Exists(Properties.Settings.Default.SkyrimLocalizerPath))
            {
                var result = MessageBox.Show(this,
                                             "Skyrim String Localizer is not found.\nWould you like to browse for it?"
                                             , "Program Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                using (var dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select Record Structure XML To Merge";
                    dlg.FileName = "Skyrim String Localizer.exe";
                    if (dlg.ShowDialog(this) != DialogResult.OK)
                        return;
                    Properties.Settings.Default.SkyrimLocalizerPath = dlg.FileName;
                }
            }
            if (File.Exists(Properties.Settings.Default.SkyrimLocalizerPath))
            {
                try
                {
                    using (var p = new Process())
                    {
                        var startInfo = new ProcessStartInfo(Properties.Settings.Default.SkyrimLocalizerPath);
                        p.StartInfo = startInfo;
                        p.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Resources.ErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool found = false;
            //stringLocalizerToolStripMenuItem.Enabled = false;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SkyrimLocalizerPath))
            {
                if (File.Exists(Properties.Settings.Default.SkyrimLocalizerPath))
                {
                    found = true;
                }
            }
            if (found)
            {
                stringLocalizerToolStripMenuItem.ToolTipText = "Open the Skyrim String Localizer...";
            }
            else
            {
                stringLocalizerToolStripMenuItem.ToolTipText =
                    "Skyrim String Localizer is not found.  Select to browse for it...";
            }
        }

        #endregion

        private void saveStringsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveStringsFiles =
                saveStringsFilesToolStripMenuItem.Checked = !saveStringsFilesToolStripMenuItem.Checked;
        }

        private void reloadStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadLanguageFiles();
        }


        private void addMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var amfNewMaster = new AddMasterForm())
            {
                if (amfNewMaster.ShowDialog(this) == DialogResult.OK)
                {
                    Plugin plugin = GetPluginFromNode(PluginTree.SelectedRecord);
                    if (plugin == null)
                    {
                        MessageBox.Show(this, "No plugin selected. Cannot continue.", "Missing Plugin",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    try
                    {
                        if (plugin.AddMaster(amfNewMaster.MasterName))
                        {
                            FixMasters();
                            RebuildSelection();
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        MessageBox.Show(this, ex.Message, "Missing Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void resetDockingWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetDockingWindows();
        }

        private void ResetDockingWindows()
        {
            switch (
                MessageBox.Show(
                    "Would you like to reset your custom layout back to default layout?\n\r Remark: You have to restart application until new setting can take effect.",
                    "Automatic State Persistence", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    dockingManagerExtender.ResetAutoPersistent(false);
                    break;
            }
        }

        private void compressionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new CompressSettings())
            {
                if (DialogResult.OK == dlg.ShowDialog(this))
                {
                    // nothing of interest
                }
            }
        }


        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.CollapseAll();
        }

        private void expandBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.ExpandAll(PluginTree.SelectedRecord);
        }

        private void collapseBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.CollapseAll(PluginTree.SelectedRecord);
        }

        /// <summary>
        /// Send text to status and then clear 5 seconds later
        /// </summary>
        /// <param name="text"></param>
        /// 
        public void SendStatusText(string text)
        {
            SendStatusText(text, SystemColors.ControlText);
        }

        public void SendStatusText(string text, Color color)
        {
            toolStripStatusLabel.ForeColor = color;
            toolStripStatusLabel.Text = text;
            if (statusTimer == null)
            {
                statusTimer = new Timer(
                    o =>
                    Invoke(new TimerCallback(o2 => { toolStripStatusLabel.Text = ""; }), new object[] {""})
                    , "", TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                statusTimer.Change(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(-1));
            }
        }

        public static void PostStatusText(string text)
        {
            PostStatusText(text, SystemColors.ControlText);
        }

        public static void PostStatusWarning(string text)
        {
            PostStatusText(text, Color.OrangeRed);
        }

        public static void PostStatusText(string text, Color color)
        {
            foreach (MainView form in Application.OpenForms.OfType<MainView>())
                (form).SendStatusText(text, color);
        }

        private void MainView_Shown(object sender, EventArgs e)
        {
            // Only prevent content hiding after window if first shown
            dockingManagerExtender.DockingManager.ContentHiding +=
                delegate(Content c, CancelEventArgs cea) { cea.Cancel = true; };
            dockingManagerExtender.DockingManager.ShowAllContents();

            if (!DesignMode)
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    var attr =
                        asm.GetCustomAttributes(true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                    if (attr != null) Text = attr.InformationalVersion;
                }
                catch
                {
                }
            }
        }

        private void editSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.ContainsFocus)
            {
                PluginTree.EditSelectedRecord();
            }
            else if (subrecordPanel.ContainsFocus)
            {
                subrecordPanel.EditSelectedSubrecord();
            }
        }

        private void editHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.ContainsFocus)
            {
                PluginTree.EditSelectedHeader();
            }
            else if (subrecordPanel.ContainsFocus)
            {
                subrecordPanel.EditSelectedSubrecordHex();
            }
        }


        private void subrecordPanel_DataChanged(object sender, EventArgs e)
        {
            var sr = subrecordPanel.GetSelectedSubrecord();
            if (sr != null)
                UpdateMainText(sr);
        }

        private void subrecordPanel_OnSubrecordChanged(object sender, RecordChangeEventArgs e)
        {
            if (e.Record is SubRecord)
            {
                if (e.Record.Parent is Record)
                    PluginTree.RefreshObject(e.Record.Parent);
                subrecordPanel.RefreshObject(e.Record);
            }
        }

        private void useNewSubrecordEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseOldSubRecordEditor = !useNewSubrecordEditorToolStripMenuItem.Checked;
        }

        private void hexModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseHexSubRecordEditor = hexModeToolStripMenuItem.Checked;
        }
    }
}