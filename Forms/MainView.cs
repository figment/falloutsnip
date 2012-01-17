using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
using TESVSnip.Windows.Controls;
using Crownwood.Magic.Docking;

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
        private static TreeNode s_clipboardNode;
        static readonly System.Text.Encoding s_CP1252Encoding = System.Text.Encoding.GetEncoding(1252);


        private SelectionContext Selection;
        private Forms.StringsEditor stringEditor = null;
        OC.Windows.Forms.History<TreeNode> historyHandler;
        private MainViewMessageFilter msgFilter;

        #region Helper Tree Node Helper
        /// <summary>
        /// Tree node with override of ToString (for History)
        /// </summary>
        class SnipTreeNode : TreeNode
        {
            public SnipTreeNode() : base() { }
            public SnipTreeNode(string text) : base(text) { }
            public SnipTreeNode(string text, TreeNode[] children) : base(text, children) { }
            public SnipTreeNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex) { }
            public SnipTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children) : base(text, imageIndex, selectedImageIndex, children) { }
            public override string ToString()
            {
                return this.Text.ToString();
            }
        }
        #endregion


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
                    MessageBox.Show("Could not parse RecordStructure.xml. Record-at-once editing will be unavailable.\n" + ex.Message, "Warning");
                }
            }
            InitializeComponent();
            this.dockingManagerExtender.AutomaticStatePersistence = global::TESVSnip.Properties.Settings.Default.AutoSaveDockingState;

            // Register message filter.
            msgFilter = new MainViewMessageFilter(this);
            Application.AddMessageFilter(msgFilter);

            InitializeToolStripFind();
            InitializeSubrecordForm();

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

            UpdateToolStripSelection();
            InitializeToolStripRecords();
            InitializeLanguage();

            Selection.RecordChanged += delegate(object o, EventArgs a) { UpdateToolStripSelection(); };
            Selection.SubRecordChanged += delegate(object o, EventArgs a) { UpdateToolStripSelection(); };

            if (!DesignMode)
            {
                try
                {
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                    var attr = asm.GetCustomAttributes(true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                    if (attr != null)
                        this.Text = attr.InformationalVersion;
                }
                catch
                {
                }
            }
        }



        private bool HasClipboardData()
        {
            if (useWindowsClipboardToolStripMenuItem.Checked)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null ? od.GetDataPresent("TESVSnip") : false;
            }
            else
            {
                return Clipboard != null;
            }
        }

        private bool HasClipboardData<T>()
        {
            if (useWindowsClipboardToolStripMenuItem.Checked)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null ? od.GetDataPresent(typeof(T).FullName) : false;
            }
            else
            {
                return Clipboard is T;
            }
        }

        private object GetClipboardData()
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                var cliptype = od.GetData("TESVSnip");
                if (cliptype is string)
                {
                    return od.GetData(cliptype.ToString());
                }
                return null;
            }
            else
            {
                return s_clipboard;
            }
        }

        private T GetClipboardData<T>() where T : class
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                var clip = od.GetData(typeof(T).FullName);
                return clip as T;
            }
            else
            {
                return s_clipboard as T;
            }
        }
        private void SetClipboardData(object value)
        {
            if (global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard)
            {
                if (value is ICloneable)
                {
                    var ido = new DataObject();
                    var srFormat = value.GetType().FullName;
                    ido.SetData(srFormat, ((ICloneable)value).Clone());
                    ido.SetData("TESVSnip", srFormat);
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetDataObject(ido, true);
                }
            }
            else
            {
                if (s_clipboard != value)
                {
                    s_clipboard = value;
                    UpdateToolStripSelection();
                }
            }
        }

        public object Clipboard
        {
            get { return GetClipboardData(); }
            set { SetClipboardData(value); }
        }

        public TreeNode ClipboardNode
        {
            get { return s_clipboardNode; }
            set
            {
                if (s_clipboardNode != value)
                {
                    s_clipboardNode = value;
                    UpdateToolStripSelection();
                }
            }
        }

        void UpdateClipboardStatus()
        {
            UpdateToolStripSelection();
        }

        internal void LoadPlugin(string s)
        {
            Plugin p = new Plugin(s, false, GetRecordFilter(s));
            TreeNode tn = new SnipTreeNode(p.Name);
            CreatePluginTree(p, tn);
            PluginTree.Nodes.Add(tn);
            UpdateStringEditor();
            FixMasters();
            GC.Collect();
        }

        private string[] GetRecordFilter(string s)
        {
            string[] recFilter = null;
            bool applyfilter = false;
            bool bAskToApplyFilter = true;
            if (TESVSnip.Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM)
            {
                if (string.Compare(System.IO.Path.GetFileName(s), "skyrim.esm", true) == 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        @"This is the first time 'skyrim.esm' has been loaded.
The file is large size and takes significant memory to load.
Would you like to configure which Records to exclude?"
                        , "First Load Options", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        TESVSnip.Properties.Settings.Default.EnableESMFilter = true;
                        TESVSnip.Properties.Settings.Default.DontAskUserAboutFiltering = true;
                        using (TESVSnip.Forms.LoadSettings settings = new TESVSnip.Forms.LoadSettings())
                        {
                            result = settings.ShowDialog(this);
                            if (result == DialogResult.Cancel) // cancel will be same as No
                            {
                                TESVSnip.Properties.Settings.Default.EnableESMFilter = false;
                                TESVSnip.Properties.Settings.Default.DontAskUserAboutFiltering = true;
                            }

                        }
                        TESVSnip.Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                    }
                    else if (result == DialogResult.No)
                    {
                        TESVSnip.Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        TESVSnip.Properties.Settings.Default.DontAskUserAboutFiltering = true;
                    }
                    else
                    {
                        TESVSnip.Properties.Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        return recFilter;
                    }
                }
                bAskToApplyFilter = false;
            }
            if (TESVSnip.Properties.Settings.Default.EnableESMFilter)
            {
                if (TESVSnip.Properties.Settings.Default.ApplyFilterToAllESM)
                    applyfilter = string.Compare(System.IO.Path.GetExtension(s), ".esm", true) == 0;
                else
                    applyfilter = string.Compare(System.IO.Path.GetFileName(s), "skyrim.esm", true) == 0;

                if (applyfilter && bAskToApplyFilter && !TESVSnip.Properties.Settings.Default.DontAskUserAboutFiltering)
                {
                    DialogResult result = MessageBox.Show(this,
                                            @"The file is large size and takes significant memory to load.
Would you like to apply the record exclusions?"
                                            , "Filter Options", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    applyfilter = (result == DialogResult.Yes);
                }
                if (applyfilter)
                {
                    recFilter = TESVSnip.Properties.Settings.Default.FilteredESMRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            return recFilter;
        }
        private void WalkPluginTree(Rec r, TreeNode tn)
        {
            TreeNode tn2 = new SnipTreeNode(r.DescriptiveName);
            tn2.Tag = r;
            if (r is GroupRecord)
            {
                foreach (Rec r2 in ((GroupRecord)r).Records) WalkPluginTree(r2, tn2);
            }
            tn.Nodes.Add(tn2);
        }

        private void CreatePluginTree(Plugin p, TreeNode tn)
        {
            tn.Tag = p;
            foreach (Rec r in p.Records) WalkPluginTree(r, tn);
        }

        private void openNewPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            if (OpenModDialog.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string s in OpenModDialog.FileNames)
                {
                    LoadPlugin(s);
                }
                FixMasters();
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            if (MessageBox.Show("This will close all open plugins, and you will lose any unsaved changes.\n" +
                "Are you sure you wish to continue", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            PluginTree.Nodes.Clear();
            listSubrecord.DataSource = null;
            Clipboard = null;
            ClipboardNode = null;
            CloseStringEditor();
            UpdateMainText("");
            RebuildSelection();
            historyHandler.Clear();
            GC.Collect();
        }

        /// <summary>
        /// This routine assigns Structure definitions to subrecords
        /// </summary>
        private bool MatchRecordStructureToRecord()
        {
            if (Selection == null || Selection.Record == null)
                return false;
            return Selection.Record.MatchRecordStructureToRecord();
        }

        private void PluginTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            toolStripRecordText.Text = PluginTree.SelectedNode == null ? "" : PluginTree.SelectedNode.Text;
            if (historyHandler.CurrentItem != PluginTree.SelectedNode)
                historyHandler.CurrentItem = PluginTree.SelectedNode;
            RebuildSelection();
        }

        void RebuildSelection()
        {
            if (PluginTree.SelectedNode == null)
            {
                listSubrecord.DataSource = null;
                Selection.Plugin = null;
                UpdateMainText("");
                return;
            }

            bool hasClipboard = HasClipboardData();

            if (PluginTree.SelectedNode.Tag is Plugin)
            {
                listSubrecord.DataSource = null;
                Selection.Plugin = ((Plugin)PluginTree.SelectedNode.Tag);
                Selection.Record = null;
                UpdateMainText(Selection.Plugin);
                cutToolStripMenuItem.Enabled = false;
                copyToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = hasClipboard;
                insertRecordToolStripMenuItem.Enabled = true;
                insertSubrecordToolStripMenuItem.Enabled = false;
                toolStripRecordCopy.Enabled = false;
                toolStripRecordPaste.Enabled = hasClipboard;
            }
            else if (PluginTree.SelectedNode.Tag is Record)
            {
                cutToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Enabled = false;
                toolStripRecordCopy.Enabled = true;
                toolStripRecordPaste.Enabled = hasClipboard;
                insertRecordToolStripMenuItem.Enabled = false;
                insertSubrecordToolStripMenuItem.Enabled = true;
                Record r = (Record)PluginTree.SelectedNode.Tag;
                Selection.Plugin = GetPluginFromNode(PluginTree.SelectedNode);
                Selection.Record = r;
                listSubrecord.DataSource = r.SubRecords;
                MatchRecordStructureToRecord();
                UpdateMainText(Selection.Record);
            }
            else
            {
                Selection.Plugin = GetPluginFromNode(PluginTree.SelectedNode);
                Selection.Record = null;
                listSubrecord.DataSource = null;
                UpdateMainText(((BaseRecord)PluginTree.SelectedNode.Tag));
                cutToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Enabled = hasClipboard;
                toolStripRecordCopy.Enabled = true;
                toolStripRecordPaste.Enabled = hasClipboard;
                insertRecordToolStripMenuItem.Enabled = true;
                insertSubrecordToolStripMenuItem.Enabled = false;
            }
            Selection.SubRecord = GetSelectedSubrecord();
            UpdateToolStripSelection();
            listSubrecord.Refresh();
        }

        private void RefreshSelection()
        {
            if (PluginTree.SelectedNode == null)
                return;

            bool hasClipboard = HasClipboardData();
            //Enable and disable relevant menu items
            if (PluginTree.SelectedNode.Tag is Plugin)
            {
                UpdateMainText(((BaseRecord)PluginTree.SelectedNode.Tag));
                cutToolStripMenuItem.Enabled = false;
                copyToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = hasClipboard;
                toolStripRecordCopy.Enabled = false;
                toolStripRecordPaste.Enabled = hasClipboard;
                insertRecordToolStripMenuItem.Enabled = true;
                insertSubrecordToolStripMenuItem.Enabled = false;
            }
            else if (PluginTree.SelectedNode.Tag is Record)
            {
                Record r = (Record)PluginTree.SelectedNode.Tag;
                if (!r.Equals(Selection.Record))
                {
                    RebuildSelection();
                }
                else
                {
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = hasClipboard;
                    toolStripRecordCopy.Enabled = true;
                    toolStripRecordPaste.Enabled = hasClipboard;
                    insertRecordToolStripMenuItem.Enabled = false;
                    insertSubrecordToolStripMenuItem.Enabled = true;
                    UpdateMainText(((Record)PluginTree.SelectedNode.Tag));
                }
            }
            else
            {
                UpdateMainText(((BaseRecord)PluginTree.SelectedNode.Tag));
                cutToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Enabled = hasClipboard;
                toolStripRecordCopy.Enabled = false;
                toolStripRecordPaste.Enabled = false;
                insertRecordToolStripMenuItem.Enabled = true;
                insertSubrecordToolStripMenuItem.Enabled = false;
            }
            Selection.SubRecord = GetSelectedSubrecord();
            UpdateToolStripSelection();
            listSubrecord.Refresh();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            if (this.PluginTree.Focused)
            {
                if (PluginTree.SelectedNode.Parent != null)
                {
                    BaseRecord parent = (BaseRecord)PluginTree.SelectedNode.Parent.Tag;
                    BaseRecord node = (BaseRecord)PluginTree.SelectedNode.Tag;
                    parent.DeleteRecord(node);
                }
                GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
                PluginTree.SelectedNode.Remove();
            }
            else if (this.listSubrecord.Focused)
            {
                if (Selection.SelectedSubrecord)
                {
                    if (listSubrecord.SelectedIndices.Count != 1) return;
                    Selection.Record.SubRecords.RemoveAt(listSubrecord.SelectedIndices[0]);
                    listSubrecord.Refresh();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedNode == null)
            {
                MessageBox.Show("No plugin selected to save", "Error");
                return;
            }
            TreeNode tn = PluginTree.SelectedNode;
            while (!(tn.Tag is Plugin)) tn = tn.Parent;
            Plugin p = (Plugin)tn.Tag;
            if (p.Filtered)
            {
                DialogResult result = MessageBox.Show(this, @"This file has had a filter applied and contents potentially removed.  
Do you still want to save?", "Modified Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            if (SaveModDialog.ShowDialog(this) == DialogResult.OK)
            {
                p.Save(SaveModDialog.FileName);
            }
            if (p.Name != tn.Text)
            {
                tn.Text = p.Name;
                FixMasters();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            if (!Selection.SelectedSubrecord && PluginTree.SelectedNode != null && PluginTree.SelectedNode.Tag is Plugin)
            {
                MessageBox.Show("Cannot cut a plugin", "Error");
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
            if (Selection.SelectedSubrecord && !this.PluginTree.Focused)
                CopySelectedSubRecord();
            else CopySelectedTreeNode();
        }

        private void CopySelectedTreeNode()
        {
            if (PluginTree.SelectedNode.Tag is Plugin)
                MessageBox.Show("Cannot copy a plugin", "Error");
            else
                CopySelectedRecord();
        }

        private void CopySelectedSubRecord()
        {
            var sr = GetSelectedSubrecords();
            if (sr == null) return;
            Clipboard = sr.Select(ss => { return (SubRecord)ss.Clone(); }).ToArray();
            ClipboardNode = null;
            UpdateToolStripSelection();
        }
        private void CopySelectedRecord()
        {
            BaseRecord node = ((BaseRecord)PluginTree.SelectedNode.Tag).Clone();
            Clipboard = node;
            ClipboardNode = (TreeNode)PluginTree.SelectedNode.Clone();
            ClipboardNode.Tag = node;
            if (ClipboardNode.Nodes.Count > 0)
            {
                ClipboardNode.Nodes.Clear();
                foreach (Rec r in ((GroupRecord)node).Records) WalkPluginTree(r, ClipboardNode);
            }
            UpdateToolStripSelection();
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false);
            return;
        }

        private void PasteFromClipboard(bool recordOnly)
        {
            if (!ValidateMakeChange())
                return;
            if (!HasClipboardData())
            {
                MessageBox.Show("The clipboard is empty", "Error");
                return;
            }
            var clipboardObject = this.Clipboard;

            if (recordOnly && !(clipboardObject is Record || clipboardObject is GroupRecord))
                return;

            BaseRecord node = (BaseRecord)PluginTree.SelectedNode.Tag;
            if (clipboardObject is Plugin)
            {
                MessageBox.Show("Plugin merging has been disabled");
                return;
            }
            else if (clipboardObject is BaseRecord)
            {
                try
                {
                    var dstNode = PluginTree.SelectedNode;
                    var br = (BaseRecord)((BaseRecord)clipboardObject).Clone();
                    node.AddRecord(br);
                    if (ClipboardNode != null)
                    {
                        var newNode = (TreeNode)ClipboardNode.Clone();
                        newNode.Tag = br;

                        PluginTree.SelectedNode.Nodes.Add(newNode);
                        GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
                    }
                    else
                    {
                        string text = (br is Rec) ? ((Rec)br).DescriptiveName : br.Name;
                        var dstRecNode = new SnipTreeNode(text);
                        dstRecNode.Tag = br;
                        if (br is GroupRecord)
                        {
                            foreach (Rec r in ((GroupRecord)br).Records)
                                WalkPluginTree(r, dstRecNode);
                        }
                        dstNode.Nodes.Add(dstRecNode);
                        GetPluginFromNode(dstNode).InvalidateCache();
                    }
                    PluginTree_AfterSelect(null, null);
                }
                catch (TESParserException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (!recordOnly)
            {
                PasteSubRecord();
            }
        }

        void PasteSubRecord()
        {
            if (!ValidateMakeChange())
                return;

            if (!HasClipboardData<SubRecord[]>())
                return;

            try
            {
                BaseRecord br = (BaseRecord)PluginTree.SelectedNode.Tag;


                int insertIdx = listSubrecord.SelectedIndices.Count == 0 ? -1 : listSubrecord.GetFocusedItem();
                var nodes = GetClipboardData<SubRecord[]>();
                foreach (var clipSr in insertIdx < 0 ? nodes : nodes.Reverse()) // insert in revers
                {
                    SubRecord sr = clipSr.Clone() as SubRecord;
                    if (sr == null)
                        return;

                    if (br is Record)
                    {
                        try
                        {
                            if (insertIdx >= 0 && insertIdx < listSubrecord.Items.Count)
                            {
                                br.InsertRecord(insertIdx, sr);
                            }
                            else
                            {
                                br.AddRecord(sr);
                            }
                        }
                        catch (TESParserException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }

                RebuildSelection();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            Plugin p = new Plugin();
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
            TreeNode tn = new SnipTreeNode(p.Name);
            tn.Tag = p;
            TreeNode tn2 = new SnipTreeNode(r.DescriptiveName);
            tn2.Tag = r;
            tn.Nodes.Add(tn2);
            PluginTree.Nodes.Add(tn);
            UpdateStringEditor();
            FixMasters();
        }

        private Plugin GetPluginFromNode(TreeNode node)
        {
            TreeNode tn = node;
            if (tn.Tag is Plugin) return (Plugin)tn.Tag;
            while (!(tn.Tag is Plugin) && tn != null) tn = tn.Parent;
            if (tn != null && tn.Tag is Plugin) return tn.Tag as Plugin;
            return tn != null && tn.Parent != null ? tn.Parent.Tag as Plugin : new Plugin();
        }

        private void PluginTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (PluginTree.SelectedNode == null) return;
            if (PluginTree.SelectedNode.Tag is Record)
            {
                Record r = (Record)PluginTree.SelectedNode.Tag;
                if (DialogResult.OK == HeaderEditor.Display(r))
                {
                    GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
                    PluginTree.SelectedNode.Text = r.DescriptiveName;
                    UpdateMainText(((BaseRecord)PluginTree.SelectedNode.Tag));
                }
            }
            else if (PluginTree.SelectedNode.Tag is GroupRecord)
            {
                GroupRecord gr = (GroupRecord)PluginTree.SelectedNode.Tag;
                if (DialogResult.OK == GroupEditor.Display(gr))
                {
                    GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
                    PluginTree.SelectedNode.Text = gr.DescriptiveName;
                    UpdateMainText(((BaseRecord)PluginTree.SelectedNode.Tag));
                }

            }
        }

        void listSubrecord_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //listSubrecord_VirtualItemsSelectionRangeChanged(sender, e);
        }

        private void listSubrecord_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            UpdateSubRecordSelection(e);
        }
        void UpdateSubRecordSelection(ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            var n = this.listSubrecord.SelectedIndices.Count;
            var oldSel = Selection.SubRecord;
            var newSel = GetSelectedSubrecord();
            if (oldSel == newSel)
                return;
            // Update the current selection
            Selection.SubRecord = newSel;

            if (Selection.SubRecord == null)
            {
                UpdateMainText("");
                return;
            }

            var context = GetSelectedContext();
            var sr = Selection.SubRecord;
            UpdateMainText(sr);
            pasteToolStripMenuItem.Enabled = false;
            copyToolStripMenuItem.Enabled = true;
            cutToolStripMenuItem.Enabled = true;
            deleteToolStripMenuItem.Enabled = true;
            insertRecordToolStripMenuItem.Enabled = false;
            insertSubrecordToolStripMenuItem.Enabled = false;
            UpdateToolStripSelection();
        }


        private void UpdateToolStripSelection()
        {
            if (Selection.Record != null)
            {
                toolStripInsertRecord.Enabled = true;
                toolStripPasteSubrecord.Enabled = HasClipboardData<SubRecord[]>();
                toolStripRecordCopy.Enabled = true;
                toolStripRecordPaste.Enabled = false;
            }
            else
            {
                toolStripInsertRecord.Enabled = false;
                toolStripPasteSubrecord.Enabled = false;
                toolStripRecordCopy.Enabled = false;
                toolStripRecordPaste.Enabled = Selection.Plugin != null ? HasClipboardData() : false;
            }
            if (Selection.SubRecord != null)
            {
                toolStripEditSubrecordHex.Enabled = true;
                toolStripEditSubrecord.Enabled = true;
                toolStripCopySubrecord.Enabled = true;
                toolStripMoveRecordDown.Enabled = true;
                toolStripMoveRecordUp.Enabled = true;
                toolStripDeleteRecord.Enabled = true;
            }
            else
            {
                toolStripEditSubrecordHex.Enabled = false;
                toolStripEditSubrecord.Enabled = false;
                toolStripCopySubrecord.Enabled = false;
                toolStripMoveRecordDown.Enabled = false;
                toolStripMoveRecordUp.Enabled = false;
                toolStripDeleteRecord.Enabled = false;
            }
            int idx = this.listSubrecord.GetFocusedItem();
            if (idx <= 0)
            {
                toolStripMoveRecordUp.Enabled = false;
            }
            if (this.listSubrecord.ItemCount != 0)
            {
                if (idx == this.listSubrecord.DataSource.Count - 1)
                {
                    toolStripMoveRecordDown.Enabled = false;
                }
            }
        }
        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            UpdateSubRecordSelection(null);
            //EditSelectedSubrecord();
        }

        void EditSelectedSubrecord()
        {
            var context = GetSelectedContext();
            //context.SubRecord = GetSelectedSubrecord();
            var sr = GetSelectedSubrecord();
            if (sr == null) return;

            if (useNewSubrecordEditorToolStripMenuItem.Checked
                && sr.Structure != null
                && sr.Structure.elements != null
                && sr.Structure.elements[0].type != ElementValueType.Blob && !sr.Structure.UseHexEditor)
            {
                Form re;
                try
                {
                    if (global::TESVSnip.Properties.Settings.Default.UseOldSubRecordEditor)
                    {
                        var p = context.Plugin;
                        var r = context.Record;
                        var formIDLookup = new dFormIDLookupS(p.LookupFormIDS);
                        var formIDScan = new dFormIDScan(this.FormIDScan);
                        var strIDLookup = new dLStringLookup(p.LookupFormStrings);
                        re = new MediumLevelRecordEditor(sr, sr.Structure, formIDLookup, formIDScan, strIDLookup);
                    }
                    else
                    {
                        re = new NewMediumLevelRecordEditor(context.Plugin, context.Record, sr, sr.Structure);
                    }                    
                }
                catch
                {
                    MessageBox.Show("Subrecord doesn't seem to conform to the expected structure.", "Error");
                    re = null;
                }
                if (re != null)
                {
                    if (DialogResult.OK == re.ShowDialog(this))
                    {
                        UpdateMainText(sr);
                        if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
                        {
                            context.Record.SetDescription(" (" + sr.GetStrData() + ")");
                            PluginTree.SelectedNode.Text = context.Record.DescriptiveName;
                        }
                        //listSubrecord.SelectedItems[0].SubItems[1].Text = sr.Size.ToString() + " *";
                        listSubrecord.Refresh();
                    }
                    return;
                }
            }
            if (hexModeToolStripMenuItem.Checked)
            {
                using (var dlg = new HexDataEdit(sr.Name, sr.GetData(), LookupFormIDS))
                {
                    if (DialogResult.OK == dlg.ShowDialog(this))
                    {
                        sr.SetData(HexDataEdit.result);
                        sr.Name = HexDataEdit.resultName;
                        UpdateMainText(sr);
                        listSubrecord.Refresh();
                    }
                }
            }
            else
            {
                new DataEdit(sr.Name, sr.GetData()).ShowDialog(this);
                if (!DataEdit.Canceled)
                {
                    sr.SetData(DataEdit.result);
                    sr.Name = DataEdit.resultName;
                    UpdateMainText(sr);
                    listSubrecord.Refresh();
                }
            }
            MatchRecordStructureToRecord();
            if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
            {
                context.Record.UpdateShortDescription();
                PluginTree.SelectedNode.Text = context.Record.DescriptiveName;
            }
        }
        void EditSelectedSubrecordHex()
        {
            try
            {
                var sr = GetSelectedSubrecord();
                if (sr == null)
                    return;
                using (var form = new HexDataEdit(sr.Name, sr.GetData(), LookupFormIDS))
                {
                    DialogResult result = form.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        sr.SetData(HexDataEdit.result);
                        sr.Name = HexDataEdit.resultName;
                        UpdateMainText(sr);

                        MatchRecordStructureToRecord();
                        if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
                        {
                            Selection.Record.UpdateShortDescription();
                            PluginTree.SelectedNode.Text = Selection.Record.DescriptiveName;
                        }
                        listSubrecord.Refresh();
                    }
                }
            }
            catch
            {

            }
        }

        private void UpdateSelectionContext()
        {
            Selection.Reset();
            Selection.SubRecord = GetSelectedSubrecord();
        }

        private SelectionContext GetSelectedContext()
        {
            return Selection;
            //context.Record = this.parentRecord
            //context.SubRecord = GetSelectedSubrecord();

        }

        private SubRecord GetSelectedSubrecord()
        {
            //if (listSubrecord.SelectedIndices.Count < 1) return null;
            //int idx = listSubrecord.SelectedIndices[0];
            //return listSubrecord.DataSource[idx] as SubRecord;
            int idx = listSubrecord.GetFocusedItem();
            if (listSubrecord.DataSource != null)
            {
                if (idx >= 0 && idx < listSubrecord.DataSource.Count)
                    return listSubrecord.DataSource[idx] as SubRecord;
            }
            return null;
        }

        private List<SubRecord> GetSelectedSubrecords()
        {
            if (listSubrecord.SelectedIndices.Count < 1) return null;
            List<SubRecord> recs = new List<SubRecord>();
            foreach (int idx in listSubrecord.SelectedIndices)
            {
                var sr = listSubrecord.DataSource[idx] as SubRecord;
                if (sr != null) recs.Add(sr);
            }
            return recs;
        }

        private void insertRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;


            BaseRecord node = (BaseRecord)PluginTree.SelectedNode.Tag;
            Record p = new Record();
            node.AddRecord(p);
            TreeNode tn = new SnipTreeNode(p.Name);
            tn.Tag = p;
            PluginTree.SelectedNode.Nodes.Add(tn);
            GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
        }

        private void insertSubrecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            BaseRecord node = (BaseRecord)PluginTree.SelectedNode.Tag;
            SubRecord p = new SubRecord();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
            PluginTree_AfterSelect(null, null);
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
            //global::TESVSnip.Properties.Settings.Default.MainHorzSplitterPct = splitHorizontal.SplitterDistance;
            //global::TESVSnip.Properties.Settings.Default.MainVertSplitterPct = splitVertical.SplitterDistance;
            global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard = useWindowsClipboardToolStripMenuItem.Checked;

            PluginTree.Nodes.Clear();
            Clipboard = null;
            ClipboardNode = null;
            Selection.Plugin = null;
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
            if (PluginTree.SelectedNode == null)
            {
                MessageBox.Show("No plugin selected to save", "Error");
                return;
            }
            if (!ValidateMakeChange())
                return;
            TreeNode tn = PluginTree.SelectedNode;
            while (!(tn.Tag is Plugin)) tn = tn.Parent;
            tn.Tag = null;
            PluginTree.Nodes.Remove(tn);
            UpdateStringEditor();
            UpdateMainText("");
            FixMasters();
            RebuildSelection();
            GC.Collect();
        }

        private bool DragDropInProgress;
        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (listSubrecord.SelectedIndices.Count < 1 || e.Button != MouseButtons.Left) return;
            DragDropInProgress = true;
            listSubrecord.DoDragDrop(listSubrecord.SelectedIndices[0] + 1, DragDropEffects.Move);
        }

        private void listView1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            System.Drawing.Point p = listSubrecord.PointToClient(Form.MousePosition);
            ListViewItem lvi = listSubrecord.GetItemAt(p.X, p.Y);
            if (lvi == null) listSubrecord.SelectedIndices.Clear();
            else lvi.Selected = true;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            int toswap = (int)e.Data.GetData(typeof(int)) - 1;
            if (toswap == -1) return;
            var rec = Selection.Record;
            SubRecord sr = rec.SubRecords[toswap];
            if (listSubrecord.SelectedIndices.Count == 0)
            {
                rec.SubRecords.RemoveAt(toswap);
                rec.SubRecords.Add(sr);
            }
            else if (listSubrecord.SelectedIndices.Count >= 1)
            {
                int moveto = listSubrecord.SelectedIndices[0];
                if (toswap == moveto) return;
                rec.SubRecords.RemoveAt(toswap);
                rec.SubRecords.Insert(moveto, sr);
            }
            else return;
            PluginTree_AfterSelect(null, null);
            return;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (!DragDropInProgress) return;
            e.Effect = DragDropEffects.Move;
            DragDropInProgress = false;
        }


        private void FixMasters()
        {
            var plugins = PluginTree.Nodes.OfType<TreeNode>().Select(x => x.Tag).OfType<Plugin>().ToArray();
            foreach (var plugin in plugins)
                plugin.UpdateReferences(plugins);
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
            if (context != null && context.Plugin != null)
                context.Plugin.LookupFormID(id);
            return "No selection";
        }

        private Record GetRecordByID(uint id)
        {
            if (Selection != null && Selection.Plugin != null)
                    return Selection.Plugin.GetRecordByID(id);
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
            string value = default(string);
            if (Selection != null && Selection.Plugin != null)
                return Selection.Plugin.LookupFormStrings(id);
            return value;
        }

        private string[] FormIDScan(string type)
        {
            List<string> ret = new List<string>();
            if (Selection != null && Selection.Plugin != null)
            {
                foreach (var pair in Selection.Plugin.EnumerateRecords(type))
                {
                    ret.Add(pair.Value.DescriptiveName);
                    ret.Add(pair.Key.ToString("X8"));
                }
            }
            return ret.ToArray();
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
                MessageBox.Show("Could not parse RecordStructure.xml. Record-at-once editing will be unavailable.\n" + ex.Message, "Warning");
            }
        }

        private void editStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (stringEditor == null)
            {
                List<Plugin> plugins = new List<Plugin>();
                foreach (TreeNode node in PluginTree.Nodes)
                {
                    Plugin plugin = node.Tag as Plugin;
                    if (plugin == null)
                        continue;
                    plugins.Add(plugin);
                }

                if (plugins.Count == 0)
                {
                    MessageBox.Show(this, "No plugins available to edit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                List<Plugin> plugins = new List<Plugin>();
                foreach (TreeNode node in PluginTree.Nodes)
                {
                    Plugin plugin = node.Tag as Plugin;
                    if (plugin == null)
                        continue;
                    plugins.Add(plugin);
                }
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

        private void PluginTree_Enter(object sender, EventArgs e)
        {
            PluginTree_AfterSelect(sender, null);
        }

        private void PluginTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            PluginTree_AfterSelect(sender, e);
        }

        #region SubRecord Manipulation
        private void InitializeSubrecordForm()
        {
            this.listSubrecord.Columns.Clear();
            this.listSubrecord.AddBindingColumn("Name", "Name", 50);
            this.listSubrecord.AddBindingColumn("Size", "Size", 40);
            this.listSubrecord.AddBindingColumn("IsValid", "*", 20, new Func<SubRecord, string>(a => a.IsValid ? "*" : ""));
            this.listSubrecord.AddBindingColumn("Description", "Description", 100);
        }

        private bool ValidateMakeChange()
        {
            return true;
        }

        private void toolStripInsertRecord_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            try
            {
                BaseRecord br = (BaseRecord)PluginTree.SelectedNode.Tag;

                if (br is Record)
                {
                    if (listSubrecord.SelectedIndices.Count == 1)
                    {
                        int idx = listSubrecord.SelectedIndices[0];
                        if (idx < 0 || idx >= (listSubrecord.Items.Count - 1))
                            return;

                        Record r = (Record)br;
                        SubRecord p = new SubRecord();
                        r.InsertRecord(idx, p);
                    }
                    else
                    {
                        SubRecord p = new SubRecord();
                        br.AddRecord(p);
                    }

                }
                else
                {
                    SubRecord p = new SubRecord();
                    br.AddRecord(p);
                }
                PluginTree_AfterSelect(null, null);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripDeleteRecord_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            var rec = Selection.Record;
            if (rec != null)
            {
                if (listSubrecord.SelectedIndices.Count < 1) return;
                rec.SubRecords.RemoveAt(listSubrecord.SelectedIndices[0]);
            }
            Selection.SubRecord = GetSelectedSubrecord();
            MatchRecordStructureToRecord();
            RefreshSelection();
        }

        private void toolStripMoveRecordUp_Click(object sender, EventArgs e)
        {
            if (listSubrecord.SelectedIndices.Count < 1) return;
            int idx = listSubrecord.SelectedIndices[0];
            if (idx < 1 || idx >= (listSubrecord.Items.Count))
                return;

            var rec = Selection.Record;
            SubRecord sr = rec.SubRecords[idx];
            rec.SubRecords.RemoveAt(idx);
            rec.SubRecords.Insert(idx - 1, sr);

            listSubrecord.ClearSelection();
            listSubrecord.SelectItem(idx - 1);
            listSubrecord.FocusItem(idx - 1);
            listSubrecord.EnsureVisible(idx - 1);

            Selection.SubRecord = GetSelectedSubrecord();
            MatchRecordStructureToRecord();
            RefreshSelection();
        }

        private void toolStripMoveRecordDown_Click(object sender, EventArgs e)
        {
            if (listSubrecord.SelectedIndices.Count < 1) return;
            int idx = listSubrecord.SelectedIndices[0];
            if (idx < 0 || idx >= (listSubrecord.Items.Count - 1))
                return;

            var rec = Selection.Record;
            SubRecord sr = rec.SubRecords[idx];
            rec.SubRecords.RemoveAt(idx);
            rec.SubRecords.Insert(idx + 1, sr);

            listSubrecord.ClearSelection();
            listSubrecord.SelectItem(idx + 1);
            listSubrecord.FocusItem(idx + 1);
            listSubrecord.EnsureVisible(idx + 1);


            Selection.SubRecord = GetSelectedSubrecord();
            MatchRecordStructureToRecord();
            RefreshSelection();
        }

        private void toolStripEditSubrecord_Click(object sender, EventArgs e)
        {
            EditSelectedSubrecord();
        }

        private void listSubrecord_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EditSelectedSubrecord();
        }

        private void toolStripEditSubrecordHex_Click(object sender, EventArgs e)
        {
            EditSelectedSubrecordHex();
        }

        #endregion

        private void toolStripCopySubrecord_Click(object sender, EventArgs e)
        {
            CopySelectedSubRecord();
        }

        private void toolStripPasteSubrecord_Click(object sender, EventArgs e)
        {
            PasteSubRecord();
        }

        private void listSubrecord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control && !e.Alt && !e.Shift)
            {
                CopySelectedSubRecord();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.V && e.Control && !e.Alt && !e.Shift)
            {
                PasteSubRecord();
                e.Handled = true;
            }
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

        Action cancelBackgroundAction = null;
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
            foreach (var plugin in PluginTree.Nodes.OfType<TreeNode>().Select(n => n.Tag).OfType<Plugin>())
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
            BackgroundNonConformingRecordIncrementalSearch(null, true, toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecPrev_Click(object sender, EventArgs e)
        {
            BackgroundNonConformingRecordIncrementalSearch(null, false, toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecRestart_Click(object sender, EventArgs e)
        {
            BackgroundNonConformingRecordIncrementalSearch(PluginTree.Nodes.Count > 0 ? PluginTree.Nodes[0] : null, true, false);
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

        private void contexMenuRecordCopy_Click(object sender, EventArgs e)
        {
            CopySelectedTreeNode();
        }

        private void contextMenuRecord_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var tn = contextMenuRecord.Tag as TreeNode;
            if (tn == null) tn = PluginTree.SelectedNode;
            if (tn == null)
            {
                e.Cancel = true;
                return;
            }
            if (tn.Tag is Plugin)
            {
                contextMenuRecordCopy.Enabled = false;
                contextMenuRecordCopy.AutoToolTip = true;
                contextMenuRecordCopy.ToolTipText = "Cannot copy a plugin";
                contextMenuRecordCopyTo.Enabled = false;
            }
            else
            {
                contextMenuRecordCopy.AutoToolTip = false;
                contextMenuRecordCopy.Enabled = true;
                contextMenuRecordCopy.ToolTipText = "Copy Record to Clipboard";
                contextMenuRecordCopyTo.Enabled = (PluginTree.GetNodeCount(false) > 1);
            }
            contextMenuRecordAddMaster.Visible = false;
            contextMenuRecordCopyTo.DropDownItems.Clear();

            var srcPlugin = GetPluginFromNode(tn);
            foreach (TreeNode n in PluginTree.Nodes)
            {
                var plugin = n.Tag as BaseRecord;
                if (plugin == null) continue;
                if (srcPlugin.Equals(plugin)) continue;

                var tsi = new System.Windows.Forms.ToolStripButton(n.Text);
                tsi.Tag = new object[] { tn.Tag, tn, plugin, n };
                var sz = TextRenderer.MeasureText(n.Text, contextMenuRecordCopyTo.Font);
                if (sz.Width > tsi.Width)
                    tsi.Width = sz.Width;
                tsi.AutoSize = true;
                contextMenuRecordCopyTo.DropDownItems.Add(tsi);
            }

            if (srcPlugin.Equals(tn.Tag))
            {
                string[] masters = srcPlugin.GetMasters();
                Array.Sort<string>(masters, StringComparer.InvariantCultureIgnoreCase);

                foreach (var item in contextMenuRecordAddMaster.DropDownItems.OfType<ToolStripButton>().Where(x => !x.Equals(browseToolStripMenuItem)).ToArray())
                    contextMenuRecordAddMaster.DropDownItems.Remove(item);

                foreach (TreeNode n in PluginTree.Nodes)
                {
                    var plugin = n.Tag as Plugin;
                    if (plugin == null) continue;
                    if (srcPlugin.Equals(plugin)) continue; // ignore self
                    if (Array.BinarySearch(masters, plugin.Name, StringComparer.InvariantCultureIgnoreCase) >= 0) // ignore masters
                        continue;

                    var tsi = new System.Windows.Forms.ToolStripButton(n.Text);
                    tsi.Tag = new object[] { tn.Tag, tn, plugin, n };
                    var sz = TextRenderer.MeasureText(n.Text, contextMenuRecordCopyTo.Font);
                    if (sz.Width > tsi.Width)
                        tsi.Width = sz.Width;
                    tsi.AutoSize = true;
                    contextMenuRecordAddMaster.DropDownItems.Add(tsi);
                }
                contextMenuRecordAddMaster.Visible = true;
            }
        }
        private void contextMenuRecord_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            contextMenuRecordCopyTo.DropDownItems.Clear();
            foreach (var item in contextMenuRecordAddMaster.DropDownItems.OfType<ToolStripButton>()
                .Where(x => !x.Equals(browseToolStripMenuItem)).ToArray())
                contextMenuRecordAddMaster.DropDownItems.Remove(item);
        }


        private void PluginTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuRecord.Tag = e.Node;
                contextMenuRecord.Show(PluginTree.PointToScreen(e.Location));
            }
        }

        private void PluginTree_OnContextMenuKey(object sender, EventArgs e)
        {
            contextMenuRecord.Show(PluginTree.PointToScreen(new System.Drawing.Point(PluginTree.Width / 4, PluginTree.Height / 4)));
        }

        private void contextMenuRecordCopyTo_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                var nodes = e.ClickedItem.Tag as object[];
                var src = nodes[0] as BaseRecord;
                var srcNode = nodes[1] as TreeNode;
                var dst = nodes[2] as BaseRecord;
                var dstNode = nodes[3] as TreeNode;
                if (src != null && dst != null && dstNode != null && srcNode != null)
                {
                    var dstRec = src.Clone() as Rec;
                    var dstRecNode = (TreeNode)srcNode.Clone();
                    if (dstRec != null && dstRecNode != null)
                    {
                        dstRecNode.Tag = dstRec;
                        if (dstRecNode.Nodes.Count > 0)
                        {
                            dstRecNode.Nodes.Clear();
                            foreach (Rec r in ((GroupRecord)dstRec).Records)
                                WalkPluginTree(r, dstRecNode);
                        }
                        dst.AddRecord(dstRec);
                        dstNode.Nodes.Add(dstRecNode);
                        //var tn = new SnipTreeNode(dstRec.DescriptiveName);
                        //tn.Tag = dstRec;
                        //dstNode.Nodes.Add(tn);
                        GetPluginFromNode(dstRecNode).InvalidateCache();
                    }
                }
                PluginTree_AfterSelect(null, null);
            }
            catch
            {

            }
        }

        private void contextMenuRecordDelete_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure?", "Delete Node", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
            {
                if (!ValidateMakeChange())
                    return;
                if (PluginTree.SelectedNode.Parent != null)
                {
                    BaseRecord parent = (BaseRecord)PluginTree.SelectedNode.Parent.Tag;
                    BaseRecord node = (BaseRecord)PluginTree.SelectedNode.Tag;
                    parent.DeleteRecord(node);
                }
                GetPluginFromNode(PluginTree.SelectedNode).InvalidateCache();
                PluginTree.SelectedNode.Remove();
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

        TreeNode GetFirstPluginNode()
        {
            return PluginTree.Nodes.Count == 0 ? null : PluginTree.Nodes[0];
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
                    var n = PluginTree.SelectedNode;
                    while (n != null && n.Parent != null)
                        n = n.Parent;
                    if (n == null) n = GetFirstPluginNode();

                    // Search current plugin and then wrap around.  
                    //   Should do it based on master plugin list first.
                    var type = m.Groups["type"].Value;
                    var searchContext = new SearchContext();
                    searchContext.rectype = type == "XXXX" ? null : type;
                    searchContext.text = m.Groups["id"].Value;
                    searchContext.type = SearchType.FormID;
                    searchContext.tn = n;
                    searchContext.wrapAround = true;
                    searchContext.partial = false;
                    searchContext.forward = true;
                    searchContext.first = true;
                    var node = PerformSearch(searchContext);
                    if (node != null)
                        PluginTree.SelectedNode = node;
                }
            }
            catch
            {

            }
        }

        #region ToolStrip Record Handlers

        void InitializeToolStripRecords()
        {
            historyHandler = new OC.Windows.Forms.History<TreeNode>(
                toolStripRecordBack, toolStripRecordNext
                , global::TESVSnip.Properties.Settings.Default.MaxHistoryItem);
            historyHandler.AllowDuplicates = true;
            historyHandler.GotoItem += new EventHandler<OC.Windows.Forms.HistoryEventArgs<TreeNode>>(historyHandler_GotoItem);

        }

        void historyHandler_GotoItem(object sender, OC.Windows.Forms.HistoryEventArgs<TreeNode> e)
        {
            PluginTree.SelectedNode = e.Item;
        }

        private void toolStripRecordCopy_Click(object sender, EventArgs e)
        {
            CopySelectedRecord();
        }
        private void toolStripRecordPaste_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(true);
        }

        private void MainView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && e.Alt && !e.Control && !e.Shift)
            {
                toolStripRecordBack.PerformButtonClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right && e.Alt && !e.Control && !e.Shift)
            {
                toolStripRecordNext.PerformButtonClick();
                e.Handled = true;
            }
        }

        private void historyNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripRecordNext.PerformButtonClick();

        }

        private void historyBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripRecordBack.PerformButtonClick();
        }

        #endregion

        private void noWindowsSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TESVSnip.Properties.Settings.Default.NoWindowsSounds =
                noWindowsSoundsToolStripMenuItem.Checked = !noWindowsSoundsToolStripMenuItem.Checked;
        }

        private void listSubrecord_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            UpdateSubRecordSelection(null);
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


            private MainView owner = null;

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
            if (m.Msg == MainViewMessageFilter.WM_KEYDOWN || m.Msg == MainViewMessageFilter.WM_CHAR || m.Msg == MainViewMessageFilter.WM_KEYUP)
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
            foreach (var p in this.PluginTree.Nodes.OfType<TreeNode>().Select(x => x.Tag as Plugin).OfType<Plugin>())
            {
                if (p != null) p.ReloadStrings();
            }
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var kvp in languageToolBarItems)
                kvp.Value.Checked = string.Compare(kvp.Key, global::TESVSnip.Properties.Settings.Default.LocalizationName, true) == 0;
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
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static int SearchPath(string lpPath, string lpFileName, string lpExtension, out string lpBuffer, out string lpFilePart)
        {
            List<string> pathsToSearch = new List<string>();
            lpBuffer = "";
            lpFilePart = "";

            if (lpPath == null)
            {
                string currentWorkingFolder = Environment.CurrentDirectory;
                string path = System.Environment.GetEnvironmentVariable("path");

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager");
                object safeProcessSearchModeObject = key.GetValue("SafeProcessSearchMode");
                if (safeProcessSearchModeObject != null)
                {
                    int safeProcessSearchMode = (int)safeProcessSearchModeObject;
                    if (safeProcessSearchMode == 1)
                    {
                        // When the value of this registry key is set to "1", 
                        // SearchPath first searches the folders that are specified in the system path, 
                        // and then searches the current working folder. 
                        pathsToSearch.AddRange(Environment.GetEnvironmentVariable("PATH").Split(new char[] { Path.PathSeparator }, StringSplitOptions.None));
                        pathsToSearch.Add(currentWorkingFolder);
                    }
                    else
                    {
                        // When the value of this registry entry is set to "0", 
                        // the computer first searches the current working folder, 
                        // and then searches the folders that are specified in the system path. 
                        // The system default value for this registry key is "0".
                        pathsToSearch.Add(currentWorkingFolder);
                        pathsToSearch.AddRange(Environment.GetEnvironmentVariable("PATH").Split(new char[] { Path.PathSeparator }, StringSplitOptions.None));
                    }
                }
                else
                {
                    // Default 0 case
                    pathsToSearch.Add(currentWorkingFolder);
                    pathsToSearch.AddRange(Environment.GetEnvironmentVariable("PATH").Split(new char[] { Path.PathSeparator }, StringSplitOptions.None));
                }
            }
            else
            {
                // Path was provided, use it
                pathsToSearch.Add(lpPath);
            }

            FileInfo foundFile = SearchPath(pathsToSearch, lpExtension, lpFileName);
            if (foundFile != null)
            {
                lpBuffer = Path.Combine(foundFile.DirectoryName, foundFile.Name);
                lpFilePart = foundFile.Name;

            }

            return lpBuffer.Length;
        }

        private static FileInfo SearchPath(List<string> paths, string extension, string fileNamePart)
        {
            string fileName = fileNamePart + extension;
            foreach (string path in paths)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                var fileInfo = dir.GetFiles().Where(file => string.Compare(file.Name, fileName, true) == 0);
                if (fileInfo.Any())
                    return fileInfo.First();
            }
            return null;
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
            if (this.PluginTree.SelectedNode != null)
                this.PluginTree.SelectedNode.ExpandAll();
        }

        private void collapseBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedNode != null)
                this.PluginTree.SelectedNode.Collapse(false);
        }

        private void addMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (TESVsnip.AddMasterForm amfNewMaster = new TESVsnip.AddMasterForm())
            {
                if (amfNewMaster.ShowDialog(this) == DialogResult.OK)
                {
                    Plugin plugin = GetPluginFromNode(PluginTree.SelectedNode);
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
                            PluginTree_AfterSelect(null, null);
                        }
                    }
                    catch (System.ApplicationException ex)
                    {
                        MessageBox.Show(this, ex.Message, "Missing Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void contextMenuRecordAddMaster_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag == null)
                return;

            try
            {
                var nodes = e.ClickedItem.Tag as object[];
                var src = nodes[0] as Plugin;
                var srcNode = nodes[1] as TreeNode;
                var dst = nodes[2] as Plugin;
                var dstNode = nodes[3] as TreeNode;
                if (src != null && dst != null && dstNode != null && srcNode != null)
                {
                    if (src.AddMaster(dst.Name))
                    {
                        FixMasters();
                        PluginTree_AfterSelect(null, null);
                    }
                }
            }
            catch { }
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
    }
}
