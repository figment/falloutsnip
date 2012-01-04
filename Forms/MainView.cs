using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TESVSnip
{
    internal delegate string dFormIDLookupS(string id);
    internal delegate string dFormIDLookupI(uint id);
    internal delegate Record dFormIDLookupR(uint id);
    internal delegate string dLStringLookup(uint id);
    internal delegate string[] dFormIDScan(string type);
    internal partial class MainView : Form
    {
        private static object s_clipboard;
        private static TreeNode s_clipboardNode;
        private SelectionContext Selection;
        private Plugin[] FormIDLookup;
        private uint[] Fixups;
        private Forms.StringsEditor stringEditor = null;

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

        OC.Windows.Forms.History<TreeNode> historyHandler;

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

            InitializeToolStripFind();
            InitializeSubrecordForm();

            if (string.IsNullOrEmpty(global::TESVSnip.Properties.Settings.Default.DefaultSaveFolder) 
                || !System.IO.Directory.Exists(global::TESVSnip.Properties.Settings.Default.DefaultSaveFolder) )
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
                splitHorizontal.SplitterDistance = global::TESVSnip.Properties.Settings.Default.MainHorzSplitterPct;
                splitVertical.SplitterDistance = global::TESVSnip.Properties.Settings.Default.MainVertSplitterPct;
            }
            else
            {
                Settings.SetWindowPosition("TESsnip", this);
                global::TESVSnip.Properties.Settings.Default.IsFirstTimeOpening = false;
                global::TESVSnip.Properties.Settings.Default.MainHorzSplitterPct = splitHorizontal.SplitterDistance;
                global::TESVSnip.Properties.Settings.Default.MainVertSplitterPct = splitVertical.SplitterDistance;
                global::TESVSnip.Properties.Settings.Default.Save();
            }
            useWindowsClipboardToolStripMenuItem.Checked = global::TESVSnip.Properties.Settings.Default.UseWindowsClipboard;

            Selection = new SelectionContext();
            Selection.formIDLookup = new dFormIDLookupI(LookupFormIDI);
            Selection.strLookup = new dLStringLookup(LookupFormStrings);
            Selection.formIDLookupR = new dFormIDLookupR(GetRecordByID);

            UpdateToolStripSelection();
            InitializeToolStripRecords();

            Selection.RecordChanged += delegate(object o, EventArgs a) { UpdateToolStripSelection(); };
            Selection.SubRecordChanged += delegate(object o, EventArgs a){ UpdateToolStripSelection(); };
            
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
            set { SetClipboardData(value);}
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
                            result = settings.ShowDialog();
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
            if (OpenModDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string s in OpenModDialog.FileNames)
                {
                    LoadPlugin(s);
                }
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            if (MessageBox.Show("This will close all open plugins, and you will lose any unsaved changes.\n" +
                "Are you sure you wish to continue", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            PluginTree.Nodes.Clear();
            Clipboard = null;
            ClipboardNode = null;
            CloseStringEditor();
            UpdateMainText("");
            RebuildSelection();
            historyHandler.Clear();
            GC.Collect();
        }

        private static void MatchRecordAddConditionals(Dictionary<int, Conditional> conditions, SubRecord sr, ElementStructure[] ess)
        {
            int offset = 0;
            byte[] data = sr.GetReadonlyData();
            for (int j = 0; j < ess.Length; j++)
            {
                var essCondID = ess[j].CondID;
                if (essCondID != 0)
                {
                    switch (ess[j].type)
                    {
                        case ElementValueType.Int:
                        case ElementValueType.FormID:
                            conditions[essCondID] = new Conditional(ElementValueType.Int, TypeConverter.h2si(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]));
                            offset += 4;
                            break;
                        case ElementValueType.UInt:
                            conditions[essCondID] = new Conditional(ElementValueType.UInt, (uint)TypeConverter.h2i(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]));
                            offset += 4;
                            break;
                        case ElementValueType.Float:
                            conditions[essCondID] = new Conditional(ElementValueType.Float, TypeConverter.h2f(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]));
                            offset += 4;
                            break;
                        case ElementValueType.Short:
                            conditions[essCondID] = new Conditional(ElementValueType.Short, (int)TypeConverter.h2ss(data[offset], data[offset + 1]));
                            offset += 2;
                            break;
                        case ElementValueType.UShort:
                            conditions[essCondID] = new Conditional(ElementValueType.UShort, TypeConverter.h2s(data[offset], data[offset + 1]));
                            offset += 2;
                            break;
                        case ElementValueType.SByte:
                            conditions[essCondID] = new Conditional(ElementValueType.SByte, (sbyte)data[offset]);
                            offset++;
                            break;
                        case ElementValueType.Byte:
                            conditions[essCondID] = new Conditional(ElementValueType.Byte, (int)data[offset]);
                            offset++;
                            break;
                        case ElementValueType.String:
                            {
                                string s = "";
                                while (data[offset] != 0) s += (char)data[offset++];
                                offset++;
                                conditions[essCondID] = new Conditional(ElementValueType.String, s);
                            }
                            break;
                        case ElementValueType.fstring:
                            conditions[essCondID] = new Conditional(ElementValueType.String, sr.GetStrData());
                            break;
                        case ElementValueType.BString:
                            {
                                int len = TypeConverter.h2s(data[offset], data[offset + 1]);
                                string s = System.Text.Encoding.ASCII.GetString(data, offset + 2, len);
                                offset = offset + (2 + len);
                                conditions[essCondID] = new Conditional(ElementValueType.String, s);
                            } break;
                        case ElementValueType.Str4:
                            {
                                string s = System.Text.Encoding.ASCII.GetString(data, offset, 4);
                                offset += 4;
                                conditions[essCondID] = new Conditional(ElementValueType.String, s);
                            } break;
                        case ElementValueType.LString:
                            conditions[essCondID] = new Conditional(ElementValueType.Int, TypeConverter.h2si(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]));
                            offset += 4;
                            break;
                        default:
                            throw new ApplicationException();
                    }
                }
                else
                {
                    switch (ess[j].type)
                    {
                        case ElementValueType.Int:
                        case ElementValueType.UInt:
                        case ElementValueType.FormID:
                        case ElementValueType.Float:
                            offset += 4;
                            break;
                        case ElementValueType.UShort:
                        case ElementValueType.Short:
                            offset += 2;
                            break;
                        case ElementValueType.SByte:
                        case ElementValueType.Byte:
                            offset++;
                            break;
                        case ElementValueType.String:
                            while (data[offset] != 0) offset++;
                            offset++;
                            break;
                        case ElementValueType.fstring:
                            break;
                        case ElementValueType.LString:
                            {
                                // Try to guess if string or string index.  Do not know if the external string checkbox is set or not in this code
                                var d = new ArraySegment<byte>(data, offset, data.Length - offset);
                                bool isString = TypeConverter.IsLikelyString(d);
                                uint id = TypeConverter.h2i(d);
                                if (!isString)
                                {
                                    offset += 4;
                                }
                                else
                                {
                                    while (data[offset] != 0) offset++;
                                    offset++;
                                }
                            } break;
                        case ElementValueType.BString:
                            int len = TypeConverter.h2s(data[offset], data[offset + 1]);
                            offset += 2 + len;
                            break;
                        case ElementValueType.Str4:
                            offset += 4;
                            break;
                        default:
                            throw new ApplicationException();
                    }
                }
            }
        }
        private static bool MatchRecordCheckCondition(Dictionary<int, Conditional> conditions, SubrecordStructure ss)
        {
            if (ss.Condition == CondType.Exists)
            {
                if (conditions.ContainsKey(ss.CondID)) return true;
                else return false;
            }
            else if (ss.Condition == CondType.Missing)
            {
                if (conditions.ContainsKey(ss.CondID)) return false;
                else return true;
            }
            if (!conditions.ContainsKey(ss.CondID)) return false;
            Conditional cond = conditions[ss.CondID];
            switch (cond.type)
            {
                case ElementValueType.SByte:
                case ElementValueType.Byte:
                case ElementValueType.UShort:
                case ElementValueType.Short:
                case ElementValueType.Int:
                case ElementValueType.UInt:
                case ElementValueType.FormID:
                    {
                        int i = Convert.ToInt32(cond.value), i2;
                        if (!int.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }
                case ElementValueType.Float:
                    {
                        float i = (float)cond.value, i2;
                        if (!float.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }
                case ElementValueType.Str4:
                case ElementValueType.fstring:
                case ElementValueType.BString:
                case ElementValueType.String:
                    {
                        string s = (string)cond.value;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return s == ss.CondOperand;
                            case CondType.Not: return s != ss.CondOperand;
                            case CondType.StartsWith: return s.StartsWith(ss.CondOperand);
                            case CondType.EndsWith: return s.EndsWith(ss.CondOperand);
                            case CondType.Contains: return s.Contains(ss.CondOperand);
                            default: return false;
                        }
                    }
                case ElementValueType.LString:
                    {
                        int i = (int)cond.value, i2;
                        if (!int.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }

                default: return false;
            }
        }

#if true


        class LoopContext
        {
            public enum LoopEvalResult
            {
                Failed, // Failed to properly match
                NoMatches, // no matches
                Success, // all matched
                Partial, // some matched
            }

            public int idx;
            public int matches;

            public int ssidx;
            public SubrecordBase[] sss;

            public LoopContext(int start, SubrecordBase[] sss)
            {
                this.idx = start;
                this.ssidx = 0;
                this.sss = sss;
                this.matches = 0;
            }
        }

        private LoopContext.LoopEvalResult InnerLoop(SubRecord[] subs, Dictionary<int, Conditional> conditions, LoopContext context)
        {
            while (true)
            {
                if (context.idx >= subs.Length || context.ssidx >= context.sss.Length)
                    return LoopContext.LoopEvalResult.Success;

                var ssb = context.sss[context.ssidx];
                var sb = subs[context.idx];
                if (ssb is SubrecordGroup)
                {
                    var sg = ssb as SubrecordGroup;
                    var newcontext = new LoopContext(context.idx, sg.elements);
                    LoopContext.LoopEvalResult result = InnerLoop(subs, conditions, newcontext);
                    if (context.idx == newcontext.idx)
                    {
                        if (ssb.optional > 0 || (ssb.repeat > 0 && context.matches > 0))
                        {
                            ++context.ssidx;
                            context.matches = 0;
                            continue;
                        }
                    }
                    else if (result == LoopContext.LoopEvalResult.Success)
                    {
                        if (ssb.repeat == 0)
                            ++context.ssidx;
                        else
                            ++context.matches;
                        context.idx = newcontext.idx;
                        continue;
                    }
                    break;
                }
                else if (ssb is SubrecordStructure)
                {
                    var ss = (SubrecordStructure)ssb;
                    if (ss.Condition != CondType.None && !MatchRecordCheckCondition(conditions, ss))
                        ++context.ssidx;

                    if (sb.Name == ss.name && (ss.size == 0 || ss.size == sb.Size))
                    {
                        sb.AttachStructure(ss);
                        if (ss.ContainsConditionals)
                        {
                            try
                            {
                                MatchRecordAddConditionals(conditions, sb, ss.elements);
                            }
                            catch { }
                        }
                        ++context.idx;
                        if (ss.repeat == 0)
                        {
                            ++context.ssidx;
                            context.matches = 0;
                        }
                        else
                        {
                            // keep ss context and try again
                            ++context.matches;
                        }
                        continue;
                    }
                    else
                    {
                        if (ss.optional > 0 || (ss.repeat > 0 && context.matches > 0))
                        {
                            ++context.ssidx;
                            context.matches = 0;
                            continue;
                        }
                        else
                        {
                            // true failure
                            break;
                        }
                    }
                }
            }
            return LoopContext.LoopEvalResult.Failed;
        }

        /// <summary>
        /// This routine assigns Structure definitions to subrecords
        /// </summary>
        private bool MatchRecordStructureToRecord()
        {
            return MatchRecordStructureToRecord(Selection.Record);
        }
        private bool MatchRecordStructureToRecord(Record parentRecord)
        {
            try
            {
                if (parentRecord == null || RecordStructure.Records == null) return false;
                if (!RecordStructure.Records.ContainsKey(parentRecord.Name)) return false;
                var subrecords = new List<SubrecordStructure>();
                var sss = RecordStructure.Records[parentRecord.Name].subrecordTree;
                var subs = parentRecord.SubRecords.ToArray();
                foreach (var sub in subs) sub.DetachStructure();

                Dictionary<int, Conditional> conditions = new Dictionary<int, Conditional>();
                var context = new LoopContext(0, sss);
                var result = InnerLoop(subs, conditions, context);
                if (result == LoopContext.LoopEvalResult.Success && context.idx == subs.Length)
                {
                    return true;
                }
            }
            catch{}
            return false;
        }
#else
        private struct LoopBlock
        {
            public readonly int start;
            public readonly int end;

            public LoopBlock(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
        }

        /// <summary>
        /// This routine assigns Structure definitions to subrecords
        /// </summary>
        private bool MatchRecordStructureToRecord()
        {
            return MatchRecordStructureToRecord(Selection.Record);
        }
        private bool MatchRecordStructureToRecord(Record parentRecord)
        {
            try
            {
                if (parentRecord == null || RecordStructure.Records == null) return false;
                if (!RecordStructure.Records.ContainsKey(parentRecord.Name)) return false;
                var subrecords = new List<SubrecordStructure>();
                var sss = RecordStructure.Records[parentRecord.Name].subrecords;
                var subs = parentRecord.SubRecords.ToArray();
                foreach (var sub in subs) sub.DetachStructure();
                int subi = 0, ssi = 0;
                Stack<LoopBlock> repeats = new Stack<LoopBlock>();
                Dictionary<int, Conditional> conditions = new Dictionary<int, Conditional>();
                while (subi < subs.Length && ssi < sss.Length)
                {
                    var ss = sss[ssi];
                    var sb = subs[subi];
                    if (ss.Condition != CondType.None && !MatchRecordCheckCondition(conditions, ss))
                    {
                        ssi++;
                        continue;
                    }
                    if (sb.Name == ss.name && (ss.size == 0 || ss.size == sb.Size))
                    {
                        sb.AttachStructure(ss);
                        if (ss.repeat > 0)
                        {
                            if (repeats.Count == 0 || repeats.Peek().start != ssi) repeats.Push(new LoopBlock(ssi, ssi + ss.repeat));
                        }
                        if (ss.ContainsConditionals)
                        {
                            try
                            {
                                MatchRecordAddConditionals(conditions, sb, ss.elements);
                            }
                            catch { }
                        }
                        subi++;
                        ssi++;
                    }
                    else if (repeats.Count > 0 && repeats.Peek().start == ssi)
                    {
                        ssi = repeats.Pop().end;
                    }
                    else if (sss[ssi].optional > 0)
                    {
                        ssi += sss[ssi].optional;
                    }
                    else return false;
                    if (repeats.Count > 0 && repeats.Peek().end == ssi) ssi = repeats.Peek().start;
                }
                return (subi == subs.Length);
            }
            catch 
            {
                return false;
            }           
        }
#endif

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
                return;

            bool hasClipboard = HasClipboardData();

            FindMasters();
            if (PluginTree.SelectedNode.Tag is Plugin)
            {
                listSubrecord.DataSource = null;
                Selection.Plugin = ((Plugin)PluginTree.SelectedNode.Tag);
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
                Selection.Record = r;
                listSubrecord.DataSource = r.SubRecords;
                MatchRecordStructureToRecord();
                UpdateMainText(Selection.Record);
            }
            else
            {
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
            if (Selection.SelectedSubrecord)
            {
                if (listSubrecord.SelectedIndices.Count != 1) return;
                Selection.Record.SubRecords.RemoveAt(listSubrecord.SelectedIndices[0]);
                listSubrecord.Refresh();

            }
            else
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

            if (SaveModDialog.ShowDialog() == DialogResult.OK)
            {
                p.Save(SaveModDialog.FileName);
            }
            if (p.Name != tn.Text) tn.Text = p.Name;
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
            if (Selection.SelectedSubrecord)
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
            if (listSubrecord.SelectedIndices.Count != 1) return;
            int idx = listSubrecord.SelectedIndices[0];
            var sr = (SubRecord)listSubrecord.DataSource[idx];

            Clipboard = sr.Clone();
            ClipboardNode = null;
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

            if (recordOnly && !(clipboardObject is Record || clipboardObject is GroupRecord) )
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
                        PluginTree.SelectedNode.Nodes.Add(ClipboardNode);
                        ClipboardNode = (TreeNode)ClipboardNode.Clone();
                        ClipboardNode.Tag = Clipboard;
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
            sr.SetData(System.Text.Encoding.ASCII.GetBytes("Default\0"));
            r.AddRecord(sr);
            p.AddRecord(r);
            TreeNode tn = new SnipTreeNode(p.Name);
            tn.Tag = p;
            TreeNode tn2 = new SnipTreeNode(r.DescriptiveName);
            tn2.Tag = r;
            tn.Nodes.Add(tn2);
            PluginTree.Nodes.Add(tn);
            UpdateStringEditor();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the current selection
            Selection.SubRecord = GetSelectedSubrecord();
            if (Selection.SubRecord == null)
            {
                UpdateMainText("");
                return;
            }

            var context = GetSelectedContext();
            var sr = Selection.SubRecord;
            if (sr.Structure != null && sr.Structure.elements != null)
            {
                UpdateMainText(sr);
            }
            else
            {
                UpdateMainText("[Subrecord data]" + Environment.NewLine
                    + "String: " + sr.GetStrData() + Environment.NewLine + Environment.NewLine
                    + "Hex:" + Environment.NewLine + sr.GetHexData());
            }
            toolStripRecordCopy.Enabled = false;
            toolStripRecordPaste.Enabled = false;
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
            }
            else
            {
                toolStripInsertRecord.Enabled = false;
                toolStripPasteSubrecord.Enabled = false;
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
                MediumLevelRecordEditor re;
                try
                {
                    re = new MediumLevelRecordEditor(sr, sr.Structure, LookupFormIDS, FormIDScan, LookupFormStrings);
                }
                catch
                {
                    MessageBox.Show("Subrecord doesn't seem to conform to the expected structure.", "Error");
                    re = null;
                }
                if (re != null)
                {
                    re.ShowDialog();
                    UpdateMainText(sr.GetFormattedData(context));
                    if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
                    {
                        context.Record.DescriptiveName = " (" + sr.GetStrData() + ")";
                        PluginTree.SelectedNode.Text = context.Record.DescriptiveName;
                    }
                    //listSubrecord.SelectedItems[0].SubItems[1].Text = sr.Size.ToString() + " *";
                    listSubrecord.Refresh();
                    return;
                }
            }
            if (hexModeToolStripMenuItem.Checked)
            {
                new HexDataEdit(sr.Name, sr.GetData(), LookupFormIDS).ShowDialog();
                if (!HexDataEdit.Canceled)
                {
                    sr.SetData(HexDataEdit.result);
                    sr.Name = HexDataEdit.resultName;
                    UpdateMainText("[Subrecord data]" + Environment.NewLine + sr.GetHexData());
                    listSubrecord.Refresh();
                }
            }
            else
            {
                new DataEdit(sr.Name, sr.GetData()).ShowDialog();
                if (!DataEdit.Canceled)
                {
                    sr.SetData(DataEdit.result);
                    sr.Name = DataEdit.resultName;
                    UpdateMainText("[Subrecord data]" + Environment.NewLine + sr.GetStrData());
                    listSubrecord.Refresh();
                }
            }
            MatchRecordStructureToRecord();
            if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
            {
                context.Record.DescriptiveName = " (" + sr.GetStrData() + ")";
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
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        sr.SetData(HexDataEdit.result);
                        sr.Name = HexDataEdit.resultName;
                        UpdateMainText("[Subrecord data]" + Environment.NewLine + sr.GetHexData());
                        listSubrecord.Refresh();
                    }
                }
                MatchRecordStructureToRecord();
                if (sr.Name == "EDID" && listSubrecord.SelectedIndices[0] == 0)
                {
                    Selection.Record.DescriptiveName = " (" + sr.GetStrData() + ")";
                    PluginTree.SelectedNode.Text = Selection.Record.DescriptiveName;
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
            foreach ( int idx in listSubrecord.SelectedIndices )
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
            global::TESVSnip.Properties.Settings.Default.MainHorzSplitterPct = splitHorizontal.SplitterDistance;
            global::TESVSnip.Properties.Settings.Default.MainVertSplitterPct = splitVertical.SplitterDistance;
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


        private void FindMasters()
        {
            TreeNode tn = PluginTree.SelectedNode;
            while (tn.Parent != null) tn = tn.Parent;
            Plugin p = (Plugin)tn.Tag;
            Plugin[] plugins = new Plugin[PluginTree.Nodes.Count];
            for (int i = 0; i < plugins.Length; i++) plugins[i] = (Plugin)PluginTree.Nodes[i].Tag;

            List<string> masters = new List<string>();
            if (p.Records.Count > 0 && p.Records[0].Name == "TES4")
            {
                foreach (SubRecord sr in ((Record)p.Records[0]).SubRecords)
                {
                    if (sr.Name == "MAST") masters.Add(sr.GetStrData().ToLowerInvariant());
                }
            }
            FormIDLookup = new Plugin[masters.Count + 1];
            Fixups = new uint[masters.Count + 1];
            for (int i = 0; i < masters.Count; i++)
            {
                for (int j = 0; j < plugins.Length; j++)
                {
                    if (masters[i] == plugins[j].Name.ToLowerInvariant())
                    {
                        FormIDLookup[i] = plugins[j];
                        uint fixup = 0;
                        if (plugins[j].Records.Count > 0 && plugins[j].Records[0].Name == "TES4")
                        {
                            foreach (SubRecord sr in ((Record)plugins[j].Records[0]).SubRecords)
                            {
                                if (sr.Name == "MAST") fixup++;
                            }
                        }
                        Fixups[i] = fixup;
                        break;
                    }
                }
            }
            FormIDLookup[masters.Count] = p;
            Fixups[masters.Count] = (uint)masters.Count;
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
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > FormIDLookup.Length)
                return "FormID was invalid";

            Record r;
            if (context.Plugin != null)
            {

                if (context.Plugin.TryGetRecordByID(id, out r))
                    return r.DescriptiveName;
            }

            // what is this doing?
            Plugin p = FormIDLookup[FormIDLookup.Length - 1];
            if (p.TryGetRecordByID(id, out r))
                return r.DescriptiveName;
            id &= 0xffffff;

            if (pluginid < FormIDLookup.Length && FormIDLookup[pluginid] != null)
            {
                p = FormIDLookup[pluginid];
                id += Fixups[pluginid] << 24;
                if (p.TryGetRecordByID(id, out r))
                    return r.DescriptiveName;
                return "No match";
            }
            else
            {
                return "Master not loaded";
            }            
        }
        
        private Record GetRecordByID(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > FormIDLookup.Length)
                return null;
            Record r;
            // What is this check for???
            if (FormIDLookup[FormIDLookup.Length - 1].TryGetRecordByID(id, out r))
                return r;
            id &= 0xffffff;
            if (pluginid >= FormIDLookup.Length || FormIDLookup[pluginid] == null)
                return null;
            id += Fixups[pluginid] << 24;
            if (FormIDLookup[pluginid].TryGetRecordByID(id, out r))
                return r;
            return null;
        }

        private string LookupFormIDS(string sid)
        {
            uint id;
            if (!uint.TryParse(sid, System.Globalization.NumberStyles.AllowHexSpecifier, null, out id))
            {
                return "FormID was invalid";
            }
            return LookupFormIDI(id);
        }

        private string LookupFormStrings(uint id)
        {
            string value = default(string);
            foreach (var plugin in FormIDLookup)
            {
                if (plugin == null) continue;

                if (plugin.Strings.TryGetValue(id, out value))
                    break;
                if (plugin.DLStrings.TryGetValue(id, out value))
                    break;
                if (plugin.ILStrings.TryGetValue(id, out value))
                    break;
            }
            return value;
        }


        private void FormIDScanRecurse(Rec r, uint match, uint mask, Dictionary<uint, string> table, string type)
        {
            Record r2 = r as Record;
            if (r2 != null)
            {
                if (r2.Name == type && (r2.FormID & 0xff000000) == match)
                {
                    table[(r2.FormID & 0xffffff) | mask] = r2.DescriptiveName;
                }
            }
            else
            {
                GroupRecord gr = (GroupRecord)r;
                if (gr.groupType == 0 && gr.ContentsType != type) return;
                foreach (Rec r3 in gr.Records)
                {
                    FormIDScanRecurse(r3, match, mask, table, type);
                }
            }
        }
        private void FormIDScanRecurse2(Rec r, Dictionary<uint, string> table, string type)
        {
            Record r2 = r as Record;
            if (r2 != null)
            {
                if (r2.Name == type)
                {
                    table[r2.FormID] = r2.DescriptiveName;
                }
            }
            else
            {
                GroupRecord gr = (GroupRecord)r;
                if (gr.groupType == 0 && gr.ContentsType != type) return;
                foreach (Rec r3 in gr.Records)
                {
                    FormIDScanRecurse2(r3, table, type);
                }
            }
        }
        private string[] FormIDScan(string type)
        {
            Dictionary<uint, string> list = new Dictionary<uint, string>();
            for (int i = 0; i < FormIDLookup.Length - 1; i++)
            {
                if (FormIDLookup[i] == null) continue;
                if (FormIDLookup[i].Records.Count < 2 || FormIDLookup[i].Records[0].Name != "TES4") continue;
                uint match = 0;
                foreach (SubRecord sr in ((Record)FormIDLookup[i].Records[0]).SubRecords) if (sr.Name == "MAST") match++;
                match <<= 24;
                uint mask = (uint)i << 24;
                for (int j = 1; j < FormIDLookup[i].Records.Count; j++) FormIDScanRecurse(FormIDLookup[i].Records[j], match, mask, list, type);
            }
            for (int j = 1; j < FormIDLookup[FormIDLookup.Length - 1].Records.Count; j++) FormIDScanRecurse2(FormIDLookup[FormIDLookup.Length - 1].Records[j], list, type);

            string[] ret = new string[list.Count * 2];
            int count = 0;
            foreach (KeyValuePair<uint, string> pair in list)
            {
                ret[count++] = pair.Value;
                ret[count++] = pair.Key.ToString("X8");
            }
            return ret;
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
            var sr = GetSelectedSubrecords();
            if (sr == null) return;

            Clipboard = sr.Select( ss => { return (SubRecord)ss.Clone(); }).ToArray();
            ClipboardNode = null;
        }

        private void toolStripPasteSubrecord_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;

            if (!HasClipboardData<SubRecord[]>())
                return;

            try
            {
                BaseRecord br = (BaseRecord)PluginTree.SelectedNode.Tag;

                
                int insertIdx = listSubrecord.GetSelectionIndices().Length == 0 ? -1 : listSubrecord.GetFocusedItem();
                var nodes =  GetClipboardData<SubRecord[]>();
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

        void EnableUserInterface(bool enable)
        {
            this.splitHorizontal.Enabled = enable;
            this.menuStrip1.Enabled = enable;
            this.toolStripIncrFind.Enabled = enable;
            this.toolStripIncrInvalidRec.Enabled = enable;
        }
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
                if (dlg.ShowDialog() != DialogResult.OK)
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
                if (dlg.ShowDialog() != DialogResult.OK)
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
                    if (dlg.ShowDialog() == DialogResult.OK)
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
                settings.ShowDialog();
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
            contextMenuRecordCopyTo.DropDownItems.Clear();
            var srcPlugin = GetPluginFromNode(tn);
            foreach (TreeNode n in PluginTree.Nodes)
            {
                var plugin = n.Tag as BaseRecord;
                if (plugin == null) continue;
                if (srcPlugin.Equals(plugin)) continue;
                
                var tsi = new System.Windows.Forms.ToolStripButton(n.Text);
                tsi.Tag = new object[]{tn.Tag, tn, plugin, n};
                var sz = TextRenderer.MeasureText(n.Text, contextMenuRecordCopyTo.Font);
                if (sz.Width > tsi.Width)
                    tsi.Width = sz.Width;
                tsi.AutoSize = true;
                contextMenuRecordCopyTo.DropDownItems.Add(tsi);
            }
            
        }

        private void contextMenuRecordCopyTo_DropDownOpening(object sender, EventArgs e)
        {
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
            contextMenuRecord.Show(PluginTree.PointToScreen(new System.Drawing.Point(PluginTree.Width/4, PluginTree.Height/4)));
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
            if (rec == null) { 
                UpdateMainText("");
            } else {
                //var sb = new System.Text.StringBuilder();
                //rec.GetFormattedData(sb, GetSelectedContext());
                //tbInfo.Text = sb.ToString();

                var rb = new RTF.RTFBuilder(16);
                rec.GetFormattedData(rb, GetSelectedContext());
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
                "^(?<text>[^#]*)#(?<type>[0-z][A-Z][A-Z][A-Z]):(?<id>[0-9a-zA-Z]+)$"
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
            historyHandler = new OC.Windows.Forms.History<TreeNode>(toolStripRecordBack, toolStripRecordNext, 100);
            historyHandler.AllowDuplicates = true;
            historyHandler.GotoItem +=new EventHandler<OC.Windows.Forms.HistoryEventArgs<TreeNode>>(historyHandler_GotoItem);
            
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
            if (e.Alt && !e.Control && !e.Shift)
            {
                if (e.KeyCode == Keys.Left)
                {
                    toolStripRecordBack.PerformButtonClick();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    toolStripRecordNext.PerformButtonClick();
                    e.Handled = true;
                }
            }
        }

        #endregion
        
    }
}