#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml.Serialization;
using IronPython.Hosting;
using JWC;
using Microsoft.Win32;
using FalloutSnip.Domain.Data;
using FalloutSnip.Domain.Data.Structure;
using FalloutSnip.Domain.Data.Structure.Xml;
using FalloutSnip.Domain.Model;
using FalloutSnip.UI.Scripts;
using FalloutSnip.Domain.Services;
using FalloutSnip.Framework;
using FalloutSnip.Properties;
using FalloutSnip.TranslateUI;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.ObjectControls;
using FalloutSnip.UI.Rendering;
using FalloutSnip.UI.Services;
using WeifenLuo.WinFormsUI.Docking;
using Encoding = FalloutSnip.Framework.Services.Encoding;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Settings = FalloutSnip.Properties.Settings;
using Timer = System.Threading.Timer;

#endregion

namespace FalloutSnip.UI.Forms
{
    internal partial class MainView : Form
    {

        private readonly MruStripMenu mruMenu;

        private volatile bool backgroundWorkCanceled;

        private bool inRebuildSelection;


        public MainView()
        {
            // TODO: Load Record Structure?
            //if (!RecordStructure.Loaded)
            //{
            //    try
            //    {
            //        RecordStructure.Load();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(Resources.CannotParseRecordStructure + ex.Message, Resources.WarningText);
            //    }
            //}

            DomainDefinition.DomainLoaded += DomainDefinitionOnDomainLoaded;

            InitializeComponent();
            InitializeToolStripFind();
            InitializeDockingWindows();
            RegisterMessageFilter();

            PluginTree.SelectionChanged += (o, e) => RebuildSelection();

            if (string.IsNullOrEmpty(Settings.Default.DefaultSaveFolder) ||
                !Directory.Exists(Settings.Default.DefaultSaveFolder))
            {
                SaveModDialog.InitialDirectory = Options.Value.GameDataDirectory;
            }
            else
            {
                SaveModDialog.InitialDirectory = Settings.Default.DefaultSaveFolder;
            }

            if (string.IsNullOrEmpty(Settings.Default.DefaultOpenFolder) ||
                !Directory.Exists(Settings.Default.DefaultOpenFolder))
            {
                OpenModDialog.InitialDirectory = Options.Value.GameDataDirectory;
            }
            else
            {
                OpenModDialog.InitialDirectory = Settings.Default.DefaultOpenFolder;
            }

            Icon = Resources.fosnip;
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

            useWindowsClipboardToolStripMenuItem.Checked = Settings.Default.UseWindowsClipboard;
            noWindowsSoundsToolStripMenuItem.Checked = Settings.Default.NoWindowsSounds;
            saveStringsFilesToolStripMenuItem.Checked = Domain.Properties.Settings.Default.SaveStringsFiles;

            useNewSubrecordEditorToolStripMenuItem.Checked = !Settings.Default.UseOldSubRecordEditor;
            hexModeToolStripMenuItem.Checked = Settings.Default.UseHexSubRecordEditor;
            uTF8ModeToolStripMenuItem.Checked = Framework.Properties.Settings.Default.UseUTF8;

            Selection = new SelectionContext();
            Selection.formIDLookup = LookupFormIDI;
            Selection.strLookup = LookupFormStrings;
            Selection.formIDLookupR = GetRecordByID;

            SubrecordList.SetContext(Selection);
            InitializeLanguage();

            ClipboardChanged += (o, e) => RebuildSelection();
            Selection.RecordChanged += (o, a) => RebuildSelection();
            Selection.SubRecordChanged += (o, a) => RebuildSelection();

            PluginTree.OnSelectionUpdated += PluginTree_OnSelectionUpdated;

            PluginTree.SelectionChanged += PluginTree_SelectionChanged;
            SubrecordList.SelectionChanged += subrecordPanel_SelectionChanged;
            SubrecordList.OnSubrecordChanged += subrecordPanel_OnSubrecordChanged;
            SubrecordList.DataChanged += subrecordPanel_DataChanged;
            BaseRecord.ChildListChanged += PluginList_ChildListChanged;

            htmlContent.OnLinkClicked += htmlContent_OnLinkClicked;
            InitializeGameOptions();
            LocalizeApp();
            PyInterpreter.InitPyInterpreter();
            HtmlRenderer.Initialize();
            mruMenu = new MruStripMenu(recentFilelToolStripMenuItem, OnMruFile,
                                       mruRegKey + "\\MRU", true, 16);
        }

