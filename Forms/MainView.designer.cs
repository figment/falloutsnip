namespace TESVSnip {
    partial class MainView {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewPluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertSubrecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hexModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useNewSubrecordEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lookupFormidsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eSMFilterSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spellsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sanitizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stripEDIDsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findDuplicatedFormIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpEDIDListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanEspToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNonconformingRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateLLXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeEsmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.martigensToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createRecordStructureXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeRecordsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reorderSubrecordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editStringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenModDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveModDialog = new System.Windows.Forms.SaveFileDialog();
            this.SaveEdidListDialog = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStopProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.splitHorizontal = new System.Windows.Forms.SplitContainer();
            this.splitVertical = new System.Windows.Forms.SplitContainer();
            this.PluginTree = new System.Windows.Forms.TreeView();
            this.listSubrecord = new TESVSnip.Windows.Controls.BindingListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripSubRecord = new System.Windows.Forms.ToolStrip();
            this.toolStripInsertRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripDeleteRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMoveRecordUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripMoveRecordDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripEditSubrecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripEditSubrecordHex = new System.Windows.Forms.ToolStripButton();
            this.toolStripPasteSubrecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripCopySubrecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbInfo = new System.Windows.Forms.TextBox();
            this.toolStripIncrFind = new System.Windows.Forms.ToolStrip();
            this.toolStripIncrFindCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripIncrFindNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindRestart = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindType = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripIncrFindMatch = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindExact = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrFindDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrInvalidRec = new System.Windows.Forms.ToolStrip();
            this.toolStripIncrInvalidRecCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrInvalidRecText = new System.Windows.Forms.ToolStripLabel();
            this.toolStripIncrInvalidRecNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrInvalidRecPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrInvalidRecRestart = new System.Windows.Forms.ToolStripButton();
            this.toolStripIncrInvalidRecDown = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.splitHorizontal.Panel1.SuspendLayout();
            this.splitHorizontal.Panel2.SuspendLayout();
            this.splitHorizontal.SuspendLayout();
            this.splitVertical.Panel1.SuspendLayout();
            this.splitVertical.Panel2.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.toolStripSubRecord.SuspendLayout();
            this.toolStripIncrFind.SuspendLayout();
            this.toolStripIncrInvalidRec.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.spellsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(667, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openNewPluginToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem,
            this.toolStripSeparator3,
            this.reloadXmlToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openNewPluginToolStripMenuItem
            // 
            this.openNewPluginToolStripMenuItem.Name = "openNewPluginToolStripMenuItem";
            this.openNewPluginToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openNewPluginToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openNewPluginToolStripMenuItem.Text = "&Open";
            this.openNewPluginToolStripMenuItem.Click += new System.EventHandler(this.openNewPluginToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.closeAllToolStripMenuItem.Text = "Close &All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(143, 6);
            // 
            // reloadXmlToolStripMenuItem
            // 
            this.reloadXmlToolStripMenuItem.Name = "reloadXmlToolStripMenuItem";
            this.reloadXmlToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.reloadXmlToolStripMenuItem.Text = "Reload &Xml";
            this.reloadXmlToolStripMenuItem.Click += new System.EventHandler(this.reloadXmlToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.insertRecordToolStripMenuItem,
            this.insertSubrecordToolStripMenuItem,
            this.findToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Enabled = false;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Enabled = false;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.deleteToolStripMenuItem.Text = "&Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // insertRecordToolStripMenuItem
            // 
            this.insertRecordToolStripMenuItem.Enabled = false;
            this.insertRecordToolStripMenuItem.Name = "insertRecordToolStripMenuItem";
            this.insertRecordToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.insertRecordToolStripMenuItem.Text = "&New record";
            this.insertRecordToolStripMenuItem.Click += new System.EventHandler(this.insertRecordToolStripMenuItem_Click);
            // 
            // insertSubrecordToolStripMenuItem
            // 
            this.insertSubrecordToolStripMenuItem.Enabled = false;
            this.insertSubrecordToolStripMenuItem.Name = "insertSubrecordToolStripMenuItem";
            this.insertSubrecordToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.insertSubrecordToolStripMenuItem.Text = "New subrecord";
            this.insertSubrecordToolStripMenuItem.Click += new System.EventHandler(this.insertSubrecordToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.findToolStripMenuItem.Text = "&Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hexModeToolStripMenuItem,
            this.useNewSubrecordEditorToolStripMenuItem,
            this.lookupFormidsToolStripMenuItem,
            this.eSMFilterSettingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // hexModeToolStripMenuItem
            // 
            this.hexModeToolStripMenuItem.Checked = true;
            this.hexModeToolStripMenuItem.CheckOnClick = true;
            this.hexModeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hexModeToolStripMenuItem.Name = "hexModeToolStripMenuItem";
            this.hexModeToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.hexModeToolStripMenuItem.Text = "&Hex mode";
            // 
            // useNewSubrecordEditorToolStripMenuItem
            // 
            this.useNewSubrecordEditorToolStripMenuItem.Checked = true;
            this.useNewSubrecordEditorToolStripMenuItem.CheckOnClick = true;
            this.useNewSubrecordEditorToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useNewSubrecordEditorToolStripMenuItem.Name = "useNewSubrecordEditorToolStripMenuItem";
            this.useNewSubrecordEditorToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.useNewSubrecordEditorToolStripMenuItem.Text = "Use new subrecord editor";
            // 
            // lookupFormidsToolStripMenuItem
            // 
            this.lookupFormidsToolStripMenuItem.Checked = true;
            this.lookupFormidsToolStripMenuItem.CheckOnClick = true;
            this.lookupFormidsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lookupFormidsToolStripMenuItem.Name = "lookupFormidsToolStripMenuItem";
            this.lookupFormidsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.lookupFormidsToolStripMenuItem.Text = "Lookup formids";
            this.lookupFormidsToolStripMenuItem.Click += new System.EventHandler(this.lookupFormidsToolStripMenuItem_Click);
            // 
            // eSMFilterSettingsToolStripMenuItem
            // 
            this.eSMFilterSettingsToolStripMenuItem.Name = "eSMFilterSettingsToolStripMenuItem";
            this.eSMFilterSettingsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.eSMFilterSettingsToolStripMenuItem.Text = "ESM &Filter Settings...";
            this.eSMFilterSettingsToolStripMenuItem.Click += new System.EventHandler(this.eSMFilterSettingsToolStripMenuItem_Click);
            // 
            // spellsToolStripMenuItem
            // 
            this.spellsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sanitizeToolStripMenuItem,
            this.stripEDIDsToolStripMenuItem,
            this.findDuplicatedFormIDToolStripMenuItem,
            this.dumpEDIDListToolStripMenuItem,
            this.cleanEspToolStripMenuItem,
            this.findNonconformingRecordToolStripMenuItem,
            this.compileScriptToolStripMenuItem,
            this.compileAllToolStripMenuItem,
            this.generateLLXmlToolStripMenuItem,
            this.makeEsmToolStripMenuItem,
            this.martigensToolStripMenuItem,
            this.createRecordStructureXmlToolStripMenuItem,
            this.mergeRecordsXMLToolStripMenuItem,
            this.reorderSubrecordsToolStripMenuItem,
            this.editStringsToolStripMenuItem});
            this.spellsToolStripMenuItem.Name = "spellsToolStripMenuItem";
            this.spellsToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.spellsToolStripMenuItem.Text = "&Spells";
            // 
            // sanitizeToolStripMenuItem
            // 
            this.sanitizeToolStripMenuItem.Name = "sanitizeToolStripMenuItem";
            this.sanitizeToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.sanitizeToolStripMenuItem.Text = "Sanitize";
            this.sanitizeToolStripMenuItem.Click += new System.EventHandler(this.sanitizeToolStripMenuItem_Click);
            // 
            // stripEDIDsToolStripMenuItem
            // 
            this.stripEDIDsToolStripMenuItem.Name = "stripEDIDsToolStripMenuItem";
            this.stripEDIDsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.stripEDIDsToolStripMenuItem.Text = "Strip EDIDs";
            this.stripEDIDsToolStripMenuItem.Click += new System.EventHandler(this.stripEDIDsToolStripMenuItem_Click);
            // 
            // findDuplicatedFormIDToolStripMenuItem
            // 
            this.findDuplicatedFormIDToolStripMenuItem.Name = "findDuplicatedFormIDToolStripMenuItem";
            this.findDuplicatedFormIDToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.findDuplicatedFormIDToolStripMenuItem.Text = "Find duplicated FormID";
            this.findDuplicatedFormIDToolStripMenuItem.Click += new System.EventHandler(this.findDuplicatedFormIDToolStripMenuItem_Click);
            // 
            // dumpEDIDListToolStripMenuItem
            // 
            this.dumpEDIDListToolStripMenuItem.Name = "dumpEDIDListToolStripMenuItem";
            this.dumpEDIDListToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.dumpEDIDListToolStripMenuItem.Text = "Dump EDID list";
            this.dumpEDIDListToolStripMenuItem.Click += new System.EventHandler(this.dumpEDIDListToolStripMenuItem_Click);
            // 
            // cleanEspToolStripMenuItem
            // 
            this.cleanEspToolStripMenuItem.Name = "cleanEspToolStripMenuItem";
            this.cleanEspToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.cleanEspToolStripMenuItem.Text = "Clean esp";
            this.cleanEspToolStripMenuItem.Click += new System.EventHandler(this.cleanEspToolStripMenuItem_Click);
            // 
            // findNonconformingRecordToolStripMenuItem
            // 
            this.findNonconformingRecordToolStripMenuItem.Name = "findNonconformingRecordToolStripMenuItem";
            this.findNonconformingRecordToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.findNonconformingRecordToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.findNonconformingRecordToolStripMenuItem.Text = "Find &nonconforming record";
            this.findNonconformingRecordToolStripMenuItem.Click += new System.EventHandler(this.findNonconformingRecordToolStripMenuItem_Click);
            // 
            // compileScriptToolStripMenuItem
            // 
            this.compileScriptToolStripMenuItem.Name = "compileScriptToolStripMenuItem";
            this.compileScriptToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.compileScriptToolStripMenuItem.Text = "Compile script";
            this.compileScriptToolStripMenuItem.Click += new System.EventHandler(this.compileScriptToolStripMenuItem_Click);
            // 
            // compileAllToolStripMenuItem
            // 
            this.compileAllToolStripMenuItem.Name = "compileAllToolStripMenuItem";
            this.compileAllToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.compileAllToolStripMenuItem.Text = "Compile all";
            this.compileAllToolStripMenuItem.Click += new System.EventHandler(this.compileAllToolStripMenuItem_Click);
            // 
            // generateLLXmlToolStripMenuItem
            // 
            this.generateLLXmlToolStripMenuItem.Name = "generateLLXmlToolStripMenuItem";
            this.generateLLXmlToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.generateLLXmlToolStripMenuItem.Text = "Generate LL xml";
            this.generateLLXmlToolStripMenuItem.Click += new System.EventHandler(this.generateLLXmlToolStripMenuItem_Click);
            // 
            // makeEsmToolStripMenuItem
            // 
            this.makeEsmToolStripMenuItem.Name = "makeEsmToolStripMenuItem";
            this.makeEsmToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.makeEsmToolStripMenuItem.Text = "Make esm";
            this.makeEsmToolStripMenuItem.Click += new System.EventHandler(this.makeEsmToolStripMenuItem_Click);
            // 
            // martigensToolStripMenuItem
            // 
            this.martigensToolStripMenuItem.Name = "martigensToolStripMenuItem";
            this.martigensToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.martigensToolStripMenuItem.Text = "SCTX replacer";
            this.martigensToolStripMenuItem.Click += new System.EventHandler(this.martigensToolStripMenuItem_Click);
            // 
            // createRecordStructureXmlToolStripMenuItem
            // 
            this.createRecordStructureXmlToolStripMenuItem.Name = "createRecordStructureXmlToolStripMenuItem";
            this.createRecordStructureXmlToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.createRecordStructureXmlToolStripMenuItem.Text = "Guess Record Structure ...";
            this.createRecordStructureXmlToolStripMenuItem.Visible = false;
            this.createRecordStructureXmlToolStripMenuItem.Click += new System.EventHandler(this.createRecordStructureXmlToolStripMenuItem_Click);
            // 
            // mergeRecordsXMLToolStripMenuItem
            // 
            this.mergeRecordsXMLToolStripMenuItem.Name = "mergeRecordsXMLToolStripMenuItem";
            this.mergeRecordsXMLToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.mergeRecordsXMLToolStripMenuItem.Text = "Merge Records XML ...";
            this.mergeRecordsXMLToolStripMenuItem.Visible = false;
            this.mergeRecordsXMLToolStripMenuItem.Click += new System.EventHandler(this.mergeRecordsXMLToolStripMenuItem_Click);
            // 
            // reorderSubrecordsToolStripMenuItem
            // 
            this.reorderSubrecordsToolStripMenuItem.Name = "reorderSubrecordsToolStripMenuItem";
            this.reorderSubrecordsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.reorderSubrecordsToolStripMenuItem.Text = "Reorder Subrecords";
            this.reorderSubrecordsToolStripMenuItem.ToolTipText = "Attempt to restucture the Current Subrecord to match XML";
            this.reorderSubrecordsToolStripMenuItem.Click += new System.EventHandler(this.reorderSubrecordsToolStripMenuItem_Click);
            // 
            // editStringsToolStripMenuItem
            // 
            this.editStringsToolStripMenuItem.Name = "editStringsToolStripMenuItem";
            this.editStringsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.editStringsToolStripMenuItem.Text = "Edit &Strings";
            this.editStringsToolStripMenuItem.Click += new System.EventHandler(this.editStringsToolStripMenuItem_Click);
            // 
            // OpenModDialog
            // 
            this.OpenModDialog.Filter = "Skyrim plugin (*.esm, *.esp)|*.esm;*.esp";
            this.OpenModDialog.Multiselect = true;
            this.OpenModDialog.RestoreDirectory = true;
            this.OpenModDialog.Title = "Select plugin(s) to open";
            // 
            // SaveModDialog
            // 
            this.SaveModDialog.DefaultExt = "esp";
            this.SaveModDialog.Filter = "Skyrim plugin (*.esp)|*.esp|Master file|*.esm";
            this.SaveModDialog.RestoreDirectory = true;
            this.SaveModDialog.Title = "Select path to save to";
            // 
            // SaveEdidListDialog
            // 
            this.SaveEdidListDialog.DefaultExt = "txt";
            this.SaveEdidListDialog.Filter = "Text file (*.txt)|*.txt";
            this.SaveEdidListDialog.RestoreDirectory = true;
            this.SaveEdidListDialog.Title = "Save file as";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusProgressBar,
            this.toolStripStopProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 636);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(667, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(652, 17);
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusProgressBar
            // 
            this.toolStripStatusProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripStatusProgressBar.Name = "toolStripStatusProgressBar";
            this.toolStripStatusProgressBar.Size = new System.Drawing.Size(200, 16);
            this.toolStripStatusProgressBar.Visible = false;
            // 
            // toolStripStopProgress
            // 
            this.toolStripStopProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripStopProgress.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStopProgress.Image = global::TESVSnip.Properties.Resources.agt_stop;
            this.toolStripStopProgress.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStopProgress.Name = "toolStripStopProgress";
            this.toolStripStopProgress.Size = new System.Drawing.Size(16, 17);
            this.toolStripStopProgress.Text = "toolStripSplitButton1";
            this.toolStripStopProgress.Visible = false;
            this.toolStripStopProgress.Click += new System.EventHandler(this.toolStripStopProgress_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // splitHorizontal
            // 
            this.splitHorizontal.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::TESVSnip.Properties.Settings.Default, "MainHorzSplitterPct", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.splitHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitHorizontal.Location = new System.Drawing.Point(0, 24);
            this.splitHorizontal.MinimumSize = new System.Drawing.Size(100, 200);
            this.splitHorizontal.Name = "splitHorizontal";
            // 
            // splitHorizontal.Panel1
            // 
            this.splitHorizontal.Panel1.Controls.Add(this.splitVertical);
            this.splitHorizontal.Panel1MinSize = 100;
            // 
            // splitHorizontal.Panel2
            // 
            this.splitHorizontal.Panel2.Controls.Add(this.tbInfo);
            this.splitHorizontal.Size = new System.Drawing.Size(667, 612);
            this.splitHorizontal.SplitterDistance = global::TESVSnip.Properties.Settings.Default.MainHorzSplitterPct;
            this.splitHorizontal.TabIndex = 3;
            // 
            // splitVertical
            // 
            this.splitVertical.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::TESVSnip.Properties.Settings.Default, "MainVertSplitterPct", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.splitVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitVertical.Location = new System.Drawing.Point(0, 0);
            this.splitVertical.MinimumSize = new System.Drawing.Size(100, 100);
            this.splitVertical.Name = "splitVertical";
            this.splitVertical.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitVertical.Panel1
            // 
            this.splitVertical.Panel1.Controls.Add(this.PluginTree);
            this.splitVertical.Panel1MinSize = 100;
            // 
            // splitVertical.Panel2
            // 
            this.splitVertical.Panel2.Controls.Add(this.listSubrecord);
            this.splitVertical.Panel2.Controls.Add(this.toolStripSubRecord);
            this.splitVertical.Size = new System.Drawing.Size(226, 612);
            this.splitVertical.SplitterDistance = global::TESVSnip.Properties.Settings.Default.MainVertSplitterPct;
            this.splitVertical.TabIndex = 1;
            // 
            // PluginTree
            // 
            this.PluginTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PluginTree.HideSelection = false;
            this.PluginTree.Location = new System.Drawing.Point(0, 0);
            this.PluginTree.Name = "PluginTree";
            this.PluginTree.Size = new System.Drawing.Size(226, 222);
            this.PluginTree.TabIndex = 0;
            this.PluginTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.PluginTree_AfterExpand);
            this.PluginTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.PluginTree_AfterSelect);
            this.PluginTree.Enter += new System.EventHandler(this.PluginTree_Enter);
            this.PluginTree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PluginTree_MouseDoubleClick);
            // 
            // listSubrecord
            // 
            this.listSubrecord.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listSubrecord.AllowDrop = true;
            this.listSubrecord.AutoScroll = false;
            this.listSubrecord.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listSubrecord.DataSource = null;
            this.listSubrecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listSubrecord.FullRowSelect = true;
            this.listSubrecord.GridLines = true;
            this.listSubrecord.HideSelection = false;
            this.listSubrecord.HoverSelection = true;
            this.listSubrecord.ItemCount = 0;
            this.listSubrecord.Location = new System.Drawing.Point(0, 25);
            this.listSubrecord.Name = "listSubrecord";
            this.listSubrecord.ShowItemToolTips = true;
            this.listSubrecord.Size = new System.Drawing.Size(226, 361);
            this.listSubrecord.TabIndex = 0;
            this.listSubrecord.UseCompatibleStateImageBehavior = false;
            this.listSubrecord.View = System.Windows.Forms.View.Details;
            this.listSubrecord.VirtualMode = true;
            this.listSubrecord.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            this.listSubrecord.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView1_ItemDrag);
            this.listSubrecord.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listSubrecord.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView1_DragDrop);
            this.listSubrecord.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView1_DragEnter);
            this.listSubrecord.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.listView1_GiveFeedback);
            this.listSubrecord.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSubrecord_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            // 
            // toolStripSubRecord
            // 
            this.toolStripSubRecord.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripSubRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripInsertRecord,
            this.toolStripDeleteRecord,
            this.toolStripSeparator1,
            this.toolStripMoveRecordUp,
            this.toolStripMoveRecordDown,
            this.toolStripButton1,
            this.toolStripEditSubrecord,
            this.toolStripEditSubrecordHex,
            this.toolStripPasteSubrecord,
            this.toolStripCopySubrecord,
            this.toolStripSeparator2});
            this.toolStripSubRecord.Location = new System.Drawing.Point(0, 0);
            this.toolStripSubRecord.Name = "toolStripSubRecord";
            this.toolStripSubRecord.Size = new System.Drawing.Size(226, 25);
            this.toolStripSubRecord.TabIndex = 1;
            // 
            // toolStripInsertRecord
            // 
            this.toolStripInsertRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripInsertRecord.Image = global::TESVSnip.Properties.Resources.insertcell;
            this.toolStripInsertRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripInsertRecord.Name = "toolStripInsertRecord";
            this.toolStripInsertRecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripInsertRecord.Text = "Insert Record";
            this.toolStripInsertRecord.Click += new System.EventHandler(this.toolStripInsertRecord_Click);
            // 
            // toolStripDeleteRecord
            // 
            this.toolStripDeleteRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDeleteRecord.Image = global::TESVSnip.Properties.Resources.deletecell;
            this.toolStripDeleteRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDeleteRecord.Name = "toolStripDeleteRecord";
            this.toolStripDeleteRecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripDeleteRecord.Text = "Delete Record";
            this.toolStripDeleteRecord.Click += new System.EventHandler(this.toolStripDeleteRecord_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripMoveRecordUp
            // 
            this.toolStripMoveRecordUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMoveRecordUp.Image = global::TESVSnip.Properties.Resources.move_task_up;
            this.toolStripMoveRecordUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripMoveRecordUp.Name = "toolStripMoveRecordUp";
            this.toolStripMoveRecordUp.Size = new System.Drawing.Size(23, 22);
            this.toolStripMoveRecordUp.Text = "Move Record Up";
            this.toolStripMoveRecordUp.Click += new System.EventHandler(this.toolStripMoveRecordUp_Click);
            // 
            // toolStripMoveRecordDown
            // 
            this.toolStripMoveRecordDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMoveRecordDown.Image = global::TESVSnip.Properties.Resources.move_task_down;
            this.toolStripMoveRecordDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripMoveRecordDown.Name = "toolStripMoveRecordDown";
            this.toolStripMoveRecordDown.Size = new System.Drawing.Size(23, 22);
            this.toolStripMoveRecordDown.Text = "Move Record Down";
            this.toolStripMoveRecordDown.Click += new System.EventHandler(this.toolStripMoveRecordDown_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripEditSubrecord
            // 
            this.toolStripEditSubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripEditSubrecord.Image = global::TESVSnip.Properties.Resources.editclear;
            this.toolStripEditSubrecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripEditSubrecord.Name = "toolStripEditSubrecord";
            this.toolStripEditSubrecord.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.toolStripEditSubrecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripEditSubrecord.Text = "Edit Subrecord";
            this.toolStripEditSubrecord.Click += new System.EventHandler(this.toolStripEditSubrecord_Click);
            // 
            // toolStripEditSubrecordHex
            // 
            this.toolStripEditSubrecordHex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripEditSubrecordHex.Image = global::TESVSnip.Properties.Resources.xdays;
            this.toolStripEditSubrecordHex.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripEditSubrecordHex.Name = "toolStripEditSubrecordHex";
            this.toolStripEditSubrecordHex.Size = new System.Drawing.Size(23, 22);
            this.toolStripEditSubrecordHex.Text = "Hex Edit";
            this.toolStripEditSubrecordHex.Click += new System.EventHandler(this.toolStripEditSubrecordHex_Click);
            // 
            // toolStripPasteSubrecord
            // 
            this.toolStripPasteSubrecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripPasteSubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripPasteSubrecord.Image = global::TESVSnip.Properties.Resources.Paste;
            this.toolStripPasteSubrecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripPasteSubrecord.Name = "toolStripPasteSubrecord";
            this.toolStripPasteSubrecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripPasteSubrecord.Text = "Paste";
            this.toolStripPasteSubrecord.Click += new System.EventHandler(this.toolStripPasteSubrecord_Click);
            // 
            // toolStripCopySubrecord
            // 
            this.toolStripCopySubrecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripCopySubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripCopySubrecord.Image = global::TESVSnip.Properties.Resources.Copy;
            this.toolStripCopySubrecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripCopySubrecord.Name = "toolStripCopySubrecord";
            this.toolStripCopySubrecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripCopySubrecord.Text = "Copy Element";
            this.toolStripCopySubrecord.Click += new System.EventHandler(this.toolStripCopySubrecord_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tbInfo
            // 
            this.tbInfo.BackColor = System.Drawing.SystemColors.Window;
            this.tbInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbInfo.Location = new System.Drawing.Point(0, 0);
            this.tbInfo.Multiline = true;
            this.tbInfo.Name = "tbInfo";
            this.tbInfo.ReadOnly = true;
            this.tbInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbInfo.ShortcutsEnabled = false;
            this.tbInfo.Size = new System.Drawing.Size(437, 612);
            this.tbInfo.TabIndex = 2;
            this.tbInfo.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tbInfo_PreviewKeyDown);
            // 
            // toolStripIncrFind
            // 
            this.toolStripIncrFind.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStripIncrFind.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripIncrFind.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripIncrFindCancel,
            this.toolStripIncrFindText,
            this.toolStripIncrFindNext,
            this.toolStripIncrFindPrev,
            this.toolStripIncrFindRestart,
            this.toolStripIncrFindType,
            this.toolStripIncrFindMatch,
            this.toolStripIncrFindExact,
            this.toolStripIncrFindDown});
            this.toolStripIncrFind.Location = new System.Drawing.Point(0, 438);
            this.toolStripIncrFind.Name = "toolStripIncrFind";
            this.toolStripIncrFind.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripIncrFind.Size = new System.Drawing.Size(667, 25);
            this.toolStripIncrFind.TabIndex = 5;
            this.toolStripIncrFind.Text = "Incremental Find";
            this.toolStripIncrFind.Visible = false;
            this.toolStripIncrFind.VisibleChanged += new System.EventHandler(this.toolStripIncrFind_VisibleChanged);
            // 
            // toolStripIncrFindCancel
            // 
            this.toolStripIncrFindCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripIncrFindCancel.Image = global::TESVSnip.Properties.Resources.delete;
            this.toolStripIncrFindCancel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripIncrFindCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindCancel.Name = "toolStripIncrFindCancel";
            this.toolStripIncrFindCancel.Size = new System.Drawing.Size(23, 22);
            this.toolStripIncrFindCancel.Text = "toolStripButton2";
            this.toolStripIncrFindCancel.Click += new System.EventHandler(this.toolStripIncrFindCancel_Click);
            // 
            // toolStripIncrFindText
            // 
            this.toolStripIncrFindText.AcceptsReturn = true;
            this.toolStripIncrFindText.Name = "toolStripIncrFindText";
            this.toolStripIncrFindText.Size = new System.Drawing.Size(100, 25);
            this.toolStripIncrFindText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripIncrFindText_KeyDown);
            this.toolStripIncrFindText.TextChanged += new System.EventHandler(this.toolStripIncrFindText_TextChanged);
            // 
            // toolStripIncrFindNext
            // 
            this.toolStripIncrFindNext.Image = global::TESVSnip.Properties.Resources.down;
            this.toolStripIncrFindNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindNext.Name = "toolStripIncrFindNext";
            this.toolStripIncrFindNext.Size = new System.Drawing.Size(51, 22);
            this.toolStripIncrFindNext.Text = "&Next";
            this.toolStripIncrFindNext.Click += new System.EventHandler(this.toolStripIncrFindNext_Click);
            // 
            // toolStripIncrFindPrev
            // 
            this.toolStripIncrFindPrev.Image = global::TESVSnip.Properties.Resources.up;
            this.toolStripIncrFindPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindPrev.Name = "toolStripIncrFindPrev";
            this.toolStripIncrFindPrev.Size = new System.Drawing.Size(50, 22);
            this.toolStripIncrFindPrev.Text = "&Prev";
            this.toolStripIncrFindPrev.Click += new System.EventHandler(this.toolStripIncrFindPrev_Click);
            // 
            // toolStripIncrFindRestart
            // 
            this.toolStripIncrFindRestart.Image = global::TESVSnip.Properties.Resources.quick_restart;
            this.toolStripIncrFindRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindRestart.Name = "toolStripIncrFindRestart";
            this.toolStripIncrFindRestart.Size = new System.Drawing.Size(63, 22);
            this.toolStripIncrFindRestart.Text = "&Restart";
            this.toolStripIncrFindRestart.Click += new System.EventHandler(this.toolStripIncrFindRestart_Click);
            // 
            // toolStripIncrFindType
            // 
            this.toolStripIncrFindType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripIncrFindType.Items.AddRange(new object[] {
            "Editor ID",
            "Form ID"});
            this.toolStripIncrFindType.Name = "toolStripIncrFindType";
            this.toolStripIncrFindType.Size = new System.Drawing.Size(75, 25);
            this.toolStripIncrFindType.ToolTipText = "Search Type";
            // 
            // toolStripIncrFindMatch
            // 
            this.toolStripIncrFindMatch.CheckOnClick = true;
            this.toolStripIncrFindMatch.Image = global::TESVSnip.Properties.Resources.emptybox;
            this.toolStripIncrFindMatch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripIncrFindMatch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindMatch.Name = "toolStripIncrFindMatch";
            this.toolStripIncrFindMatch.Size = new System.Drawing.Size(93, 22);
            this.toolStripIncrFindMatch.Text = "Match Case";
            this.toolStripIncrFindMatch.ToolTipText = "Match Case";
            this.toolStripIncrFindMatch.Visible = false;
            this.toolStripIncrFindMatch.CheckStateChanged += new System.EventHandler(this.toolStripCheck_CheckStateChanged);
            // 
            // toolStripIncrFindExact
            // 
            this.toolStripIncrFindExact.CheckOnClick = true;
            this.toolStripIncrFindExact.Image = global::TESVSnip.Properties.Resources.emptybox;
            this.toolStripIncrFindExact.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripIncrFindExact.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindExact.Name = "toolStripIncrFindExact";
            this.toolStripIncrFindExact.Size = new System.Drawing.Size(58, 22);
            this.toolStripIncrFindExact.Text = "Exact";
            this.toolStripIncrFindExact.ToolTipText = "Match Case";
            this.toolStripIncrFindExact.CheckStateChanged += new System.EventHandler(this.toolStripCheck_CheckStateChanged);
            // 
            // toolStripIncrFindDown
            // 
            this.toolStripIncrFindDown.CheckOnClick = true;
            this.toolStripIncrFindDown.Image = global::TESVSnip.Properties.Resources.emptybox;
            this.toolStripIncrFindDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripIncrFindDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrFindDown.Name = "toolStripIncrFindDown";
            this.toolStripIncrFindDown.Size = new System.Drawing.Size(90, 22);
            this.toolStripIncrFindDown.Text = "Down Only";
            this.toolStripIncrFindDown.ToolTipText = "Down Only";
            this.toolStripIncrFindDown.CheckStateChanged += new System.EventHandler(this.toolStripCheck_CheckStateChanged);
            // 
            // toolStripIncrInvalidRec
            // 
            this.toolStripIncrInvalidRec.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStripIncrInvalidRec.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripIncrInvalidRec.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripIncrInvalidRecCancel,
            this.toolStripIncrInvalidRecText,
            this.toolStripIncrInvalidRecNext,
            this.toolStripIncrInvalidRecPrev,
            this.toolStripIncrInvalidRecRestart,
            this.toolStripIncrInvalidRecDown});
            this.toolStripIncrInvalidRec.Location = new System.Drawing.Point(0, 438);
            this.toolStripIncrInvalidRec.Name = "toolStripIncrInvalidRec";
            this.toolStripIncrInvalidRec.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripIncrInvalidRec.Size = new System.Drawing.Size(667, 25);
            this.toolStripIncrInvalidRec.TabIndex = 6;
            this.toolStripIncrInvalidRec.Text = "Incremental Invalid Record Search";
            this.toolStripIncrInvalidRec.Visible = false;
            this.toolStripIncrInvalidRec.VisibleChanged += new System.EventHandler(this.toolStripIncrInvalidRec_VisibleChanged);
            // 
            // toolStripIncrInvalidRecCancel
            // 
            this.toolStripIncrInvalidRecCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripIncrInvalidRecCancel.Image = global::TESVSnip.Properties.Resources.delete;
            this.toolStripIncrInvalidRecCancel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripIncrInvalidRecCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrInvalidRecCancel.Name = "toolStripIncrInvalidRecCancel";
            this.toolStripIncrInvalidRecCancel.Size = new System.Drawing.Size(23, 22);
            this.toolStripIncrInvalidRecCancel.Text = "toolStripButton2";
            this.toolStripIncrInvalidRecCancel.Click += new System.EventHandler(this.toolStripIncrInvalidRecCancel_Click);
            // 
            // toolStripIncrInvalidRecText
            // 
            this.toolStripIncrInvalidRecText.Name = "toolStripIncrInvalidRecText";
            this.toolStripIncrInvalidRecText.Size = new System.Drawing.Size(107, 22);
            this.toolStripIncrInvalidRecText.Text = "Invalid Item Search";
            // 
            // toolStripIncrInvalidRecNext
            // 
            this.toolStripIncrInvalidRecNext.Image = global::TESVSnip.Properties.Resources.down;
            this.toolStripIncrInvalidRecNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrInvalidRecNext.Name = "toolStripIncrInvalidRecNext";
            this.toolStripIncrInvalidRecNext.Size = new System.Drawing.Size(51, 22);
            this.toolStripIncrInvalidRecNext.Text = "&Next";
            this.toolStripIncrInvalidRecNext.Click += new System.EventHandler(this.toolStripIncrInvalidRecNext_Click);
            // 
            // toolStripIncrInvalidRecPrev
            // 
            this.toolStripIncrInvalidRecPrev.Image = global::TESVSnip.Properties.Resources.up;
            this.toolStripIncrInvalidRecPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrInvalidRecPrev.Name = "toolStripIncrInvalidRecPrev";
            this.toolStripIncrInvalidRecPrev.Size = new System.Drawing.Size(50, 22);
            this.toolStripIncrInvalidRecPrev.Text = "&Prev";
            this.toolStripIncrInvalidRecPrev.Click += new System.EventHandler(this.toolStripIncrInvalidRecPrev_Click);
            // 
            // toolStripIncrInvalidRecRestart
            // 
            this.toolStripIncrInvalidRecRestart.Image = global::TESVSnip.Properties.Resources.quick_restart;
            this.toolStripIncrInvalidRecRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrInvalidRecRestart.Name = "toolStripIncrInvalidRecRestart";
            this.toolStripIncrInvalidRecRestart.Size = new System.Drawing.Size(63, 22);
            this.toolStripIncrInvalidRecRestart.Text = "&Restart";
            this.toolStripIncrInvalidRecRestart.Click += new System.EventHandler(this.toolStripIncrInvalidRecRestart_Click);
            // 
            // toolStripIncrInvalidRecDown
            // 
            this.toolStripIncrInvalidRecDown.CheckOnClick = true;
            this.toolStripIncrInvalidRecDown.Image = global::TESVSnip.Properties.Resources.emptybox;
            this.toolStripIncrInvalidRecDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripIncrInvalidRecDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripIncrInvalidRecDown.Name = "toolStripIncrInvalidRecDown";
            this.toolStripIncrInvalidRecDown.Size = new System.Drawing.Size(90, 22);
            this.toolStripIncrInvalidRecDown.Text = "Down Only";
            this.toolStripIncrInvalidRecDown.ToolTipText = "Match Case";
            this.toolStripIncrInvalidRecDown.CheckStateChanged += new System.EventHandler(this.toolStripCheck_CheckStateChanged);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 658);
            this.Controls.Add(this.splitHorizontal);
            this.Controls.Add(this.toolStripIncrFind);
            this.Controls.Add(this.toolStripIncrInvalidRec);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(380, 300);
            this.Name = "MainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TESsnip (Skyrim edition)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TESsnip_FormClosing);
            this.Load += new System.EventHandler(this.MainView_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitHorizontal.Panel1.ResumeLayout(false);
            this.splitHorizontal.Panel2.ResumeLayout(false);
            this.splitHorizontal.Panel2.PerformLayout();
            this.splitHorizontal.ResumeLayout(false);
            this.splitVertical.Panel1.ResumeLayout(false);
            this.splitVertical.Panel2.ResumeLayout(false);
            this.splitVertical.Panel2.PerformLayout();
            this.splitVertical.ResumeLayout(false);
            this.toolStripSubRecord.ResumeLayout(false);
            this.toolStripSubRecord.PerformLayout();
            this.toolStripIncrFind.ResumeLayout(false);
            this.toolStripIncrFind.PerformLayout();
            this.toolStripIncrInvalidRec.ResumeLayout(false);
            this.toolStripIncrInvalidRec.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView PluginTree;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openNewPluginToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog OpenModDialog;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.TextBox tbInfo;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hexModeToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog SaveModDialog;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitHorizontal;
        private System.Windows.Forms.SplitContainer splitVertical;
        private TESVSnip.Windows.Controls.BindingListView listSubrecord;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripMenuItem insertRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertSubrecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spellsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sanitizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stripEDIDsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findDuplicatedFormIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useNewSubrecordEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpEDIDListToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog SaveEdidListDialog;
        private System.Windows.Forms.ToolStripMenuItem reloadXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lookupFormidsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanEspToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNonconformingRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compileScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compileAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateLLXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeEsmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem martigensToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editStringsToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripSubRecord;
        private System.Windows.Forms.ToolStripButton toolStripInsertRecord;
        private System.Windows.Forms.ToolStripButton toolStripDeleteRecord;
        private System.Windows.Forms.ToolStripButton toolStripMoveRecordUp;
        private System.Windows.Forms.ToolStripButton toolStripMoveRecordDown;
        private System.Windows.Forms.ToolStripButton toolStripEditSubrecord;
        private System.Windows.Forms.ToolStripButton toolStripEditSubrecordHex;
        private System.Windows.Forms.ToolStripMenuItem reorderSubrecordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripPasteSubrecord;
        private System.Windows.Forms.ToolStripButton toolStripCopySubrecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem createRecordStructureXmlToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripStatusProgressBar;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ToolStripMenuItem mergeRecordsXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eSMFilterSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStopProgress;
        private System.Windows.Forms.ToolStrip toolStripIncrFind;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindCancel;
        private System.Windows.Forms.ToolStripTextBox toolStripIncrFindText;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindNext;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindPrev;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindMatch;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindExact;
        private System.Windows.Forms.ToolStripComboBox toolStripIncrFindType;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindRestart;
        private System.Windows.Forms.ToolStrip toolStripIncrInvalidRec;
        private System.Windows.Forms.ToolStripButton toolStripIncrInvalidRecCancel;
        private System.Windows.Forms.ToolStripLabel toolStripIncrInvalidRecText;
        private System.Windows.Forms.ToolStripButton toolStripIncrInvalidRecNext;
        private System.Windows.Forms.ToolStripButton toolStripIncrInvalidRecPrev;
        private System.Windows.Forms.ToolStripButton toolStripIncrInvalidRecRestart;
        private System.Windows.Forms.ToolStripButton toolStripIncrInvalidRecDown;
        private System.Windows.Forms.ToolStripButton toolStripIncrFindDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}