using System.Configuration;
using System.Text;
using System.Linq;
using System.Web;
using System.Windows.Media.Animation;
using IronPython.Runtime;
using Microsoft.Win32;
using PythonConsoleControl;
using TESVSnip.Domain.Data.RecordStructure.Xml;
using TESVSnip.Domain.Scripts;
using TESVSnip.UI.Rendering.Extensions;
using TESVSnip.UI.Services;
using PyInterpreter = TESVSnip.Domain.Scripts.PyInterpreter;

namespace TESVSnip.UI.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using TESVSnip.Domain.Data.RecordStructure;
    using TESVSnip.Domain.Model;
    using TESVSnip.Domain.Services;
    using TESVSnip.Framework;
    using TESVSnip.Framework.Services;
    using TESVSnip.Properties;
    using TESVSnip.UI.Docking;
    using TESVSnip.UI.ObjectControls;

    using WeifenLuo.WinFormsUI.Docking;

    using Settings = TESVSnip.Properties.Settings;
    using Timer = System.Threading.Timer;

    internal partial class MainView : Form
    {
        private const int WM_SETREDRAW = 0x0b;

        private static readonly Regex linkRegex =
            new Regex(
                "^(?:(?<text>[^#]*)#)?(?:(?<plugin>[^\\/:*?\"<>|@]*)@)?(?<type>[0-z][A-Z][A-Z][A-Z_]):(?<id>[0-9a-zA-Z]+)$",
                RegexOptions.None);

        private static object s_clipboard;

        internal Dictionary<string, ToolStripMenuItem> languageToolBarItems =
            new Dictionary<string, ToolStripMenuItem>(StringComparer.InvariantCultureIgnoreCase);

        private readonly SelectionContext Selection;

        private readonly PluginTreeContent pluginTreeContent = new PluginTreeContent();
        private readonly HtmlContent htmlContent = new HtmlContent();

        private OutputTextContent outputTextContent = null;

        private readonly SubrecordListContent subrecordListContent = new SubrecordListContent();

        private volatile bool backgroundWorkCanceled;

        private bool inRebuildSelection;

        private DeserializeDockContent mDeserializeDockContent;

        private Timer statusTimer;

        private StringsEditor stringEditor;

        private JWC.MruStripMenu mruMenu;
        private static string mruRegKey = "SOFTWARE\\TESVSnip (Skyrim Edition)\\MRU";

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

            this.InitializeComponent();
            this.InitializeToolStripFind();
            this.InitializeDockingWindows();
            this.RegisterMessageFilter();

            this.PluginTree.SelectionChanged += (o, e) => this.RebuildSelection();

            if (string.IsNullOrEmpty(Settings.Default.DefaultSaveFolder) ||
                !Directory.Exists(Settings.Default.DefaultSaveFolder))
            {
                this.SaveModDialog.InitialDirectory = Options.Value.GameDataDirectory;
            }
            else
            {
                this.SaveModDialog.InitialDirectory = Settings.Default.DefaultSaveFolder;
            }

            if (string.IsNullOrEmpty(Settings.Default.DefaultOpenFolder) ||
                !Directory.Exists(Settings.Default.DefaultOpenFolder))
            {
                this.OpenModDialog.InitialDirectory = Options.Value.GameDataDirectory;
            }
            else
            {
                this.OpenModDialog.InitialDirectory = Settings.Default.DefaultOpenFolder;
            }

            Icon = Resources.tesv_ico;
            try
            {
                if (!Settings.Default.IsFirstTimeOpening)
                {
                    Services.Settings.GetWindowPosition("TESsnip", this);
                }
                else
                {
                    Services.Settings.SetWindowPosition("TESsnip", this);
                    Settings.Default.IsFirstTimeOpening = false;
                    Settings.Default.Save();
                }
            }
            catch
            {
            }

            this.useWindowsClipboardToolStripMenuItem.Checked = Settings.Default.UseWindowsClipboard;
            this.noWindowsSoundsToolStripMenuItem.Checked = Settings.Default.NoWindowsSounds;
            this.saveStringsFilesToolStripMenuItem.Checked = Domain.Properties.Settings.Default.SaveStringsFiles;

            this.useNewSubrecordEditorToolStripMenuItem.Checked = !Settings.Default.UseOldSubRecordEditor;
            this.hexModeToolStripMenuItem.Checked = Settings.Default.UseHexSubRecordEditor;
            this.uTF8ModeToolStripMenuItem.Checked = Framework.Properties.Settings.Default.UseUTF8;

            this.Selection = new SelectionContext();
            this.Selection.formIDLookup = this.LookupFormIDI;
            this.Selection.strLookup = this.LookupFormStrings;
            this.Selection.formIDLookupR = this.GetRecordByID;

            this.SubrecordList.SetContext(this.Selection);
            this.InitializeLanguage();

            ClipboardChanged += (o, e) => this.RebuildSelection();
            this.Selection.RecordChanged += (o, a) => this.RebuildSelection();
            this.Selection.SubRecordChanged += (o, a) => this.RebuildSelection();

            this.PluginTree.OnSelectionUpdated += this.PluginTree_OnSelectionUpdated;

            this.PluginTree.SelectionChanged += this.PluginTree_SelectionChanged;
            this.SubrecordList.SelectionChanged += this.subrecordPanel_SelectionChanged;
            this.SubrecordList.OnSubrecordChanged += this.subrecordPanel_OnSubrecordChanged;
            this.SubrecordList.DataChanged += this.subrecordPanel_DataChanged;
            PluginList.ChildListChanged += PluginList_ChildListChanged;

            this.htmlContent.OnLinkClicked += htmlContent_OnLinkClicked;
            this.LocalizeApp();
            PyInterpreter.InitPyInterpreter();
            TESVSnip.UI.Rendering.HtmlRenderer.Initialize();
            mruMenu = new JWC.MruStripMenu(recentFilelToolStripMenuItem, new JWC.MruStripMenu.ClickedHandler(OnMruFile),
                                       mruRegKey + "\\MRU", true, 16);
        }

        void PluginList_ChildListChanged(object sender, RecordChangeEventArgs e)
        {
            this.RebuildSelection();
            this.UpdateStringEditor();
            this.FixMasters();
            this.PluginTree.UpdateRoots();
        }

        public static event EventHandler ClipboardChanged;

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

        private PluginTreeView PluginTree
        {
            get { return this.pluginTreeContent.PluginTree; }
        }

        private SubrecordListEditor SubrecordList
        {
            get { return this.subrecordListContent.SubrecordList; }
        }

        public static void PostStatusText(string text)
        {
            PostStatusText(text, SystemColors.ControlText);
        }

        public static void PostStatusText(string text, Color color)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault();
            if (form != null)
            {
                form.SendStatusText(text, color);
            }
        }

        public static void PostStatusWarning(string text)
        {
            PostStatusText(text, Color.OrangeRed);
        }

        public void CancelBackgroundProcess()
        {
            this.backgroundWorkCanceled = true;
            this.backgroundWorker1.CancelAsync();
        }

        public RecordSearchForm CreateSearchWindow()
        {
            int id = Application.OpenForms.OfType<RecordSearchForm>().Count() + 1;
            var form = new RecordSearchForm();
            form.Text = string.Format("Search {0}", id);

            var searchform = Application.OpenForms.OfType<RecordSearchForm>().LastOrDefault(x => x.Visible);
            if (searchform != null)
            {
                if (searchform.Pane != null)
                {
                    // second item in list
                    form.Show(searchform.Pane, null);
                }
                else if (searchform.PanelPane != null)
                {
                    form.Show(searchform.PanelPane, null);
                }
            }
            else
            {
                if (this.dockPanel.ActiveDocumentPane != null)
                {
                    form.Show(this.dockPanel.ActiveDocumentPane, DockAlignment.Bottom, 0.33);
                }
                else
                {
                    form.Show(this.dockPanel, DockState.Document);
                }
            }

            return form;
        }

        public bool IsBackroundProcessCanceled()
        {
            return this.backgroundWorkCanceled;
        }

        /// <summary>
        /// Send text to status and then clear 5 seconds later.
        /// </summary>
        /// <param name="text">
        /// </param>
        public void SendStatusText(string text)
        {
            this.SendStatusText(text, SystemColors.ControlText);
        }

        public void SendStatusText(string text, Color color)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string, Color>(this.SendStatusText), new object[] {text, color});
                }
                else
                {
                    this.toolStripStatusLabel.ForeColor = color;
                    this.toolStripStatusLabel.Text = text;
                    if (this.statusTimer == null)
                    {
                        this.statusTimer = new Timer(
                            o =>
                            Invoke(new TimerCallback(o2 => { this.toolStripStatusLabel.Text = string.Empty; }),
                                   new object[] {string.Empty}), string.Empty, TimeSpan.FromSeconds(15),
                            TimeSpan.FromMilliseconds(-1));
                    }
                    else
                    {
                        this.statusTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void StartBackgroundWork(Action workAction, Action completedAction)
        {
            if (this.backgroundWorker1.IsBusy)
            {
                return;
            }

            this.EnableUserInterface(false);
            this.backgroundWorkCanceled = false;
            this.toolStripStatusProgressBar.ProgressBar.Value = this.toolStripStatusProgressBar.Minimum;
            this.toolStripStatusProgressBar.Visible = true;
            this.toolStripStopProgress.Visible = true;
            this.backgroundWorker1.RunWorkerAsync(new[] {workAction, completedAction});
        }

        public void UpdateBackgroundProgress(int percentProgress)
        {
            this.backgroundWorker1.ReportProgress(percentProgress);
        }

        internal static object GetClipboardData()
        {
            if (Settings.Default.UseWindowsClipboard)
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
            if (Settings.Default.UseWindowsClipboard)
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

        internal static bool HasClipboardData()
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent("TESVSnip");
            }

            return Clipboard != null;
        }

        internal static bool HasClipboardData<T>()
        {
            if (Settings.Default.UseWindowsClipboard)
            {
                var od = System.Windows.Forms.Clipboard.GetDataObject();
                return od != null && od.GetDataPresent(typeof (T).FullName);
            }

            return Clipboard is T;
        }

        internal static void PostReferenceSearch(uint formid)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault();
            if (form != null)
            {
                form.ReferenceSearch(formid);
            }
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

        internal static void SynchronizeSelection(IEnumerable<BaseRecord> selection)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault();
            if (form != null)
            {
                form.PluginTree.SetSelectedRecords(selection);
            }
        }

        internal void LoadPlugin(string s)
        {
            try
            {
                var p = new Plugin(s, false, this.GetRecordFilter(s));
                PluginList.All.AddRecord(p);
                //this.UpdateStringEditor();
                //this.FixMasters();
                //this.PluginTree.UpdateRoots();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                this.ClearCachedInfo();
            }
        }

        internal bool PreFilterMessage(ref Message m)
        {
            // Intercept the left mouse button down message.
            if (m.Msg == MainViewMessageFilter.WM_KEYDOWN)
            {
                if (m.WParam == new IntPtr((int) Keys.F6))
                {
                    var current = this.dockPanel.ActiveContent;
                    if (current != null)
                    {
                        var next = current.DockHandler.NextActive;
                        if (next != null)
                        {
                            next.DockHandler.Activate();
                            return true;
                        }
                    }

                    bool forward = !MainViewMessageFilter.IsShiftDown();
                    var formList = Application.OpenForms.OfType<IDockContent>().ToList();
                    var first = formList.Where(
                        x =>
                            {
                                var f = x as Control;
                                return f != null && f.ContainsFocus;
                            }).FirstOrDefault();
                    if (first != null)
                    {
                        int idx = formList.IndexOf(first);

                        if (idx >= 0)
                        {
                            idx = ++idx%formList.Count;
                        }

                        var c = formList[idx];
                        if (c != null)
                        {
                            c.DockHandler.Activate();
                        }
                        else
                        {
                            first.DockHandler.GiveUpFocus();
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static void ControlEnable(Control control, bool enable)
        {
            var ipenable = new IntPtr(enable ? 1 : 0);
            SendMessage(control.Handle, WM_SETREDRAW, ipenable, IntPtr.Zero);
        }

        private static bool IsVisible(IDockContent content)
        {
            return content.DockHandler.DockState != DockState.Hidden &&
                   content.DockHandler.DockState != DockState.Unknown;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private void CloseAllContents()
        {
            // we don't want to create another instance of tool window, set DockPanel to null
            this.pluginTreeContent.DockPanel = null;
            this.subrecordListContent.DockPanel = null;
        }

        private void CloseStringEditor()
        {
            if (this.stringEditor != null)
            {
                StringsEditor editor = this.stringEditor;
                this.stringEditor = null;
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

        private void CopySelectedSubRecord()
        {
            var sr = this.GetSelectedSubrecords();
            if (sr == null)
            {
                return;
            }

            Clipboard = sr.Select(ss => (SubRecord) ss.Clone()).ToArray();
        }

        private void CopySelection()
        {
            // Route to focused control.
            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.CopySelectedRecord();
            }
            else if (this.SubrecordList.ContainsFocus)
            {
                if (this.Selection.SelectedSubrecord)
                {
                    this.CopySelectedSubRecord();
                }
            }
        }

        private void EnableUserInterface(bool enable)
        {
            // ControlEnable(this.splitHorizontal, enable);
            // ControlEnable(this.splitVertical, enable);
            ControlEnable(this.menuStrip1, enable);
            ControlEnable(this.toolStripIncrFind, enable);
            ControlEnable(this.toolStripIncrInvalidRec, enable);
        }

        private void FixMasters()
        {
            PluginList.FixMasters();
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof (PluginTreeContent).ToString())
            {
                return this.pluginTreeContent;
            }

            if (persistString == typeof (SubrecordListContent).ToString())
            {
                return this.subrecordListContent;
            }


            if (persistString == typeof (HtmlContent).ToString())
            {
                return this.htmlContent;
            }

            return null;
        }

        private Plugin GetPluginFromNode(BaseRecord node)
        {
            BaseRecord tn = node;
            if (tn is Plugin)
            {
                return (Plugin) tn;
            }

            while (!(tn is Plugin) && tn != null)
            {
                tn = tn.Parent;
            }

            if (tn != null)
            {
                return tn as Plugin;
            }

            return new Plugin();
        }

        private Record GetRecordByID(uint id)
        {
            if (this.Selection != null && this.Selection.Record != null)
            {
                var p = this.GetPluginFromNode(this.Selection.Record);
                if (p != null)
                {
                    return p.GetRecordByID(id);
                }
            }

            return null;
        }

        private string[] GetRecordFilter(string s)
        {
            string[] recFilter = null;
            bool bAskToApplyFilter = true;
            if (Settings.Default.IsFirstTimeOpeningSkyrimESM)
            {
                if (string.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    DialogResult result = MessageBox.Show(
                        this, Resources.MainView_FirstTimeSkyrimLoad_ExcludeInquiry, Resources.FirstLoadOptions,
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        Settings.Default.EnableESMFilter = true;
                        Settings.Default.DontAskUserAboutFiltering = true;
                        using (var settings = new LoadSettings())
                        {
                            result = settings.ShowDialog(this);
                            if (result == DialogResult.Cancel)
                            {
                                // cancel will be same as No
                                Settings.Default.EnableESMFilter = false;
                                Settings.Default.DontAskUserAboutFiltering = true;
                            }
                        }

                        Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                    }
                    else if (result == DialogResult.No)
                    {
                        Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        Settings.Default.DontAskUserAboutFiltering = true;
                    }
                    else
                    {
                        Settings.Default.IsFirstTimeOpeningSkyrimESM = false;
                        return null;
                    }
                }

                bAskToApplyFilter = false;
            }

            if (Settings.Default.EnableESMFilter)
            {
                bool applyfilter;
                if (Settings.Default.ApplyFilterToAllESM)
                {
                    applyfilter = string.Compare(Path.GetExtension(s), ".esm", StringComparison.OrdinalIgnoreCase) == 0;
                }
                else
                {
                    applyfilter =
                        string.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0;
                }

                if (applyfilter && bAskToApplyFilter && !Settings.Default.DontAskUserAboutFiltering)
                {
                    DialogResult result = MessageBox.Show(
                        this, Resources.ESM_Large_File_Size_Inquiry, Resources.Filter_Options_Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    applyfilter = result == DialogResult.Yes;
                }

                if (applyfilter)
                {
                    recFilter = Settings.Default.FilteredESMRecords.Trim().Split(new[] {';', ','},
                                                                                 StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return recFilter;
        }

        private SelectionContext GetSelectedContext()
        {
            return this.Selection;

            // context.Record = this.parentRecord
            // context.SubRecord = GetSelectedSubrecord();
        }

        private SubRecord GetSelectedSubrecord()
        {
            return this.SubrecordList.GetSelectedSubrecord();
        }

        private IEnumerable<SubRecord> GetSelectedSubrecords()
        {
            return this.SubrecordList.GetSelectedSubrecords();
        }

        private void InitializeDockingWindows()
        {
            this.mDeserializeDockContent = this.GetContentFromPersistString;
        }

        private void InitializeLanguage()
        {
            this.languageToolBarItems.Add("English", this.englishToolStripMenuItem);
            this.languageToolBarItems.Add("Czech", this.czechToolStripMenuItem);
            this.languageToolBarItems.Add("French", this.frenchToolStripMenuItem);
            this.languageToolBarItems.Add("German", this.germanToolStripMenuItem);
            this.languageToolBarItems.Add("Italian", this.italianToolStripMenuItem);
            this.languageToolBarItems.Add("Spanish", this.spanishToolStripMenuItem);
            this.languageToolBarItems.Add("Russian", this.russianToolStripMenuItem);
            this.languageToolBarItems.Add("Polish", this.polishToolStripMenuItem);
        }

        private void LayoutDockingWindows(bool force)
        {
            try
            {
                if (!force && IsVisible(this.pluginTreeContent) && IsVisible(this.subrecordListContent) &&
                    IsVisible(htmlContent))
                {
                    return;
                }

                this.dockPanel.SuspendLayout(true);
                if (force)
                {
                    this.pluginTreeContent.DockPanel = null;
                    this.subrecordListContent.DockPanel = null;
                    this.htmlContent.DockPanel = null;
                }

                if (!IsVisible(this.pluginTreeContent) || force)
                {
                    this.pluginTreeContent.Show(this.dockPanel, DockState.DockLeft);
                    this.dockPanel.Width = Math.Max(this.dockPanel.Width, this.pluginTreeContent.MinimumSize.Width);
                }

                if (!IsVisible(this.subrecordListContent) || force)
                {
                    this.subrecordListContent.Show(this.pluginTreeContent.Pane, DockAlignment.Bottom, 0.5);
                }

                if (!IsVisible(this.htmlContent) || force)
                {
                    this.htmlContent.Show(this.dockPanel, DockState.Document);
                }
            }
            catch
            {
            }
            finally
            {
                this.dockPanel.ResumeLayout(true, true);
            }
        }

        private void LoadDockingWindows()
        {
            string configFile = Path.Combine(Options.Value.SettingsDirectory, @"DockPanel.config");
            if (File.Exists(configFile))
            {
                try
                {
                    this.dockPanel.SuspendLayout(true);
                    this.dockPanel.LoadFromXml(configFile, this.mDeserializeDockContent);
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
                    this.dockPanel.ResumeLayout(true, true);
                }
            }

            this.LayoutDockingWindows(force: false);
        }

        private string LookupFormIDI(uint id)
        {
            return this.LookupFormIDI(this.Selection, id);
        }

        private string LookupFormIDI(SelectionContext context, uint id)
        {
            if (context != null && context.Record != null)
            {
                var p = this.GetPluginFromNode(context.Record);
                if (p != null)
                {
                    p.LookupFormID(id);
                }
            }

            return "No selection";
        }

        private string LookupFormStrings(uint id)
        {
            if (this.Selection != null && this.Selection.Record != null)
            {
                var p = this.GetPluginFromNode(this.Selection.Record);
                if (p != null)
                {
                    return p.LookupFormStrings(id);
                }
            }

            return null;
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            this.LoadDockingWindows();
            this.FixMasters();
            this.toolStripIncrFind.Visible = false;
            this.toolStripIncrFind.Enabled = false;
            this.toolStripIncrInvalidRec.Visible = false;
            this.toolStripIncrInvalidRec.Enabled = false;
            BuildDynamicScriptsMenu();
        }

        private void MainView_Shown(object sender, EventArgs e)
        {
            //// Only prevent content hiding after window if first shown
            // dockingManagerExtender.DockingManager.ContentHiding +=
            // delegate(Content c, CancelEventArgs cea) { cea.Cancel = true; };
            // dockingManagerExtender.DockingManager.ShowAllContents();
            this.ShowDockingWindows();

            if (!DesignMode)
            {
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    var attr =
                        asm.GetCustomAttributes(true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                    if (attr != null)
                    {
                        Text = attr.InformationalVersion;
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// This routine assigns Structure definitions to subrecords.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private bool MatchRecordStructureToRecord()
        {
            var rec = this.Selection.Record as Record;
            if (rec == null)
            {
                return false;
            }

            return rec.MatchRecordStructureToRecord();
        }

        private void PasteFromClipboard(bool recordOnly, bool asNew)
        {
            if (!HasClipboardData())
            {
                MessageBox.Show(Resources.TheClipboardIsEmpty, Resources.ErrorText);
                return;
            }

            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.PasteFromClipboard(recordOnly, asNew);
            }
            else if (this.SubrecordList.ContainsFocus)
            {
                this.SubrecordList.PasteFromClipboard();
            }
        }

        private void PluginTree_OnSelectionUpdated(object sender, EventArgs e)
        {
            // fix EDID if relevant
            this.UpdateMainText(this.PluginTree.SelectedRecord);
            this.PluginTree.RefreshObject(this.PluginTree.SelectedRecord);
        }

        private void PluginTree_SelectionChanged(object sender, EventArgs e)
        {
            this.UpdateMainText(this.PluginTree.SelectedRecord);
        }

        private void RebuildSelection()
        {
            if (this.inRebuildSelection)
            {
                return;
            }

            bool oldInRebuildSelection = this.inRebuildSelection;
            try
            {
                this.inRebuildSelection = true;
                var rec = this.PluginTree.SelectedRecord;
                if (rec == null)
                {
                    this.SubrecordList.Record = null;
                    this.Selection.Record = null;
                    this.UpdateMainText(string.Empty);
                    return;
                }

                bool hasClipboard = HasClipboardData();

                if (rec is Plugin)
                {
                    this.SubrecordList.Record = null;
                    this.Selection.Record = null;
                    this.cutToolStripMenuItem.Enabled = false;
                    this.copyToolStripMenuItem.Enabled = false;
                    this.deleteToolStripMenuItem.Enabled = false;
                    this.pasteToolStripMenuItem.Enabled = hasClipboard;
                    this.pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    this.insertGroupToolStripMenuItem.Enabled = true;
                    this.insertRecordToolStripMenuItem.Enabled = true;
                    this.insertSubrecordToolStripMenuItem.Enabled = false;
                }
                else if (rec is Record)
                {
                    this.cutToolStripMenuItem.Enabled = true;
                    this.copyToolStripMenuItem.Enabled = true;
                    this.deleteToolStripMenuItem.Enabled = true;
                    this.pasteToolStripMenuItem.Enabled = hasClipboard;
                    this.pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    this.insertGroupToolStripMenuItem.Enabled = false;
                    this.insertRecordToolStripMenuItem.Enabled = true;
                    this.insertSubrecordToolStripMenuItem.Enabled = true;
                    this.Selection.Record = rec as Rec;
                    this.SubrecordList.Record = this.Selection.Record as Record;
                    this.MatchRecordStructureToRecord();
                }
                else if (rec is GroupRecord)
                {
                    this.Selection.Record = null;
                    this.SubrecordList.Record = null;
                    this.cutToolStripMenuItem.Enabled = true;
                    this.copyToolStripMenuItem.Enabled = true;
                    this.deleteToolStripMenuItem.Enabled = true;
                    this.pasteToolStripMenuItem.Enabled = hasClipboard;
                    this.pasteNewToolStripMenuItem.Enabled = hasClipboard;
                    this.insertGroupToolStripMenuItem.Enabled = true;
                    this.insertRecordToolStripMenuItem.Enabled = true;
                    this.insertSubrecordToolStripMenuItem.Enabled = false;
                }
                else
                {
                    this.Selection.Record = null;
                    this.SubrecordList.Record = null;
                    this.cutToolStripMenuItem.Enabled = false;
                    this.copyToolStripMenuItem.Enabled = false;
                    this.deleteToolStripMenuItem.Enabled = false;
                    this.pasteToolStripMenuItem.Enabled = false;
                    this.pasteNewToolStripMenuItem.Enabled = false;
                    this.insertGroupToolStripMenuItem.Enabled = false;
                    this.insertRecordToolStripMenuItem.Enabled = false;
                    this.insertSubrecordToolStripMenuItem.Enabled = false;
                }

                this.Selection.SubRecord = this.GetSelectedSubrecord();
            }
            finally
            {
                this.inRebuildSelection = oldInRebuildSelection;
            }
        }

        private void ReferenceSearch(uint formid)
        {
            var search = this.CreateSearchWindow();
            search.ReferenceSearch(formid);
        }

        private void RegisterMessageFilter()
        {
            // Register message filter.
            try
            {
                var msgFilter = new MainViewMessageFilter(this);
                Application.AddMessageFilter(msgFilter);
            }
            catch
            {
            }
        }

        private void ReloadLanguageFiles()
        {
            foreach (Plugin p in PluginList.All.Records)
            {
                p.ReloadStrings();
            }
        }

        private void SaveDockingWindows()
        {
            string configFile = null;
            try
            {

                configFile = Path.Combine(Options.Value.SettingsDirectory, "DockPanel.config");
                this.dockPanel.SaveAsXml(configFile);
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
            this.pluginTreeContent.CloseButtonVisible = false;
            this.subrecordListContent.CloseButtonVisible = false;
            this.htmlContent.MdiParent = this;
            this.htmlContent.CloseButtonVisible = false;
            this.htmlContent.CloseButton = false;
            this.htmlContent.HideOnClose = true;
            this.LayoutDockingWindows(force: false);
        }

        private void TESsnip_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.UseWindowsClipboard = this.useWindowsClipboardToolStripMenuItem.Checked;
            PluginList.All.Clear();
            this.PluginTree.UpdateRoots();
            Clipboard = null;
            this.Selection.Record = null;
            this.RebuildSelection();
            this.CloseStringEditor();
            this.SaveDockingWindows();
            Services.Settings.SetWindowPosition("TESsnip", this);
        }

        private void UpdateMainText(BaseRecord rec)
        {
            if (rec == null)
            {
                this.UpdateMainText(string.Empty);
            }
            else
            {
                try
                {
                    string html = TESVSnip.UI.Rendering.HtmlRenderer.GetDescription(rec);
                    this.htmlContent.UpdateText(html);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.WarningText);
                }
            }
        }

        private void UpdateMainText(string text)
        {
            this.htmlContent.UpdateText(text);
        }

        private void UpdateStringEditor()
        {
            if (this.stringEditor != null)
            {
                var plugins = PluginList.All.Records.OfType<Plugin>().ToList();
                if (plugins.Count == 0)
                {
                    this.CloseStringEditor();
                }
                else
                {
                    this.stringEditor.Reload(plugins.ToArray());
                }
            }
        }

        private void addMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var amfNewMaster = new AddMasterForm())
            {
                if (amfNewMaster.ShowDialog(this) == DialogResult.OK)
                {
                    Plugin plugin = this.GetPluginFromNode(this.PluginTree.SelectedRecord);
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
                            this.FixMasters();
                            this.RebuildSelection();
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        MessageBox.Show(this, ex.Message, "Missing Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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
            this.toolStripStatusProgressBar.Value = e.ProgressPercentage%this.toolStripStatusProgressBar.Maximum;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.EnableUserInterface(true);
            this.toolStripStatusProgressBar.Visible = false;
            this.toolStripStopProgress.Visible = false;
            if (e.Cancelled || e.Error != null)
            {
                return;
            }

            var completedAction = e.Result as Action;
            if (completedAction != null)
            {
                completedAction();
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Resources.CloseAllLoseChangesInquiry, Resources.WarningText, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }
            CloseAllPlugins();
        }

        private void CloseAllPlugins()
        {
            PluginList.All.Records.Clear();
            this.PluginTree.UpdateRoots();
            this.Selection.Reset();
            this.SubrecordList.Record = null;
            Clipboard = null;
            this.CloseStringEditor();
            this.UpdateMainText(string.Empty);
            ClearCachedInfo();
        }

        private void ClearCachedInfo()
        {
            // Following is mostly about cleaning up leaked / referenced memory
            this.SubrecordList.SetContext(this.Selection);
            this.RebuildSelection();
            this.PluginTree.UpdateRoots();
            this.Selection.Reset();
            this.SubrecordList.SetContext(this.Selection);
            GC.Collect();            
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }

            if (MessageBox.Show(Resources.CloseActivePluginInquiry, Resources.WarningText, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }

            var p = this.GetPluginFromNode(this.PluginTree.SelectedRecord);
            PluginList.All.DeleteRecord(p);
            this.UpdateStringEditor();
            this.UpdateMainText(string.Empty);
            this.FixMasters();
            this.PluginTree.UpdateRoots();
            this.RebuildSelection();
            ClearCachedInfo();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.CollapseAll();
        }

        private void collapseBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.CollapseAll(this.PluginTree.SelectedRecord);
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

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CopySelection();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.Selection.SelectedSubrecord && this.PluginTree.SelectedRecord != null &&
                this.PluginTree.SelectedRecord is Plugin)
            {
                MessageBox.Show(Resources.Cannot_cut_a_plugin, Resources.ErrorText);
                return;
            }

            this.copyToolStripMenuItem_Click(null, null);
            this.deleteToolStripMenuItem_Click(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.DeleteSelection();
            }
            else if (this.SubrecordList.ContainsFocus)
            {
                this.SubrecordList.DeleteSelection();
            }
        }

        private void eSMFilterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Update the global list
            bool modified = false;
            var groups = Settings.Default.AllESMRecords != null
                             ? Settings.Default.AllESMRecords.Trim().Split(new[] {';', ','},
                                                                           StringSplitOptions.RemoveEmptyEntries).ToList
                                   ()
                             : new List<string>();
            groups.Sort();
            foreach (var plugin in PluginList.All.Records.OfType<Plugin>())
            {
                plugin.ForEach(
                    r =>
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
                Settings.Default.AllESMRecords = string.Join(";", groups.ToArray());
            }

            using (var settings = new LoadSettings())
            {
                settings.ShowDialog(this);
            }
        }

        private void editHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.EditSelectedHeader();
            }
            else if (this.SubrecordList.ContainsFocus)
            {
                this.SubrecordList.EditSelectedSubrecordHex();
            }
        }

        private void editSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.ContainsFocus)
            {
                this.PluginTree.EditSelectedRecord();
            }
            else if (this.SubrecordList.ContainsFocus)
            {
                this.SubrecordList.EditSelectedSubrecord();
            }
        }

        private void editStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.stringEditor == null)
            {
                var plugins = PluginList.All.Records.OfType<Plugin>().ToList();
                if (plugins.Count == 0)
                {
                    MessageBox.Show(this, "No plugins available to edit", Resources.ErrorText, MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                this.stringEditor = new StringsEditor();
                this.stringEditor.FormClosed += delegate { this.CloseStringEditor(); };
                this.stringEditor.Plugins = plugins.ToArray();
                this.stringEditor.Show(this); // modeless. Close if the tree is modified.
            }
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.pasteToolStripMenuItem.Enabled = HasClipboardData();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.ExpandAll();
        }

        private void expandBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PluginTree.ExpandAll(this.PluginTree.SelectedRecord);
        }

        private void findInRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CreateSearchWindow();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.toolStripIncrFind.Visible)
            {
                this.toolStripIncrFind.Enabled = true;
                this.toolStripIncrFind.Visible = true;
                this.toolStripIncrFind.Focus();
                this.toolStripIncrFindText.Select();
                this.toolStripIncrFindText.SelectAll();
                this.toolStripIncrFindText.Focus();
            }
            else
            {
                this.toolStripIncrFind.Visible = false;
                this.toolStripIncrFind.Enabled = false;
            }
        }

        private void hexModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseHexSubRecordEditor = this.hexModeToolStripMenuItem.Checked;
        }

        private void insertGroupToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var node = this.PluginTree.SelectedRecord;
            var p = new GroupRecord("NEW_");
            node.AddRecord(p);
            this.GetPluginFromNode(this.PluginTree.SelectedRecord).InvalidateCache();
            this.PluginTree.RefreshObject(node);
        }

        private void insertRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = this.PluginTree.SelectedRecord;

            if (node is Record && (node.Parent is GroupRecord || node.Parent is Plugin))
            {
                node = node.Parent;
            }

            var record = new Record();
            if (node is GroupRecord)
            {
                var g = (GroupRecord) node;
                if (g.groupType == 0)
                {
                    record.Name = g.ContentsType;
                }
            }

            node.AddRecord(record);
            Spells.giveRecordNewFormID(record, false);
            this.GetPluginFromNode(this.PluginTree.SelectedRecord).InvalidateCache();
            this.PluginTree.RefreshObject(node);
        }

        private void insertSubrecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseRecord node = this.PluginTree.SelectedRecord;
            var p = new SubRecord();
            node.AddRecord(p);
            this.GetPluginFromNode(this.PluginTree.SelectedRecord).InvalidateCache();
            this.PluginTree.RefreshObject(node);
            this.RebuildSelection();
        }

        private void languageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (var kvp in this.languageToolBarItems)
            {
                if (e.ClickedItem == kvp.Value)
                {
                    if (Domain.Properties.Settings.Default.LocalizationName != kvp.Key)
                    {
                        Domain.Properties.Settings.Default.LocalizationName = kvp.Key;
                        this.ReloadLanguageFiles();
                    }

                    break;
                }
            }
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var kvp in this.languageToolBarItems)
            {
                kvp.Value.Checked =
                    string.Compare(kvp.Key, Domain.Properties.Settings.Default.LocalizationName, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        private void lookupFormidsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lookupFormidsToolStripMenuItem.Checked)
            {
                this.Selection.formIDLookup = this.LookupFormIDI;
                this.Selection.strLookup = this.LookupFormStrings;
                this.Selection.formIDLookupR = this.GetRecordByID;
            }
            else
            {
                this.Selection.formIDLookup = null;
                this.Selection.strLookup = null;
                this.Selection.formIDLookupR = null;
            }
        }

        private void mergeRecordsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Records baseRecords;
            Records updateRecords;

            var xs = new XmlSerializer(typeof (Records));
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Base Record Structure";
                dlg.InitialDirectory = Options.Value.SettingsDirectory;
                dlg.FileName = "RecordStructure.xml";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

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
                {
                    return;
                }

                using (FileStream fs = File.OpenRead(dlg.FileName))
                {
                    updateRecords = xs.Deserialize(fs) as Records;
                }
            }

            if (updateRecords != null && baseRecords != null)
            {
                var builder = new RecordBuilder();
                builder.MergeRecords(baseRecords.Items.OfType<RecordsRecord>(),
                                     updateRecords.Items.OfType<RecordsRecord>());

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

        private void newFormIDNoReferenceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.giveSelectionNewFormID(false);
        }

        private void newFormIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.giveSelectionNewFormID(true);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPlugin();
        }

        public Plugin NewPlugin()
        {
            var p = new Plugin();
            var r = new Record {Name = "TES4"};
            var sr = new SubRecord {Name = "HEDR"};
            sr.SetData(new byte[] {0xD7, 0xA3, 0x70, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x01});
            r.AddRecord(sr);
            sr = new SubRecord {Name = "CNAM"};
            sr.SetData(Encoding.Instance.GetBytes("Default\0"));
            r.AddRecord(sr);
            p.AddRecord(r);
            PluginList.All.AddRecord(p);
            return p;
        }

        public void RefreshPlugins()
        {
            this.RebuildSelection();
            this.UpdateStringEditor();
            this.FixMasters();
            this.PluginTree.UpdateRoots();
        }

        private void noWindowsSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.NoWindowsSounds =
                this.noWindowsSoundsToolStripMenuItem.Checked = !this.noWindowsSoundsToolStripMenuItem.Checked;
        }

        private void openNewPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.OpenModDialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadPluginFromListOfFileNames(this.OpenModDialog.FileNames);
            }
        }

        private void LoadPluginFromListOfFileNames(string[] fileNames)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                this.PluginTree.EnableEvents(false);

                try
                {
                    foreach (string s in fileNames)
                    {
                        this.LoadPlugin(s);
                        mruMenu.AddFileAndSaveToRegistry(s);
                    }
                }
                finally
                {
                    this.FixMasters();
                    this.PluginTree.UpdateRoots();
                    this.PluginTree.EnableEvents(true);
                    this.ClearCachedInfo();
                    sw.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                    toolStripStatusLabel.Text =
                        string.Format(TranslateUI.TranslateUiGlobalization.ResManager.GetString("MSG_LoadPluginIn"), t.ToString());                    
                }
            }
            catch (Exception ex)
            {
                string errMsg =
                    "Message: " + ex.Message +
                    Environment.NewLine +
                    Environment.NewLine +
                    "StackTrace: " + ex.StackTrace +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Source: " + ex.Source +
                    Environment.NewLine +
                    Environment.NewLine +
                    "GetType: " + ex.GetType().ToString();

                System.Windows.Forms.Clipboard.SetDataObject(errMsg, true);

                // Create an EventLog instance and assign its source.
                EventLog myLog = new EventLog();
                myLog.Source = "ThreadException";
                myLog.WriteEntry(errMsg);

                MessageBox.Show(errMsg, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void pasteNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PasteFromClipboard(false, true);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.PasteFromClipboard(false, false);
        }

        private void reloadStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ReloadLanguageFiles();
        }

        private void reloadXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                RecordStructure.Load();
                foreach (var rec in PluginList.All.Enumerate(x => x is Record).OfType<Record>())
                {
                    rec.MatchRecordStructureToRecord();
                }

                this.RebuildSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.CannotParseRecordStructure + ex.Message, Resources.WarningText);
            }
        }

        private void resetDockingWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutDockingWindows(force: true);
        }

        void htmlContent_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                bool isControlPressed = ModifierKeys == Keys.Control;
                
                var uri = new Uri(e.LinkText);
                if (uri.Scheme == "nav")
                {                   
                    var result = HttpUtility.ParseQueryString(uri.Query);
                    var pluginName = uri.LocalPath;
                    var masterName = result.Get("m");
                    if (pluginName == ".")
                        pluginName = null;
                    var type = result["t"];
                    uint formID = 0;
                    uint.TryParse(result["v"], NumberStyles.HexNumber, null, out formID);

                    Plugin startPlugin = null;
                    Plugin masterPlugin = null;
                    if (!string.IsNullOrEmpty(pluginName))
                        startPlugin = PluginList.All[pluginName];
                    if (!string.IsNullOrEmpty(masterName))
                        masterPlugin = PluginList.All[masterName];
                    if (masterPlugin == null && !string.IsNullOrEmpty(masterName))
                    {
                        var ext = Path.GetExtension(masterName);
                        if (string.Compare(ext, ".esm", false) == 0 
                            || string.Compare(ext, ".esp", false) == 0)
                        {
                            var dr = MessageBox.Show(this
                                , string.Format(Resources.Plugin_not_loaded__LoadNow, masterName)
                                , Resources.Load_Reference
                                , MessageBoxButtons.YesNo, MessageBoxIcon.Question
                                ,MessageBoxDefaultButton.Button2);                            
                            if (dr == DialogResult.Yes)
                            {
                                using (var dlg = new OpenFileDialog())
                                {
                                    dlg.Title = "Select Plugin to load";
                                    dlg.InitialDirectory = Options.Value.GameDataDirectory;
                                    dlg.FileName = masterName;
                                    dlg.Filter = "Master|" + masterName;
                                    dlg.FilterIndex = 0;
                                    dlg.CheckFileExists = true;
                                    if (dlg.ShowDialog(this) == DialogResult.OK)
                                    {
                                        if (string.Compare(Path.GetFileName(dlg.FileName), masterName, true) == 0)
                                        {
                                            LoadPluginFromListOfFileNames(new[] { masterName });
                                            masterPlugin = PluginList.All[masterName];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // Use efficient search before exhaustive search
                    if (startPlugin != null && masterPlugin != null)
                    {
                        var node = startPlugin.GetRecordByID(formID);
                        if (node != null)
                        {
                            this.PluginTree.SelectedRecord = node;
                            return;
                        }
                    }

                    var startNode = startPlugin ?? this.PluginTree.SelectedRecord ?? this.PluginTree.TopRecord;

                    // System.Windows.Forms.Application.
                    // Search current plugin and then wrap around.  
                    // Should do it based on master plugin list first.

                    var searchContext = new SearchSettings();
                    searchContext.rectype = type == "XXXX" ? null : type;
                    searchContext.text = result["v"];
                    searchContext.type = SearchType.FormID;
                    searchContext.startNode = startNode;
                    searchContext.wrapAround = true;
                    searchContext.partial = false;
                    searchContext.forward = true;
                    searchContext.first = true;

                    if (isControlPressed)
                    {
                        // Cursor.Position
                        var contextMenu = new ContextMenu();
                        contextMenu.MenuItems.Add(
                            "&Find In Tree",
                            (o, args) =>
                            {
                                var node = this.PerformSearch(searchContext);
                                if (node != null)
                                {
                                    this.PluginTree.SelectedRecord = node;
                                }
                            });
                        contextMenu.MenuItems.Add("Find &References", (o, args) => this.ReferenceSearch(formID));
                        contextMenu.Show(this, PointToClient(MousePosition));
                    }
                    else
                    {
                        var node = this.PerformSearch(searchContext);
                        if (node != null)
                        {
                            this.PluginTree.SelectedRecord = node;
                        }
                    }
                }
            }
            catch
            {
            }
        }



        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }

            var p = this.GetPluginFromNode(this.PluginTree.SelectedRecord);
            if (p.Filtered)
            {
                DialogResult result = MessageBox.Show(
                    this, Resources.SavePluginWithFilterAppliedInquiry, Resources.WarningText, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            this.SaveModDialog.FileName = p.Name;
            if (this.SaveModDialog.ShowDialog(this) == DialogResult.OK)
            {
                Stopwatch sw = Stopwatch.StartNew();
                p.Save(this.SaveModDialog.FileName);
                mruMenu.AddFileAndSaveToRegistry(this.SaveModDialog.FileName);
                this.FixMasters();
                sw.Stop();
                TimeSpan t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                toolStripStatusLabel.Text =
                    string.Format(TranslateUI.TranslateUiGlobalization.ResManager.GetString("MSG_SavePluginIn"), t.ToString());
            }
        }

        private void saveStringsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Domain.Properties.Settings.Default.SaveStringsFiles =
                this.saveStringsFilesToolStripMenuItem.Checked = !this.saveStringsFilesToolStripMenuItem.Checked;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }

            var p = this.GetPluginFromNode(this.PluginTree.SelectedRecord);
            if (p.Filtered)
            {
                DialogResult result = MessageBox.Show(
                    this, Resources.SavePluginWithFilterAppliedInquiry, Resources.WarningText, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(p.PluginPath)) p.PluginPath = Options.Value.GameDataDirectory;
            string pluginFilPath = Path.Combine(p.PluginPath, p.Name);
            p.Save(pluginFilPath);
            mruMenu.AddFileAndSaveToRegistry(Path.Combine(p.PluginPath, p.Name));
            this.FixMasters();
        }

        private void searchAdvancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecordStructure recStruct = null;
            var rec = this.PluginTree.SelectedRecord;
            if (rec is Record)
            {
                RecordStructure.Records.TryGetValue(rec.Name, out recStruct);
            }

            if (recStruct == null)
            {
                recStruct = RecordStructure.Records.Values.Random(RecordStructure.Records.Count).First();
            }

            using (var dlg = new SearchFilterAdvanced(recStruct))
            {
                dlg.ShowDialog(this);
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = this.searchToolStripMenuItem.Tag as SearchCriteriaSettings;
            using (var dlg = new SearchFilterBasic())
            {
                RecordStructure recStruct = null;
                if (settings != null)
                {
                    if (RecordStructure.Records.TryGetValue(settings.Type, out recStruct))
                    {
                        dlg.SetRecordStructure(recStruct);
                        dlg.Criteria = settings;
                    }
                }

                if (recStruct == null)
                {
                    var rec = this.PluginTree.SelectedRecord;
                    if (rec is GroupRecord)
                    {
                        var gr = rec as GroupRecord;
                        var ct = gr.ContentsType;
                        if (!string.IsNullOrEmpty(ct))
                        {
                            RecordStructure.Records.TryGetValue(ct, out recStruct);
                        }
                    }
                    else if (rec is Record)
                    {
                        RecordStructure.Records.TryGetValue(rec.Name, out recStruct);
                    }

                    dlg.SetRecordStructure(recStruct);
                }

                dlg.EnableFindAll(false); // hide final all since we will open 
                if (DialogResult.Cancel != dlg.ShowDialog(this))
                {
                    this.searchToolStripMenuItem.Tag = dlg.Criteria;
                    var window = this.CreateSearchWindow();
                    window.SetSearchCriteria(dlg.Criteria, doSearch: true);
                }
            }
        }

        private void subrecordPanel_DataChanged(object sender, EventArgs e)
        {
            var sr = this.SubrecordList.GetSelectedSubrecord();
            if (sr != null)
            {
                this.UpdateMainText(sr);
            }
        }

        private void subrecordPanel_OnSubrecordChanged(object sender, RecordChangeEventArgs e)
        {
            if (e.Record is SubRecord)
            {
                if (e.Record.Parent is Record)
                {
                    this.PluginTree.RefreshObject(e.Record.Parent);
                }

                this.SubrecordList.RefreshObject(e.Record);
            }
        }

        private void subrecordPanel_SelectionChanged(object sender, EventArgs e)
        {
            this.UpdateMainText(this.SubrecordList.SubRecord);
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

        private void toolStripCheck_CheckStateChanged(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;
            button.Image = button.Checked ? Resources.checkedbox : Resources.emptybox;
        }

        private void toolStripIncrInvalidRecCancel_Click(object sender, EventArgs e)
        {
            this.toolStripIncrInvalidRec.Visible = false;
            this.toolStripIncrInvalidRec.Enabled = false;
        }

        private void toolStripIncrInvalidRecNext_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null)
            {
                return;
            }

            this.BackgroundNonConformingRecordIncrementalSearch(this.PluginTree.SelectedRecord, true,
                                                                this.toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecPrev_Click(object sender, EventArgs e)
        {
            if (this.PluginTree.SelectedRecord == null)
            {
                return;
            }

            this.BackgroundNonConformingRecordIncrementalSearch(this.PluginTree.SelectedRecord, false,
                                                                this.toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecRestart_Click(object sender, EventArgs e)
        {
            var rec = PluginList.All.Records.OfType<BaseRecord>().FirstOrDefault();
            if (rec == null)
            {
                return;
            }

            this.BackgroundNonConformingRecordIncrementalSearch(rec, true, false);
        }

        private void toolStripIncrInvalidRec_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void toolStripIncrInvalidRec_VisibleChanged(object sender, EventArgs e)
        {
            this.findNonconformingRecordToolStripMenuItem.Checked = this.toolStripIncrInvalidRec.Visible;
            this.toolStripIncrInvalidRecStatus.Text = "Select Next or Prev to start search.";
            this.toolStripIncrInvalidRecStatus.ForeColor = Color.DarkGray;
        }

        private void toolStripStopProgress_Click(object sender, EventArgs e)
        {
            this.CancelBackgroundProcess();
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
        }

        private void uTF8ModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Framework.Properties.Settings.Default.UseUTF8 = this.uTF8ModeToolStripMenuItem.Checked;
            if (MessageBox.Show(Resources.RestartText, Resources.InfoText, MessageBoxButtons.YesNoCancel) ==
                DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void useNewSubrecordEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseOldSubRecordEditor = !this.useNewSubrecordEditorToolStripMenuItem.Checked;
        }

        private void useWindowsClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseWindowsClipboard =
                this.useWindowsClipboardToolStripMenuItem.Checked = !this.useWindowsClipboardToolStripMenuItem.Checked;
        }

        private void openListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                           "Skyrim");
                string defaultFile = Path.Combine(path, "plugins.txt");
                dlg.Title = "Select PluginList List File";
                dlg.InitialDirectory = path;
                dlg.FileName = "plugins.txt";
                dlg.Filter = "Plugin Lists|*.txt|All Files|*.*";
                dlg.FilterIndex = 0;
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor origCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                Stopwatch sw = Stopwatch.StartNew();
                this.PluginTree.EnableEvents(false);
                try
                {
                    using (StreamReader fs = File.OpenText(dlg.FileName))
                    {
                        CloseAllPlugins();
                        bool checkSkyrim = string.Compare(dlg.FileName, defaultFile, StringComparison.InvariantCultureIgnoreCase) == 0;
                        while (true)
                        {
                            string line = fs.ReadLine();
                            if (line == null) break;
                            if (line == "") continue;
                            string file = Path.Combine(Options.Value.GameDataDirectory, line.Trim());
                            if (checkSkyrim)
                            {
                                checkSkyrim = false;
                                // skyrim is not first item in plugins.txt so load it first
                                if (string.Compare(Path.GetFileName(file), "skyrim.esm", StringComparison.InvariantCultureIgnoreCase) != 0)
                                {
                                    var skyrimFile = Path.Combine(Options.Value.GameDataDirectory, "skyrim.esm");
                                    if (File.Exists(skyrimFile))
                                        PluginList.All.AddRecord(new Plugin(skyrimFile, false, GetRecordFilter(skyrimFile)));
                                }
                            }
                            if (File.Exists(file))
                            {
                                var p = new Plugin(file, false, GetRecordFilter(file));
                                PluginList.All.AddRecord(p);
                            }
                        }
                    }
                }
                finally
                {
                    UpdateStringEditor();
                    FixMasters();
                    PluginTree.UpdateRoots();
                    this.PluginTree.EnableEvents(true);
                    this.ClearCachedInfo();

                    Cursor.Current = origCursor;

                    sw.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                    toolStripStatusLabel.Text =
                        string.Format(TranslateUI.TranslateUiGlobalization.ResManager.GetString("MSG_LoadPluginIn"), t.ToString());
                }
            }
        }

        private void saveListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //C:\Users\User\AppData\Local\Oblivion\plugins.txt
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Select PluginList List File";
                dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.FileName = "plugins.txt";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                using (FileStream fs = File.OpenWrite(dlg.FileName))
                using (var sw = new StreamWriter(fs))
                {
                    foreach (Plugin p in PluginList.All.Records)
                    {
                        sw.WriteLine(Path.GetFileName(p.Name));
                    }
                    sw.Flush();
                }
            }
        }

        #region MessageFilter
        public class MainViewMessageFilter : IMessageFilter
        {
            public const int WM_CHAR = 0x102;

            public const int WM_KEYDOWN = 0x100;

            public const int WM_KEYUP = 0x101;

            private const ushort KEY_PRESSED = 0x8000;

            private readonly MainView owner = null;

            public MainViewMessageFilter(MainView owner)
            {
                this.owner = owner;
            }

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

            [DllImport("user32.dll")]
            public static extern ushort GetAsyncKeyState(VirtualKeyStates nVirtKey);

            [DllImport("user32.dll")]
            public static extern ushort GetKeyState(VirtualKeyStates nVirtKey);

            public static bool IsAltDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_MENU);
            }

            public static bool IsControlDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_CONTROL);
            }

            public static bool IsShiftDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_SHIFT);
            }

            public bool PreFilterMessage(ref Message m)
            {
                try
                {
                    return this.owner.PreFilterMessage(ref m);
                }
                catch
                {
                }

                return true;
            }
        }
        #endregion

        /// <summary>
        /// Event for MRU List
        /// </summary>
        /// <param name="number"></param>
        /// <param name="filename"></param>
        private void OnMruFile(int number, String filename)
        {
            if (System.IO.File.Exists(filename))
            {
                string[] fileNames = new string[] {filename};
                mruMenu.SetFirstFile(number);
                this.Update();
                LoadPluginFromListOfFileNames(fileNames);
            }
            else
            {
                string msg = string.Format(TranslateUI.TranslateUiGlobalization.ResManager.GetString("UI_MRU_FileNotExist"),
                                           filename);
                MessageBox.Show(msg, "Tesvsnip", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mruMenu.RemoveFile(number);
            }
        }

        private void resetSettingsToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration conf = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string pathUserConfig = conf.FilePath;
            try
            {
                if (File.Exists(pathUserConfig))
                {
                    File.Delete(pathUserConfig);
                    Application.Restart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tesvsnip", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public TESVSnip.UI.Docking.InterpreterConsole CreateInterpreterWindow()
        {
            int id = Application.OpenForms.OfType<InterpreterConsole>().Count() + 1;
            var form = new InterpreterConsole {Text = string.Format("Console {0}", id)};
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
                form.Show(this.dockPanel, DockState.Document);
            }

            return form;
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var pyWindow = CreateInterpreterWindow();
                if (pyWindow != null)
                {
                    var pyConsole = pyWindow.InnerView.consoleControl;
                    Scripting.BootstrapConsole(pyConsole, (c) =>
                        {
                            var engine = c.ScriptScope.Engine;
                            var paths = engine.GetSearchPaths().ToList();
                            paths.Add(PluginEngine.PluginsPyPath);
                            paths.Add(Path.Combine(Options.Value.ScriptsDirectory, "lib"));
                            engine.SetSearchPaths(paths);

                            var runtime = engine.Runtime;
                            runtime.LoadAssembly(Assembly.GetExecutingAssembly());
                            runtime.LoadAssembly(typeof (TESVSnip.Domain.Model.BaseRecord).Assembly);
                            runtime.LoadAssembly(typeof(String).Assembly);
                            runtime.LoadAssembly(typeof(System.Drawing.Icon).Assembly);
                            runtime.LoadAssembly(typeof(IronPython.Hosting.Python).Assembly);
                            runtime.LoadAssembly(typeof(System.Dynamic.DynamicObject).Assembly);
                            runtime.LoadAssembly(typeof(System.Windows.Forms.Cursor).Assembly);

                            c.ScriptScope.SetVariable("__window__", this);
                            c.ScriptScope.SetVariable("__plugins__", PluginList.All);
                            c.ScriptScope.SetVariable("__options__", Options.Value);
                            c.ScriptScope.SetVariable("__settings__", Settings.Default);
                            c.ScriptScope.SetVariable("plugins", PluginList.All);
                            c.ScriptScope.SetVariable("exit", new Action(() => BeginInvoke(new Action(pyWindow.Close))));
                        });
                }
            }
            catch 
            {
                
            }
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
                form = new T() { Text = name };
                form.Show(this.dockPanel, DockState.Document);
            }
            if (!form.Visible)
            {
                form.Show(this.dockPanel, DockState.Document);
            }
            return form;
        }

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
        void outputWindow_Closed(object sender, EventArgs e)
        {
            outputTextContent = null;
            PluginEngine.Default.OnConsoleMessage -= pluginEngine_OnConsoleMessage;
        }
        void pluginEngine_OnConsoleMessage(object sender, PluginEngine.MessageEventArgs e)
        {
            if (outputTextContent != null)
            {
                outputTextContent.AppendText(e.Text);
            }
        }
        #endregion

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
                    var item = new ToolStripMenuItem()
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
                    var item = new ToolStripMenuItem()
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

            var rootUri = new Uri(Path.Combine(PluginEngine.PluginsPyPath,"."), UriKind.Absolute);
            foreach (
                var filename in
                    Directory.EnumerateFiles(PluginEngine.PluginsPyPath, "*.py", SearchOption.TopDirectoryOnly))
            {
                var relativePath = rootUri.MakeRelativeUri(new Uri(filename, UriKind.Absolute)).ToString();
                // was going to show subdirectories but will leave that alone for now
                var item = new ToolStripMenuItem()
                {
                    Name = Path.GetFileNameWithoutExtension(relativePath),
                    Text = Path.GetFileNameWithoutExtension(relativePath),
                    Image = Properties.Resources.PythonScript32x32,
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
            this.ClearDynamicScripts();
            var outputWindow = GetWindowByName<OutputTextContent>("Output");
            if (outputWindow != null)
                outputWindow.UpdateText("");
            PluginEngine.Default.Reinitialize();
            this.LoadDynamicScripts();
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
                this.PluginTree.SelectedRecord != null;
        }

        private void globalScriptsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag != null)
            {
                var name = e.ClickedItem.Tag as string;
                PluginEngine.Default.ExecuteByName(name);
            }
        }

        private System.Collections.IList BuildSelectionList()
        {
            var recs = this.PluginTree.SelectedRecords;
            return recs != null ? recs.ToArray() : new object[0];
        }

        private void selectionScriptsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var recs = BuildSelectionList();
            if (recs.Count == 0) return;
            foreach (ToolStripMenuItem menu in this.selectionScriptsToolStripMenuItem.DropDownItems)
            {
                var name = menu.Tag as string;
                menu.Enabled = !string.IsNullOrEmpty(name) &&
                                PluginEngine.Default.IsValidSelectionByName(name, recs);
            }
        }

        private void selectionScriptsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var recs = BuildSelectionList();
            if (recs.Count == 0) return;

            var name = e.ClickedItem.Tag as string;
            if (e.ClickedItem.Enabled)
                PluginEngine.Default.ExecuteSelectionByName(name, recs);
        }
        #endregion

        [DllImport("shell32.dll")]
        static extern int FindExecutable(string lpFile, string lpDirectory, [Out] System.Text.StringBuilder lpResult);

        private void editScriptsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var filename = e.ClickedItem.Tag as string;
            if (!string.IsNullOrEmpty(filename))
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.ExternalPythonEditor))
                {
                    Process.Start(Properties.Settings.Default.ExternalPythonEditor, filename);
                }
                else
                {
                    var info = new ProcessStartInfo(filename);
                    if (info.Verbs.Length > 0)
                    {
                        // try to find default editor via Open With. as usually python is default "open" which just runs it
                        //  after that try to find "edit" then one of the "edit with using notepad" then any edit with
                        using (var key = Registry.CurrentUser.OpenSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Path.GetExtension(filename) + @"\OpenWithList", false))
                        {
                            if (key != null)
                            {
                                foreach (var item in key.GetValue("MRUList", "").ToString())
                                {
                                    var process = key.GetValue(item.ToString(), "").ToString();
                                    if (!string.IsNullOrEmpty(process) && !process.StartsWith("python") && !process.StartsWith("py.exe"))
                                    {
                                        var tmpinfo = new ProcessStartInfo(process, filename) { Verb = "open" };
                                        if (tmpinfo.Verbs.Contains("open", StringComparer.InvariantCultureIgnoreCase)) // make sure open is a valid verb
                                        {
                                            info = tmpinfo;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(info.Verb))
                            info.Verb = info.Verbs.FirstOrDefault(
                                (v) => 0 == string.Compare(v, "edit", StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrEmpty(info.Verb))
                            info.Verb = info.Verbs.FirstOrDefault(
                                (v) => v.StartsWith("edit", StringComparison.InvariantCultureIgnoreCase)
                                    && v.IndexOf("notepad", StringComparison.InvariantCultureIgnoreCase) > 0
                                );
                        if (string.IsNullOrEmpty(info.Verb))
                            info.Verb = info.Verbs.FirstOrDefault(
                                (v) => v.StartsWith("edit", StringComparison.InvariantCultureIgnoreCase) );
                    }
                    info.WorkingDirectory = Path.GetDirectoryName(filename);
                    info.WindowStyle = ProcessWindowStyle.Normal;
                    Process.Start(info);
                }
            }
        }

    }
}