        private void InitializeGameOptions()
        {
            string defaultDomain = Properties.Settings.Default.DefaultDomain ?? "Skyrim";
            defaultGameSettingsToolStripMenuItem.DropDownItems.Clear();
            foreach (var domain in FalloutSnip.Domain.Data.DomainDefinition.AllDomains())
            {
                var item = new ToolStripMenuItem
                    {
                        Name = domain.Name,
                        Text = string.IsNullOrWhiteSpace(domain.DisplayName) ? domain.Name : domain.DisplayName,
                        Visible = true,
                        Enabled = true,
                        Tag = domain.Name,
                        Checked = defaultDomain == domain.Name,
                    };
                defaultGameSettingsToolStripMenuItem.DropDownItems.Add(item);
            }
            defaultGameSettingsToolStripMenuItem.DropDownItemClicked +=
                delegate(object sender, ToolStripItemClickedEventArgs e)
                    {
                        Properties.Settings.Default.DefaultDomain = e.ClickedItem.Tag.ToString();
                        foreach (var item in defaultGameSettingsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>())
                            item.Checked = Properties.Settings.Default.DefaultDomain == item.Tag.ToString();
                        Options.Value.Reconfigure();
                    };
        }

        private PluginTreeView PluginTree
        {
            get { return pluginTreeContent.PluginTree; }
        }

        private SubrecordListEditor SubrecordList
        {
            get { return subrecordListContent.SubrecordList; }
        }

        private void DomainDefinitionOnDomainLoaded(object sender, EventArgs eventArgs)
        {
            ReinitializeToolStripFind();
        }

        private void PluginList_ChildListChanged(object sender, RecordChangeEventArgs e)
        {
            RebuildSelection();
            UpdateStringEditor();
            FixMasters();
            PluginTree.UpdateRoots();
        }

        public static event EventHandler ClipboardChanged;

        public void CancelBackgroundProcess()
        {
            backgroundWorkCanceled = true;
            backgroundWorker1.CancelAsync();
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
                if (dockPanel.ActiveDocumentPane != null)
                {
                    form.Show(dockPanel.ActiveDocumentPane, DockAlignment.Bottom, 0.33);
                }
                else
                {
                    form.Show(dockPanel, DockState.Document);
                }
            }

            return form;
        }

        public bool IsBackroundProcessCanceled()
        {
            return backgroundWorkCanceled;
        }

