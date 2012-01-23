namespace TESVSnip.ObjectControls
{
    partial class PluginTreeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginTreeView));
            this.imageSmallTreeList = new System.Windows.Forms.ImageList(this.components);
            this.toolStripRecord = new System.Windows.Forms.ToolStrip();
            this.toolStripRecordBack = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripRecordNext = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripRecordPaste = new System.Windows.Forms.ToolStripButton();
            this.toolStripRecordCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripRecordText = new System.Windows.Forms.ToolStripLabel();
            this.contextMenuRecord = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuRecordAddMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuRecordCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuRecordCopyTo = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuRecordDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.PluginTree = new TESVSnip.Controls.CustomTreeView();
            this.toolStripRecord.SuspendLayout();
            this.contextMenuRecord.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PluginTree)).BeginInit();
            this.SuspendLayout();
            // 
            // imageSmallTreeList
            // 
            this.imageSmallTreeList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.imageSmallTreeList, "imageSmallTreeList");
            this.imageSmallTreeList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStripRecord
            // 
            resources.ApplyResources(this.toolStripRecord, "toolStripRecord");
            this.toolStripRecord.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripRecordBack,
            this.toolStripRecordNext,
            this.toolStripRecordPaste,
            this.toolStripRecordCopy,
            this.toolStripRecordText});
            this.toolStripRecord.Name = "toolStripRecord";
            // 
            // toolStripRecordBack
            // 
            resources.ApplyResources(this.toolStripRecordBack, "toolStripRecordBack");
            this.toolStripRecordBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRecordBack.Image = global::TESVSnip.Properties.Resources.agt_back;
            this.toolStripRecordBack.Name = "toolStripRecordBack";
            // 
            // toolStripRecordNext
            // 
            resources.ApplyResources(this.toolStripRecordNext, "toolStripRecordNext");
            this.toolStripRecordNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRecordNext.Image = global::TESVSnip.Properties.Resources.agt_forward;
            this.toolStripRecordNext.Name = "toolStripRecordNext";
            // 
            // toolStripRecordPaste
            // 
            resources.ApplyResources(this.toolStripRecordPaste, "toolStripRecordPaste");
            this.toolStripRecordPaste.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripRecordPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRecordPaste.Image = global::TESVSnip.Properties.Resources.Paste;
            this.toolStripRecordPaste.Name = "toolStripRecordPaste";
            this.toolStripRecordPaste.Click += new System.EventHandler(this.toolStripRecordPaste_Click);
            // 
            // toolStripRecordCopy
            // 
            resources.ApplyResources(this.toolStripRecordCopy, "toolStripRecordCopy");
            this.toolStripRecordCopy.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripRecordCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRecordCopy.Image = global::TESVSnip.Properties.Resources.Copy;
            this.toolStripRecordCopy.Name = "toolStripRecordCopy";
            this.toolStripRecordCopy.Click += new System.EventHandler(this.toolStripRecordCopy_Click);
            // 
            // toolStripRecordText
            // 
            resources.ApplyResources(this.toolStripRecordText, "toolStripRecordText");
            this.toolStripRecordText.Name = "toolStripRecordText";
            // 
            // contextMenuRecord
            // 
            resources.ApplyResources(this.contextMenuRecord, "contextMenuRecord");
            this.contextMenuRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuRecordAddMaster,
            this.contextMenuRecordCopy,
            this.contextMenuRecordCopyTo,
            this.contextMenuRecordDelete,
            this.toolStripMenuItem2});
            this.contextMenuRecord.Name = "contextMenuRecord";
            this.contextMenuRecord.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.contextMenuRecord_Closing);
            this.contextMenuRecord.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuRecord_Opening);
            // 
            // contextMenuRecordAddMaster
            // 
            resources.ApplyResources(this.contextMenuRecordAddMaster, "contextMenuRecordAddMaster");
            this.contextMenuRecordAddMaster.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseToolStripMenuItem});
            this.contextMenuRecordAddMaster.Name = "contextMenuRecordAddMaster";
            this.contextMenuRecordAddMaster.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuRecordAddMaster_DropDownItemClicked);
            // 
            // browseToolStripMenuItem
            // 
            resources.ApplyResources(this.browseToolStripMenuItem, "browseToolStripMenuItem");
            this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
            this.browseToolStripMenuItem.Click += new System.EventHandler(this.addMasterToolStripMenuItem_Click);
            // 
            // contextMenuRecordCopy
            // 
            resources.ApplyResources(this.contextMenuRecordCopy, "contextMenuRecordCopy");
            this.contextMenuRecordCopy.Name = "contextMenuRecordCopy";
            this.contextMenuRecordCopy.Click += new System.EventHandler(this.contexMenuRecordCopy_Click);
            // 
            // contextMenuRecordCopyTo
            // 
            resources.ApplyResources(this.contextMenuRecordCopyTo, "contextMenuRecordCopyTo");
            this.contextMenuRecordCopyTo.Name = "contextMenuRecordCopyTo";
            this.contextMenuRecordCopyTo.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuRecordCopyTo_DropDownItemClicked);
            // 
            // contextMenuRecordDelete
            // 
            resources.ApplyResources(this.contextMenuRecordDelete, "contextMenuRecordDelete");
            this.contextMenuRecordDelete.Name = "contextMenuRecordDelete";
            this.contextMenuRecordDelete.Click += new System.EventHandler(this.contextMenuRecordDelete_Click);
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // toolStripMenuItem3
            // 
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.expandBranchToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.collapseBranchToolStripMenuItem_Click);
            // 
            // PluginTree
            // 
            resources.ApplyResources(this.PluginTree, "PluginTree");
            this.PluginTree.AllowColumnReorder = true;
            this.PluginTree.AllowDrop = true;
            this.PluginTree.CheckBoxes = false;
            this.PluginTree.ContextMenuStrip = this.contextMenuRecord;
            this.PluginTree.EmptyListMsgFont = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PluginTree.EnableSearchByKeyboard = true;
            this.PluginTree.GridLines = true;
            this.PluginTree.HideSelection = false;
            this.PluginTree.IsSimpleDragSource = true;
            this.PluginTree.IsSimpleDropSink = true;
            this.PluginTree.Name = "PluginTree";
            this.PluginTree.OverlayText.Text = resources.GetString("resource.Text");
            this.PluginTree.OwnerDraw = true;
            this.PluginTree.SelectAllOnControlA = false;
            this.PluginTree.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
            this.PluginTree.SelectedRecord = null;
            this.PluginTree.ShowCommandMenuOnRightClick = true;
            this.PluginTree.ShowFilterMenuOnRightClick = false;
            this.PluginTree.ShowGroups = false;
            this.PluginTree.ShowImagesOnSubItems = true;
            this.PluginTree.ShowItemToolTips = true;
            this.PluginTree.SmallImageList = this.imageSmallTreeList;
            this.PluginTree.UseCompatibleStateImageBehavior = false;
            this.PluginTree.UseFiltering = true;
            this.PluginTree.UseHotItem = true;
            this.PluginTree.View = System.Windows.Forms.View.Details;
            this.PluginTree.VirtualMode = true;
            this.PluginTree.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.PluginTree_ModelCanDrop);
            this.PluginTree.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.PluginTree_ModelDropped);
            this.PluginTree.SelectionChanged += new System.EventHandler(this.PluginTree_SelectionChanged);
            this.PluginTree.SelectedIndexChanged += new System.EventHandler(this.PluginTree_SelectedIndexChanged);
            this.PluginTree.SizeChanged += new System.EventHandler(this.PluginTree_SizeChanged);
            this.PluginTree.Enter += new System.EventHandler(this.PluginTree_Enter);
            this.PluginTree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PluginTree_MouseDoubleClick);
            // 
            // PluginTreeView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PluginTree);
            this.Controls.Add(this.toolStripRecord);
            this.Name = "PluginTreeView";
            this.toolStripRecord.ResumeLayout(false);
            this.toolStripRecord.PerformLayout();
            this.contextMenuRecord.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PluginTree)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TESVSnip.Controls.CustomTreeView PluginTree;
        private System.Windows.Forms.ToolStrip toolStripRecord;
        private System.Windows.Forms.ToolStripSplitButton toolStripRecordBack;
        private System.Windows.Forms.ToolStripSplitButton toolStripRecordNext;
        private System.Windows.Forms.ToolStripButton toolStripRecordPaste;
        private System.Windows.Forms.ToolStripButton toolStripRecordCopy;
        private System.Windows.Forms.ToolStripLabel toolStripRecordText;
        private System.Windows.Forms.ContextMenuStrip contextMenuRecord;
        private System.Windows.Forms.ToolStripMenuItem contextMenuRecordAddMaster;
        private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextMenuRecordCopy;
        private System.Windows.Forms.ToolStripMenuItem contextMenuRecordCopyTo;
        private System.Windows.Forms.ToolStripMenuItem contextMenuRecordDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ImageList imageSmallTreeList;
    }
}
