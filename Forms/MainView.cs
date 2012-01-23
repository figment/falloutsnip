using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Drawing;
using TESVSnip.Properties;
using Crownwood.Magic.Docking;
using TESVSnip.Forms;

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
        private Forms.StringsEditor stringEditor = null;
        private readonly MainViewMessageFilter msgFilter;
        private System.Threading.Timer statusTimer = null;
        private bool inRebuildSelection = false;

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
            this.dockingManagerExtender.AutomaticStatePersistence = global::TESVSnip.Properties.Settings.Default.AutoSaveDockingState;
            this.dockingManagerExtender.DockingManager.ContentHiding += delegate(Content c, System.ComponentModel.CancelEventArgs cea) { cea.Cancel = true; };
            this.dockingManagerExtender.DockingManager.ContextMenu += delegate(Crownwood.Magic.Menus.PopupMenu pm, System.ComponentModel.CancelEventArgs cea)
            {
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
                pm.MenuCommands.RemoveAt(pm.MenuCommands.Count - 1);
            };

            // Register message filter.
            msgFilter = new MainViewMessageFilter(this);
            Application.AddMessageFilter(msgFilter);

            InitializeToolStripFind();

            this.PluginTree.SelectionChanged += (o, e) => RebuildSelection();

            if (string.IsNullOrEmpty(global::TESVSnip.Properties.Settings.Default.DefaultSaveFolder)
                || !System.IO.Directory.Exists(global::TESVSnip.Properties.Settings.Default.DefaultSaveFolder))
            {
                this.SaveModDialog.InitialDirectory = Program.gameDataDir;
            }
            else
            {
                this.SaveModDialog.InitialDirectory = global::TESVSnip.Properties.Settings.Default.DefaultSaveFolder;
            }
            if (string.IsNullOrEmpty(global::TESVSnip.Properties.Settings.Default.DefaultOpenFolder)
                || !System.IO.Directory.Exists(global::TESVSnip.Properties.Settings.Default.DefaultOpenFolder))
            {
                this.OpenModDialog.InitialDirectory = Program.gameDataDir;
            }
            else
            {
                this.OpenModDialog.InitialDirectory = global::TESVSnip.Properties.Settings.Default.DefaultOpenFolder;
            }


            this.Icon = Properties.Resources.tesv_ico;

            if (!global::TESVSnip.Properties.Settings.Default.IsFirstTimeOpening)
            {
                Settings.GetWindowPosition("TESsnip", this);
            }
            else
            {
                Settings.SetWindowPosition("TESsnip", this);
                global::TESVSnip.Properties.Settings.Default.IsFirstTimeOpening = false;
                global::TESVSnip.Properties.Settings.Default.Save();
            }
            useWindowsClipboardToolStripMenuItem.Checked = global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard;
            noWindowsSoundsToolStripMenuItem.Checked = global::TESVSnip.Properties.Settings.Default.NoWindowsSounds;
            disableHyperlinksToolStripMenuItem.Checked = global::TESVSnip.Properties.Settings.Default.DisableHyperlinks;
            this.rtfInfo.DetectUrls = !global::TESVSnip.Properties.Settings.Default.DisableHyperlinks;
            saveStringsFilesToolStripMenuItem.Checked = global::TESVSnip.Properties.Settings.Default.SaveStringsFiles;

            Selection = new SelectionContext();
            Selection.formIDLookup = new dFormIDLookupI(LookupFormIDI);
            Selection.strLookup = new dLStringLookup(LookupFormStrings);
            Selection.formIDLookupR = new dFormIDLookupR(GetRecordByID);

            this.subrecordPanel.SetContext(Selection);
            InitializeLanguage();

            ClipboardChanged += (o, e) => RebuildSelection();
            Selection.RecordChanged += (o, a) => RebuildSelection();
            Selection.SubRecordChanged += (o, a) => RebuildSelection();

            PluginTree.SelectionChanged += new EventHandler(PluginTree_SelectionChanged);
            subrecordPanel.SelectionChanged += new EventHandler(subrecordPanel_SelectionChanged);
        }

        void subrecordPanel_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(subrecordPanel.SubRecord);
        }

        void PluginTree_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(PluginTree.SelectedRecord);
        }

        internal static bool HasClipboardData()
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent("TESVSnip");
            }
            else
            {
                return Clipboard != null;
            }
        }

        internal static bool HasClipboardData<T>()
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent(typeof(T).FullName);
            }
            else
            {
                return Clipboard is T;
            }
        }

        internal static object GetClipboardData()
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
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
            else
            {
                return s_clipboard;
            }
        }

        internal static T GetClipboardData<T>() where T : class
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                if (od != null)
                {
                    var clip = od.GetData(typeof(T).FullName);
                    return clip as T;
                }
                return default(T);
            }
            else
            {
                return s_clipboard as T;
            }
        }
        internal static void SetClipboardData(object value)
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
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

        void UpdateClipboardStatus()
        {
            RebuildSelection();
        }

        internal void LoadPlugin(string s)
        {
            Plugin p = new Plugin(s, false, GetRecordFilter(s));
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
            if (TESVSnip.Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM)
            {
                if (System.String.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        Resources.MainView_FirstTimeSkyrimLoad_ExcludeInquiry, Resources.FirstLoadOptions
                        , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
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
                bool applyfilter = false;
                if (Properties.Settings.Default.ApplyFilterToAllESM)
                    applyfilter = String.Compare(Path.GetExtension(s), ".esm", StringComparison.OrdinalIgnoreCase) == 0;
                else
                    applyfilter = String.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0;

                if (applyfilter && bAskToApplyFilter && !TESVSnip.Properties.Settings.Default.DontAskUserAboutFiltering)
                {
                    DialogResult result = MessageBox.Show(this, Resources.ESM_Large_File_Size_Inquiry, Resources.Filter_Options_Text
                        , MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    applyfilter = (result == DialogResult.Yes);
                }
                if (applyfilter)
                {
                    recFilter = TESVSnip.Properties.Settings.Default.FilteredESMRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                this.PluginTree.UpdateRoots();
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.CloseAllLoseChangesInquiry, Resources.WarningText, MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            PluginList.All.Records.Clear();
            PluginTree.UpdateRoots();
            this.subrecordPanel.Record = null;
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

        void RebuildSelection()
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
                    this.subrecordPanel.Record = null;
                    Selection.Record = null;
                    UpdateMainText("");
                    return;
                }

                bool hasClipboard = HasClipboardData();

                if (rec is Plugin)
                {
                    this.subrecordPanel.Record = null;
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
                    this.subrecordPanel.Record = Selection.Record as Record;
                    MatchRecordStructureToRecord();
                }
                else
                {
                    Selection.Record = null;
                    this.subrecordPanel.Record = null;
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
            if (this.PluginTree.ContainsFocus)
            {
                GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
                if (PluginTree.SelectedRecord.Parent != null)
                {
                    var parent = (BaseRecord)PluginTree.SelectedRecord.Parent;
                    var node = (BaseRecord)PluginTree.SelectedRecord;
                    parent.DeleteRecord(node);
                    PluginTree.RefreshObject(parent);
                }
            }
            else if (this.subrecordPanel.ContainsFocus)
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
                DialogResult result = MessageBox.Show(this, Resources.SavePluginWithFilterAppliedInquiry, Resources.WarningText, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
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
            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.CopySelectedRecord();
            }
            else if (this.subrecordPanel.ContainsFocus)
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
            Clipboard = sr.Select(ss => (SubRecord)ss.Clone()).ToArray();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false);
            return;
        }

        private void PasteFromClipboard(bool recordOnly)
        {
            if (!HasClipboardData())
            {
                MessageBox.Show(Resources.TheClipboardIsEmpty, Resources.ErrorText);
                return;
            }

            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.PasteFromClipboard(recordOnly);
            }
            else if (this.subrecordPanel.ContainsFocus)
            {
                this.subrecordPanel.PasteFromClipboard();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plugin p = new Plugin();
            PluginList.All.AddRecord(p);
            Record r = new Record();
            r.Name = "TES4";
            SubRecord sr = new SubRecord();
            sr.Name = "HEDR";
            sr.SetData(new byte[] { 0xD7, 0xA3, 0x70, 0x3F, 0xFA, 0x56, 0x0C, 0x00, 0x19, 0xEA, 0x07, 0xFF });
            r.AddRecord(sr);
            sr = new SubRecord();
            sr.Name = "CNAM";
            sr.SetData(TESVSnip.Encoding.CP1252.GetBytes("Default\0"));
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
            if (tn is Plugin) return (Plugin)tn;
            while (!(tn is Plugin) && tn != null) tn = tn.Parent;
            if (tn != null && tn is Plugin) return tn as Plugin;
            return tn != null && tn.Parent != null ? tn.Parent as Plugin : new Plugin();
        }

        private void PluginTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;
            if (PluginTree.SelectedRecord is Record)
            {
                var r = (Record)PluginTree.SelectedRecord;
                if (DialogResult.OK == HeaderEditor.Display(r))
                {
                    GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
                    UpdateMainText(((BaseRecord)PluginTree.SelectedRecord));
                }
            }
            else if (PluginTree.SelectedRecord is GroupRecord)
            {
                var gr = (GroupRecord)PluginTree.SelectedRecord;
                if (DialogResult.OK == GroupEditor.Display(gr))
                {
                    GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
                    UpdateMainText(((BaseRecord)PluginTree.SelectedRecord));
                }

            }
        }

        private SelectionContext GetSelectedContext()
        {
            return Selection;
            //context.Record = this.parentRecord
            //context.SubRecord = GetSelectedSubrecord();

        }

        private SubRecord GetSelectedSubrecord()
        {
            return this.subrecordPanel.GetSelectedSubrecord();
        }

        private IEnumerable<SubRecord> GetSelectedSubrecords()
        {
            return this.subrecordPanel.GetSelectedSubrecords();
        }

        private void insertRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = (BaseRecord)PluginTree.SelectedRecord;
            var p = new Record();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
        }

        private void insertSubrecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseRecord node = (BaseRecord)PluginTree.SelectedRecord;
            SubRecord p = new SubRecord();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
            RebuildSelection();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.toolStripIncrFind.Visible)
            {
                toolStripIncrFind.Visible = true;
                this.toolStripIncrFind.Focus();
                this.toolStripIncrFindText.Select();
                this.toolStripIncrFindText.SelectAll();
                this.toolStripIncrFindText.Focus();
            }
            else
            {
                this.toolStripIncrFind.Visible = false;
            }
        }

        private void TESsnip_FormClosing(object sender, FormClosingEventArgs e)
        {
            global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard = useWindowsClipboardToolStripMenuItem.Checked;
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

        private bool RecurseFormIDSearch(Rec rec, uint FormID, ref string edid)
        {
            if (rec is Record)
            {
                if (((Record)rec).FormID == FormID)
                {
                    edid = rec.DescriptiveName;
                    return true;
                }
            }
            else
            {
                foreach (Rec r in ((GroupRecord)rec).Records)
                {
                    if (RecurseFormIDSearch(r, FormID, ref edid)) return true;
                }
            }
            return false;
        }


        private string LookupFormIDI(uint id)
        {
            return LookupFormIDI(this.Selection, id);
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

        private string LookupFormIDS(string sid)
        {
            uint id;
            if (!uint.TryParse(sid, System.Globalization.NumberStyles.AllowHexSpecifier, null, out id))
                return "FormID was invalid";
            return LookupFormIDI(id);
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
                    MessageBox.Show(this, "No plugins available to edit", Resources.ErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                stringEditor = new Forms.StringsEditor();
                stringEditor.FormClosed += delegate(object a, FormClosedEventArgs args)
                {
                    CloseStringEditor();
                };
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
            if (this.stringEditor != null)
            {
                Forms.StringsEditor editor = this.stringEditor;
                this.stringEditor = null;
                try
                {
                    if (!editor.IsDisposed)
                        editor.Close();
                }
                catch { }

            }
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            FixMasters();
        }

        #region Enable Disable User Interface
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void ControlEnable(Control control, bool enable)
        {
            IntPtr ipenable = new IntPtr(enable ? 1 : 0);
            SendMessage(control.Handle, WM_SETREDRAW, ipenable, IntPtr.Zero);
        }
        private const int WM_SETREDRAW = 0x0b;

        void EnableUserInterface(bool enable)
        {
            //ControlEnable(this.splitHorizontal, enable);
            //ControlEnable(this.splitVertical, enable);
            ControlEnable(this.menuStrip1, enable);
            ControlEnable(this.toolStripIncrFind, enable);
            ControlEnable(this.toolStripIncrInvalidRec, enable);
        }
        #endregion
        #region Action

        readonly Action cancelBackgroundAction = null;
        volatile bool backgroundWorkCanceled = false;
        public void StartBackgroundWork(Action workAction, Action completedAction)
        {
            if (this.backgroundWorker1.IsBusy)
                return;

            EnableUserInterface(false);
            backgroundWorkCanceled = false;
            this.toolStripStatusProgressBar.ProgressBar.Value = this.toolStripStatusProgressBar.Minimum;
            this.toolStripStatusProgressBar.Visible = true;
            this.toolStripStopProgress.Visible = true;
            this.backgroundWorker1.RunWorkerAsync(new Action[] { workAction, completedAction });
        }

        public void UpdateBackgroundProgress(int percentProgress)
        {
            this.backgroundWorker1.ReportProgress(percentProgress);
        }

        public void CancelBackgroundProcess()
        {
            backgroundWorkCanceled = true;
            this.backgroundWorker1.CancelAsync();
        }
        public bool IsBackroundProcessCanceled()
        {
            return backgroundWorkCanceled;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Action[] actions = e.Argument as Action[];
            if (actions.Length > 0)
            {
                actions[0]();
            }
            if (actions.Length > 1)
            {
                e.Result = actions[1];
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.toolStripStatusProgressBar.Value = e.ProgressPercentage % this.toolStripStatusProgressBar.Maximum;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            EnableUserInterface(true);
            this.toolStripStatusProgressBar.Visible = false;
            this.toolStripStopProgress.Visible = false;
            if (e.Cancelled || e.Error != null)
                return;
            Action completedAction = e.Result as Action;
            if (completedAction != null) completedAction();
        }

        private void toolStripStopProgress_Click(object sender, EventArgs e)
        {
            CancelBackgroundProcess();
            if (cancelBackgroundAction != null)
                cancelBackgroundAction();
        }


        private void mergeRecordsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TESVSnip.Data.Records baseRecords = null;
            TESVSnip.Data.Records updateRecords = null;

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(TESVSnip.Data.Records));
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Select Base Record Structure";
                dlg.InitialDirectory = Program.settingsDir;
                dlg.FileName = "RecordStructure.xml";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                using (System.IO.FileStream fs = System.IO.File.OpenRead(dlg.FileName))
                {
                    baseRecords = xs.Deserialize(fs) as TESVSnip.Data.Records;
                }
            }
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Select Record Structure XML To Merge";
                dlg.InitialDirectory = System.IO.Path.GetTempPath();
                dlg.FileName = "RecordStructure.xml";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                using (System.IO.FileStream fs = System.IO.File.OpenRead(dlg.FileName))
                {
                    updateRecords = xs.Deserialize(fs) as TESVSnip.Data.Records;
                }
            }
            if (updateRecords != null && baseRecords != null)
            {

                var builder = new TESVSnip.Data.RecordBuilder();
                builder.MergeRecords(baseRecords.Items.OfType<TESVSnip.Data.RecordsRecord>()
                    , updateRecords.Items.OfType<TESVSnip.Data.RecordsRecord>());

                using (var dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.Title = "Select Record Structure To Save";
                    dlg.InitialDirectory = System.IO.Path.GetTempPath();
                    dlg.FileName = "RecordStructure.xml";
                    dlg.OverwritePrompt = false;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        using (System.IO.StreamWriter fs = System.IO.File.CreateText(dlg.FileName))
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
            List<string> groups = TESVSnip.Properties.Settings.Default.AllESMRecords != null
                ? TESVSnip.Properties.Settings.Default.AllESMRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
            groups.Sort();
            foreach (var plugin in PluginList.All.Records.OfType<Plugin>())
            {
                plugin.ForEach((r) =>
                {
                    if (r is GroupRecord)
                    {
                        var g = (GroupRecord)r;
                        var s = g.ContentsType;
                        if (!string.IsNullOrEmpty(s))
                        {
                            int idx = groups.BinarySearch(s);
                            if (idx < 0) { groups.Insert(~idx, s); modified = true; }
                        }
                    }
                });
            }
            RecordStructure.Load();
            var allRecords = RecordStructure.Records.Select((kvp) => kvp.Key).ToList();
            foreach (var str in allRecords)
            {
                int idx = groups.BinarySearch(str);
                if (idx < 0) { groups.Insert(~idx, str); modified = true; }
            }

            if (modified)
            {
                TESVSnip.Properties.Settings.Default.AllESMRecords = string.Join(";", groups.ToArray());
            }

            using (TESVSnip.Forms.LoadSettings settings = new TESVSnip.Forms.LoadSettings())
            {
                settings.ShowDialog(this);
            }
        }


        #region ToolStrip Check Toggle
        private void toolStripCheck_CheckStateChanged(object sender, EventArgs e)
        {
            var button = sender as System.Windows.Forms.ToolStripButton;
            button.Image = button.Checked
                ? global::TESVSnip.Properties.Resources.checkedbox
                : global::TESVSnip.Properties.Resources.emptybox
                ;
        }
        #endregion


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lookupFormidsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lookupFormidsToolStripMenuItem.Checked)
            {
                Selection.formIDLookup = new dFormIDLookupI(LookupFormIDI);
                Selection.strLookup = new dLStringLookup(LookupFormStrings);
                Selection.formIDLookupR = new dFormIDLookupR(GetRecordByID);
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
            if (this.PluginTree.SelectedRecord == null) return;
            BackgroundNonConformingRecordIncrementalSearch(this.PluginTree.SelectedRecord, true, toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecPrev_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null) return;
            BackgroundNonConformingRecordIncrementalSearch(this.PluginTree.SelectedRecord, false, toolStripIncrInvalidRecWrapAround.Checked);
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
            toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.DarkGray;
        }

        #endregion

        private void toolStripIncrInvalidRec_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void PluginTree_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

            }
        }




        private void useWindowsClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard =
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
                if (!Encoding.TryGetFontInfo(global::TESVSnip.Properties.Settings.Default.LocalizationName, out defLang))
                    defLang = new FontLangInfo(1252, 1033, 0);

                var rb = new RTF.RTFBuilder(RTF.RTFFont.Arial, 16, defLang.lcid, defLang.charset);
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

        static readonly System.Text.RegularExpressions.Regex linkRegex =
            new System.Text.RegularExpressions.Regex(
                "^(?:(?<text>[^#]*)#)?(?<type>[0-z][A-Z][A-Z][A-Z_]):(?<id>[0-9a-zA-Z]+)$"
            , System.Text.RegularExpressions.RegexOptions.None);

        private void rtfInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                var m = linkRegex.Match(e.LinkText);
                if (m.Success)
                {
                    var n = PluginTree.SelectedRecord ?? PluginTree.TopRecord;
                    var p = GetPluginFromNode(n);

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
            TESVSnip.Properties.Settings.Default.NoWindowsSounds =
                noWindowsSoundsToolStripMenuItem.Checked = !noWindowsSoundsToolStripMenuItem.Checked;
        }

        private void disableHyperlinksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            global::TESVSnip.Properties.Settings.Default.DisableHyperlinks =
                disableHyperlinksToolStripMenuItem.Checked = !disableHyperlinksToolStripMenuItem.Checked;
            this.rtfInfo.DetectUrls = !global::TESVSnip.Properties.Settings.Default.DisableHyperlinks;
        }

        #region Key Intercept Hack so Del does not override text box in find
        public class MainViewMessageFilter : IMessageFilter
        {
            public const int WM_CHAR = 0x102;
            public const int WM_KEYDOWN = 0x100;
            public const int WM_KEYUP = 0x101;


            private readonly MainView owner = null;

            public MainViewMessageFilter(MainView owner)
            {
                this.owner = owner;
            }

            public bool PreFilterMessage(ref Message m)
            {
                try { return this.owner.PreFilterMessage(ref m); }
                catch { }
                return true;
            }
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern ushort GetKeyState(VirtualKeyStates nVirtKey);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern ushort GetAsyncKeyState(VirtualKeyStates nVirtKey);

            internal enum VirtualKeyStates : int
            {
                VK_LBUTTON = 0x01,
                VK_RBUTTON = 0x02,
                VK_CANCEL = 0x03,
                VK_MBUTTON = 0x04,
                VK_LSHIFT = 0xA0,
                VK_RSHIFT = 0xA1,
                VK_LCONTROL = 0xA2,
                VK_RCONTROL = 0xA3,
                VK_LMENU = 0xA4,
                VK_RMENU = 0xA5,
                VK_LEFT = 0x25,
                VK_UP = 0x26,
                VK_RIGHT = 0x27,
                VK_DOWN = 0x28,
                VK_SHIFT = 0x10,
                VK_CONTROL = 0x11,
                VK_MENU = 0x12,
            }
            const ushort KEY_PRESSED = 0x8000;

            public static bool IsControlDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_CONTROL);
            }
            public static bool IsAltDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_MENU);
            }

        }

        internal bool PreFilterMessage(ref Message m)
        {
            // Intercept the left mouse button down message.
            if (m.Msg == MainViewMessageFilter.WM_KEYDOWN || m.Msg == MainViewMessageFilter.WM_KEYUP)
            {
                if (m.WParam == new IntPtr((int)Keys.Delete))
                {
                    if (this.toolStripIncrFindText.Focused)
                    {
                        m.WParam = new IntPtr((int)Keys.Oem1);
                        SendMessage(this.toolStripIncrFind.Handle, m.Msg, m.WParam, m.LParam);
                        return true;
                    }
                }
            }
            return false;
        }


        private void toolStripIncrFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Oem1)
            {
                if (this.toolStripIncrFindText.Focused)
                {
                    // total hack
                    if (this.toolStripIncrFindText.SelectionLength > 0)
                        this.toolStripIncrFindText.SelectedText = "";// delete selected text
                    else if (this.toolStripIncrFindText.SelectionStart + 1 <= this.toolStripIncrFindText.TextLength)
                    {
                        this.toolStripIncrFindText.SelectionLength = 1;
                        this.toolStripIncrFindText.SelectedText = "";// delete selected text
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }
        #endregion

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #region String Tools
        internal Dictionary<string, ToolStripMenuItem> languageToolBarItems = new Dictionary<string, ToolStripMenuItem>(StringComparer.InvariantCultureIgnoreCase);
        void InitializeLanguage()
        {
            languageToolBarItems.Add("English", englishToolStripMenuItem);
            languageToolBarItems.Add("Czech", czechToolStripMenuItem);
            languageToolBarItems.Add("French", frenchToolStripMenuItem);
            languageToolBarItems.Add("German", germanToolStripMenuItem);
            languageToolBarItems.Add("Italian", italianToolStripMenuItem);
            languageToolBarItems.Add("Spanish", spanishToolStripMenuItem);
            languageToolBarItems.Add("Russian", russianToolStripMenuItem);
        }

        void ReloadLanguageFiles()
        {
            foreach (Plugin p in PluginList.All.Records)
                p.ReloadStrings();
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
                kvp.Value.Checked = String.Compare(kvp.Key, Properties.Settings.Default.LocalizationName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private void languageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
            {
                if (e.ClickedItem == kvp.Value)
                {
                    if (global::TESVSnip.Properties.Settings.Default.LocalizationName != kvp.Key)
                    {
                        global::TESVSnip.Properties.Settings.Default.LocalizationName = kvp.Key;
                        ReloadLanguageFiles();
                    }
                    break;
                }
            }
        }

        private void stringLocalizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath) ||
                !System.IO.File.Exists(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath))
            {
                var result = MessageBox.Show(this, "Skyrim String Localizer is not found.\nWould you like to browse for it?"
                    , "Program Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.Title = "Select Record Structure XML To Merge";
                    dlg.FileName = "Skyrim String Localizer.exe";
                    if (dlg.ShowDialog(this) != DialogResult.OK)
                        return;
                    global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath = dlg.FileName;
                }
            }
            if (System.IO.File.Exists(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath))
            {
                try
                {
                    using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                    {
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath);
                        p.StartInfo = startInfo;
                        p.Start();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Resources.ErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool found = false;
            //stringLocalizerToolStripMenuItem.Enabled = false;
            if (!string.IsNullOrEmpty(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath))
            {
                if (System.IO.File.Exists(global::TESVSnip.Properties.Settings.Default.SkyrimLocalizerPath))
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
                stringLocalizerToolStripMenuItem.ToolTipText = "Skyrim String Localizer is not found.  Select to browse for it...";
            }
        }

        #endregion

        private void saveStringsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            global::TESVSnip.Properties.Settings.Default.SaveStringsFiles =
            saveStringsFilesToolStripMenuItem.Checked = !saveStringsFilesToolStripMenuItem.Checked;
        }

        private void reloadStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadLanguageFiles();
        }


        private void addMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var amfNewMaster = new TESVSnip.AddMasterForm())
            {
                if (amfNewMaster.ShowDialog(this) == DialogResult.OK)
                {
                    Plugin plugin = GetPluginFromNode(PluginTree.SelectedRecord);
                    if (plugin == null)
                    {
                        MessageBox.Show(this, "No plugin selected. Cannot continue.", "Missing Plugin", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    catch (System.ApplicationException ex)
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
            switch (MessageBox.Show("Would you like to reset your custom layout back to default layout?\n\r Remark: You have to restart application until new setting can take effect.", "Automatic State Persistence", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    this.dockingManagerExtender.ResetAutoPersistent(false);
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
            this.PluginTree.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.CollapseAll();
        }

        private void expandBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.ExpandAll(this.PluginTree.SelectedRecord);
        }

        private void collapseBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.CollapseAll(this.PluginTree.SelectedRecord);
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
                statusTimer = new System.Threading.Timer(
                    (o) => this.Invoke(new TimerCallback((object o2) => { toolStripStatusLabel.Text = ""; }), new object[]{""})
                    , "", TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(-1) );
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
            if (!DesignMode)
            {
                try
                {
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                    var attr = asm.GetCustomAttributes(true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                    if (attr != null) Text = attr.InformationalVersion;
                }
                catch{ }
            }
        }
    }
}