        public void StartBackgroundWork(Action workAction, Action completedAction)
        {
            if (backgroundWorker1.IsBusy)
            {
                return;
            }

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

        internal static void PostReferenceSearch(uint formid)
        {
            var form = Application.OpenForms.OfType<MainView>().FirstOrDefault();
            if (form != null)
            {
                form.ReferenceSearch(formid);
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
                var p = new Plugin(s, false, GetRecordFilter(s));
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
                ClearCachedInfo();
            }
        }

        internal bool PreFilterMessage(ref Message m)
        {
            // Intercept the left mouse button down message.
            if (m.Msg == MainViewMessageFilter.WM_KEYDOWN)
            {
                if (m.WParam == new IntPtr((int) Keys.F6))
                {
                    var current = dockPanel.ActiveContent;
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


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private void EnableUserInterface(bool enable)
        {
            // ControlEnable(this.splitHorizontal, enable);
            // ControlEnable(this.splitVertical, enable);
            ControlEnable(menuStrip1, enable);
            ControlEnable(toolStripIncrFind, enable);
            ControlEnable(toolStripIncrInvalidRec, enable);
        }

        private void FixMasters()
        {
            PluginList.FixMasters();
        }



        private string[] GetRecordFilter(string s)
        {
            string[] recFilter = null;
            bool bAskToApplyFilter = true;
            if (Settings.Default.IsFirstTimeOpeningSkyrimESM)
            {
                if (string.Compare(Path.GetFileName(s), "skyrim.esm", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var result = MessageBox.Show(
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
                    var result = MessageBox.Show(
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


        private void MainView_Load(object sender, EventArgs e)
        {
            LoadDockingWindows();
            FixMasters();
            toolStripIncrFind.Visible = false;
            //toolStripIncrFind.Enabled = false;
            toolStripIncrInvalidRec.Visible = false;
            //toolStripIncrInvalidRec.Enabled = false;
            BuildDynamicScriptsMenu();
        }

        private void MainView_Shown(object sender, EventArgs e)
        {
            //// Only prevent content hiding after window if first shown
            // dockingManagerExtender.DockingManager.ContentHiding +=
            // delegate(Content c, CancelEventArgs cea) { cea.Cancel = true; };
            // dockingManagerExtender.DockingManager.ShowAllContents();
            ShowDockingWindows();

            if (!DesignMode)
            {
                try
                {
                    var asm = Assembly.GetExecutingAssembly();
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
        ///     This routine assigns Structure definitions to subrecords.
        /// </summary>
        /// <returns>
        ///     The System.Boolean.
        /// </returns>
        private bool MatchRecordStructureToRecord()
        {
            var rec = Selection.Record as Record;
            if (rec == null)
            {
                return false;
            }

            return rec.MatchRecordStructureToRecord();
        }


        private void ReferenceSearch(uint formid)
        {
            var search = CreateSearchWindow();
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


        private void TESsnip_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.UseWindowsClipboard = useWindowsClipboardToolStripMenuItem.Checked;
            PluginList.All.Clear();
            PluginTree.UpdateRoots();
            Clipboard = null;
            Selection.Record = null;
            RebuildSelection();
            CloseStringEditor();
            SaveDockingWindows();
            Services.Settings.SetWindowPosition("TESsnip", this);
        }

        private void UpdateMainText(BaseRecord rec)
        {
            if (rec == null)
            {
                UpdateMainText(string.Empty);
            }
            else
            {
                try
                {
                    string html = HtmlRenderer.GetDescription(rec);
                    htmlContent.UpdateText(html);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.WarningText);
                }
            }
        }

        private void UpdateMainText(string text)
        {
            htmlContent.UpdateText(text);
        }

        private void addMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var amfNewMaster = new AddMasterForm())
            {
                if (amfNewMaster.ShowDialog(this) == DialogResult.OK)
                {
                    var plugin = GetPluginFromNode(PluginTree.SelectedRecord);
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
            PluginTree.UpdateRoots();
            Selection.Reset();
            SubrecordList.Record = null;
            Clipboard = null;
            CloseStringEditor();
            UpdateMainText(string.Empty);
            ClearCachedInfo();
        }

        private void ClearCachedInfo()
        {
            // Following is mostly about cleaning up leaked / referenced memory
            SubrecordList.SetContext(Selection);
            RebuildSelection();
            PluginTree.UpdateRoots();
            Selection.Reset();
            SubrecordList.SetContext(Selection);
            GC.Collect();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }

            if (MessageBox.Show(Resources.CloseActivePluginInquiry, Resources.WarningText, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }

            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            PluginList.All.DeleteRecord(p);
            UpdateStringEditor();
            UpdateMainText(string.Empty);
            FixMasters();
            PluginTree.UpdateRoots();
            RebuildSelection();
            ClearCachedInfo();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.CollapseAll();
        }

        private void collapseBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.CollapseAll(PluginTree.SelectedRecord);
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
            CopySelection();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Selection.SelectedSubrecord && PluginTree.SelectedRecord != null &&
                PluginTree.SelectedRecord is Plugin)
            {
                MessageBox.Show(Resources.Cannot_cut_a_plugin, Resources.ErrorText);
                return;
            }

            copyToolStripMenuItem_Click(null, null);
            deleteToolStripMenuItem_Click(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.ContainsFocus)
            {
                PluginTree.DeleteSelection();
            }
            else if (SubrecordList.ContainsFocus)
            {
                SubrecordList.DeleteSelection();
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


            var allRecords = DomainDefinition.GetRecordNames().Select(x => x.ToString()).ToList();
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
            if (PluginTree.ContainsFocus)
            {
                PluginTree.EditSelectedHeader();
            }
            else if (SubrecordList.ContainsFocus)
            {
                SubrecordList.EditSelectedSubrecordHex();
            }
        }

        private void editSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.ContainsFocus)
            {
                PluginTree.EditSelectedRecord();
            }
            else if (SubrecordList.ContainsFocus)
            {
                SubrecordList.EditSelectedSubrecord();
            }
        }

        private void editStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenStringEditor();
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            pasteToolStripMenuItem.Enabled = HasClipboardData();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.ExpandAll();
        }

        private void expandBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginTree.ExpandAll(PluginTree.SelectedRecord);
        }

        private void findInRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateSearchWindow();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!toolStripIncrFind.Visible)
            {
                toolStripIncrFind.Enabled = true;
                toolStripIncrFind.Visible = true;
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.SelectAll();
                toolStripIncrFindText.Focus();
            }
            else
            {
                toolStripIncrFind.Visible = false;
                //toolStripIncrFind.Enabled = false;
            }
        }

        private void hexModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseHexSubRecordEditor = hexModeToolStripMenuItem.Checked;
        }

        private void insertGroupToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var node = PluginTree.SelectedRecord;
            var p = new GroupRecord("NEW_");
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
        }

        private void insertRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = PluginTree.SelectedRecord;

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
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
        }

        private void insertSubrecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = PluginTree.SelectedRecord;
            var p = new SubRecord();
            node.AddRecord(p);
            GetPluginFromNode(PluginTree.SelectedRecord).InvalidateCache();
            PluginTree.RefreshObject(node);
            RebuildSelection();
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


        private void newFormIDNoReferenceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            giveSelectionNewFormID(false);
        }

        private void newFormIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            giveSelectionNewFormID(true);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPlugin();
        }

        public Plugin NewPlugin()
        {
            var define = DomainDefinition.Load("Skyrim");
            var p = new Plugin(define);
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
            RebuildSelection();
            UpdateStringEditor();
            FixMasters();
            PluginTree.UpdateRoots();
        }

        private void noWindowsSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.NoWindowsSounds =
                noWindowsSoundsToolStripMenuItem.Checked = !noWindowsSoundsToolStripMenuItem.Checked;
        }

        private void openNewPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenModDialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadPluginFromListOfFileNames(OpenModDialog.FileNames);
            }
        }

        private void LoadPluginFromListOfFileNames(string[] fileNames)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                PluginTree.EnableEvents(false);

                try
                {
                    foreach (var s in fileNames)
                    {
                        LoadPlugin(s);
                        mruMenu.AddFileAndSaveToRegistry(s);
                    }
                }
                finally
                {
                    FixMasters();
                    PluginTree.UpdateRoots();
                    PluginTree.EnableEvents(true);
                    ClearCachedInfo();
                    sw.Stop();
                    var t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                    toolStripStatusLabel.Text =
                        string.Format(TranslateUiGlobalization.ResManager.GetString("MSG_LoadPluginIn"), t.ToString());
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
                    "GetType: " + ex.GetType();

                System.Windows.Forms.Clipboard.SetDataObject(errMsg, true);

                // Create an EventLog instance and assign its source.
                var myLog = new EventLog();
                myLog.Source = "ThreadException";
                myLog.WriteEntry(errMsg);

                MessageBox.Show(errMsg, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void pasteNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false, true);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false, false);
        }

        private void reloadStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadLanguageFiles();
        }

        private void reloadXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FalloutSnip.Domain.Data.DomainDefinition.Reload();

                foreach (var rec in PluginList.All.Enumerate(x => x is Record).OfType<Record>())
                {
                    rec.MatchRecordStructureToRecord();
                }

                RebuildSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.CannotParseRecordStructure + ex.Message, Resources.WarningText);
            }
        }

        private void resetDockingWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutDockingWindows(force: true);
        }

        private void htmlContent_OnLinkClicked(object sender, LinkClickedEventArgs e)
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
                                                     , MessageBoxDefaultButton.Button2);
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
                                            LoadPluginFromListOfFileNames(new[] {masterName});
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
                            PluginTree.SelectedRecord = node;
                            return;
                        }
                    }

                    var startNode = startPlugin ?? PluginTree.SelectedRecord ?? PluginTree.TopRecord;

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
                                    var node = PerformSearch(searchContext);
                                    if (node != null)
                                    {
                                        PluginTree.SelectedRecord = node;
                                    }
                                });
                        contextMenu.MenuItems.Add("Find &References", (o, args) => ReferenceSearch(formID));
                        contextMenu.Show(this, PointToClient(MousePosition));
                    }
                    else
                    {
                        var node = PerformSearch(searchContext);
                        if (node != null)
                        {
                            PluginTree.SelectedRecord = node;
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
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelectedToSave, Resources.ErrorText);
                return;
            }

            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            if (p.Filtered)
            {
                var result = MessageBox.Show(
                    this, Resources.SavePluginWithFilterAppliedInquiry, Resources.WarningText, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            SaveModDialog.FileName = p.Name;
            if (SaveModDialog.ShowDialog(this) == DialogResult.OK)
            {
                var sw = Stopwatch.StartNew();
                p.Save(SaveModDialog.FileName);
                mruMenu.AddFileAndSaveToRegistry(SaveModDialog.FileName);
                FixMasters();
                sw.Stop();
                var t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                toolStripStatusLabel.Text =
                    string.Format(TranslateUiGlobalization.ResManager.GetString("MSG_SavePluginIn"), t.ToString());
            }
        }

        private void saveStringsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Domain.Properties.Settings.Default.SaveStringsFiles =
                saveStringsFilesToolStripMenuItem.Checked = !saveStringsFilesToolStripMenuItem.Checked;
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
                var result = MessageBox.Show(
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
            FixMasters();
        }

        private void searchAdvancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecordStructure recStruct = null;
            var rec = PluginTree.SelectedRecord;
            if (rec is Record)
            {
                recStruct = ((Record) rec).GetStructure();
            }

            if (recStruct == null)
            {
                recStruct = DomainDefinition.LoadedDomains().First().Records.Values.Random(1).First();
            }

            using (var dlg = new SearchFilterAdvanced(recStruct))
            {
                dlg.ShowDialog(this);
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = searchToolStripMenuItem.Tag as SearchCriteriaSettings;
            using (var dlg = new SearchFilterBasic())
            {
                var recStructs = DomainDefinition.LoadedDomains().First().Records;
                RecordStructure recStruct = null;
                if (settings != null)
                {
                    foreach (var domain in DomainDefinition.LoadedDomains())
                    {
                        if (domain.Records.TryGetValue(settings.Type, out recStruct))
                        {
                            recStructs = domain.Records;
                            dlg.SetRecordStructure(recStruct);
                            dlg.Criteria = settings;
                            break;
                        }
                    }
                }

                if (recStruct == null)
                {
                    var rec = PluginTree.SelectedRecord;
                    if (rec is GroupRecord)
                    {
                        var gr = rec as GroupRecord;
                        var ct = gr.ContentsType;
                        if (!string.IsNullOrEmpty(ct))
                        {
                            recStructs.TryGetValue(ct, out recStruct);
                        }
                    }
                    else if (rec is Record)
                    {
                        recStructs.TryGetValue(rec.Name, out recStruct);
                    }

                    dlg.SetRecordStructure(recStruct);
                }

                dlg.EnableFindAll(false); // hide final all since we will open 
                if (DialogResult.Cancel != dlg.ShowDialog(this))
                {
                    searchToolStripMenuItem.Tag = dlg.Criteria;
                    var window = CreateSearchWindow();
                    window.SetSearchCriteria(dlg.Criteria, doSearch: true);
                }
            }
        }

        private void subrecordPanel_DataChanged(object sender, EventArgs e)
        {
            var sr = SubrecordList.GetSelectedSubrecord();
            if (sr != null)
            {
                UpdateMainText(sr);
            }
        }

        private void subrecordPanel_OnSubrecordChanged(object sender, RecordChangeEventArgs e)
        {
            if (e.Record is SubRecord)
            {
                if (e.Record.Parent is Record)
                {
                    PluginTree.RefreshObject(e.Record.Parent);
                }

                SubrecordList.RefreshObject(e.Record);
            }
        }

        private void subrecordPanel_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMainText(SubrecordList.SubRecord);
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
            toolStripIncrInvalidRec.Visible = false;
            //toolStripIncrInvalidRec.Enabled = false;
        }

        private void toolStripIncrInvalidRecNext_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                return;
            }

            BackgroundNonConformingRecordIncrementalSearch(PluginTree.SelectedRecord, true,
                                                           toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecPrev_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                return;
            }

            BackgroundNonConformingRecordIncrementalSearch(PluginTree.SelectedRecord, false,
                                                           toolStripIncrInvalidRecWrapAround.Checked);
        }

        private void toolStripIncrInvalidRecRestart_Click(object sender, EventArgs e)
        {
            var rec = PluginList.All.Records.OfType<BaseRecord>().FirstOrDefault();
            if (rec == null)
            {
                return;
            }

            BackgroundNonConformingRecordIncrementalSearch(rec, true, false);
        }

        private void toolStripIncrInvalidRec_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void toolStripIncrInvalidRec_VisibleChanged(object sender, EventArgs e)
        {
            findNonconformingRecordToolStripMenuItem.Checked = toolStripIncrInvalidRec.Visible;
            toolStripIncrInvalidRecStatus.Text = "Select Next or Prev to start search.";
            toolStripIncrInvalidRecStatus.ForeColor = Color.DarkGray;
        }

        private void toolStripStopProgress_Click(object sender, EventArgs e)
        {
            CancelBackgroundProcess();
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
        }

        private void uTF8ModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Framework.Properties.Settings.Default.UseUTF8 = uTF8ModeToolStripMenuItem.Checked;
            if (MessageBox.Show(Resources.RestartText, Resources.InfoText, MessageBoxButtons.YesNoCancel) ==
                DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void useNewSubrecordEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseOldSubRecordEditor = !useNewSubrecordEditorToolStripMenuItem.Checked;
        }

        private void useWindowsClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseWindowsClipboard =
                useWindowsClipboardToolStripMenuItem.Checked = !useWindowsClipboardToolStripMenuItem.Checked;
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

                var origCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                var sw = Stopwatch.StartNew();
                PluginTree.EnableEvents(false);
                try
                {
                    using (var fs = File.OpenText(dlg.FileName))
                    {
                        CloseAllPlugins();
                        bool checkSkyrim =
                            string.Compare(dlg.FileName, defaultFile, StringComparison.InvariantCultureIgnoreCase) == 0;
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
                                if (
                                    string.Compare(Path.GetFileName(file), "skyrim.esm",
                                                   StringComparison.InvariantCultureIgnoreCase) != 0)
                                {
                                    var skyrimFile = Path.Combine(Options.Value.GameDataDirectory, "skyrim.esm");
                                    if (File.Exists(skyrimFile))
                                        PluginList.All.AddRecord(new Plugin(skyrimFile, false,
                                                                            GetRecordFilter(skyrimFile)));
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
                    PluginTree.EnableEvents(true);
                    ClearCachedInfo();

                    Cursor.Current = origCursor;

                    sw.Stop();
                    var t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                    toolStripStatusLabel.Text =
                        string.Format(TranslateUiGlobalization.ResManager.GetString("MSG_LoadPluginIn"), t.ToString());
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

                using (var fs = File.OpenWrite(dlg.FileName))
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

        /// <summary>
        ///     Event for MRU List
        /// </summary>
        /// <param name="number"></param>
        /// <param name="filename"></param>
        private void OnMruFile(int number, String filename)
        {
            if (File.Exists(filename))
            {
                var fileNames = new[] {filename};
                mruMenu.SetFirstFile(number);
                Update();
                LoadPluginFromListOfFileNames(fileNames);
            }
            else
            {
                string msg = string.Format(TranslateUiGlobalization.ResManager.GetString("UI_MRU_FileNotExist"),
                                           filename);
                MessageBox.Show(msg, "FalloutSnip", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mruMenu.RemoveFile(number);
            }
        }

        private void resetSettingsToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
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
                MessageBox.Show(ex.Message, "FalloutSnip", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                            runtime.LoadAssembly(typeof (BaseRecord).Assembly);
                            runtime.LoadAssembly(typeof (String).Assembly);
                            runtime.LoadAssembly(typeof (Icon).Assembly);
                            runtime.LoadAssembly(typeof (Python).Assembly);
                            runtime.LoadAssembly(typeof (DynamicObject).Assembly);
                            runtime.LoadAssembly(typeof (Cursor).Assembly);

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


        private void editScriptsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var filename = e.ClickedItem.Tag as string;
            if (!string.IsNullOrEmpty(filename))
            {
                if (!string.IsNullOrEmpty(Settings.Default.ExternalPythonEditor))
                {
                    Process.Start(Settings.Default.ExternalPythonEditor, filename);
                }
                else
                {
                    var info = new ProcessStartInfo(filename);
                    if (info.Verbs.Length > 0)
                    {
                        // try to find default editor via Open With. as usually python is default "open" which just runs it
                        //  after that try to find "edit" then one of the "edit with using notepad" then any edit with
                        using (var key = Registry.CurrentUser.OpenSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" +
                            Path.GetExtension(filename) + @"\OpenWithList", false))
                        {
                            if (key != null)
                            {
                                foreach (var item in key.GetValue("MRUList", "").ToString())
                                {
                                    var process = key.GetValue(item.ToString(), "").ToString();
                                    if (!string.IsNullOrEmpty(process) && !process.StartsWith("python") &&
                                        !process.StartsWith("py.exe"))
                                    {
                                        var tmpinfo = new ProcessStartInfo(process, filename) {Verb = "open"};
                                        if (tmpinfo.Verbs.Contains("open", StringComparer.InvariantCultureIgnoreCase))
                                            // make sure open is a valid verb
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
                                (v) => v.StartsWith("edit", StringComparison.InvariantCultureIgnoreCase));
                    }
                    info.WorkingDirectory = Path.GetDirectoryName(filename);
                    info.WindowStyle = ProcessWindowStyle.Normal;
                    Process.Start(info);
                }
            }
        }

    }
}