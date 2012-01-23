namespace TESVSnip.Forms
{
    partial class SubrecordListEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubrecordListEditor));
            this.subrecordPanel = new System.Windows.Forms.Panel();
            this.listSubrecord = new TESVSnip.Windows.Controls.ObjectBindingListView();
            this.toolStripSubRecord = new System.Windows.Forms.ToolStrip();
            this.toolStripInsertRecord = new System.Windows.Forms.ToolStripDropDownButton();
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
            this.subrecordPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listSubrecord)).BeginInit();
            this.toolStripSubRecord.SuspendLayout();
            this.SuspendLayout();
            // 
            // subrecordPanel
            // 
            resources.ApplyResources(this.subrecordPanel, "subrecordPanel");
            this.subrecordPanel.Controls.Add(this.listSubrecord);
            this.subrecordPanel.Controls.Add(this.toolStripSubRecord);
            this.subrecordPanel.MinimumSize = new System.Drawing.Size(200, 100);
            this.subrecordPanel.Name = "subrecordPanel";
            // 
            // listSubrecord
            // 
            resources.ApplyResources(this.listSubrecord, "listSubrecord");
            this.listSubrecord.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listSubrecord.AllowDrop = true;
            this.listSubrecord.DataSource = null;
            this.listSubrecord.EnableSearchByKeyboard = false;
            this.listSubrecord.FullRowSelect = true;
            this.listSubrecord.GridLines = true;
            this.listSubrecord.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listSubrecord.HideSelection = false;
            this.listSubrecord.ItemCount = 0;
            this.listSubrecord.Name = "listSubrecord";
            this.listSubrecord.OverlayText.Text = resources.GetString("resource.Text");
            this.listSubrecord.OwnerDraw = true;
            this.listSubrecord.ShowItemToolTips = true;
            this.listSubrecord.UseCompatibleStateImageBehavior = false;
            this.listSubrecord.View = System.Windows.Forms.View.Details;
            this.listSubrecord.VirtualMode = true;
            this.listSubrecord.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            this.listSubrecord.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView1_ItemDrag);
            this.listSubrecord.SelectedIndexChanged += new System.EventHandler(this.listSubrecord_SelectedIndexChanged);
            this.listSubrecord.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView1_DragDrop);
            this.listSubrecord.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView1_DragEnter);
            this.listSubrecord.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.listView1_GiveFeedback);
            this.listSubrecord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listSubrecord_KeyDown);
            this.listSubrecord.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSubrecord_MouseDoubleClick);
            // 
            // toolStripSubRecord
            // 
            resources.ApplyResources(this.toolStripSubRecord, "toolStripSubRecord");
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
            this.toolStripSubRecord.Name = "toolStripSubRecord";
            // 
            // toolStripInsertRecord
            // 
            resources.ApplyResources(this.toolStripInsertRecord, "toolStripInsertRecord");
            this.toolStripInsertRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripInsertRecord.Image = global::TESVSnip.Properties.Resources.insertcell;
            this.toolStripInsertRecord.Name = "toolStripInsertRecord";
            this.toolStripInsertRecord.DropDownClosed += new System.EventHandler(this.toolStripInsertRecord_DropDownClosed);
            this.toolStripInsertRecord.DropDownOpening += new System.EventHandler(this.toolStripInsertRecord_DropDownOpening);
            this.toolStripInsertRecord.Click += new System.EventHandler(this.toolStripInsertRecord_Click);
            // 
            // toolStripDeleteRecord
            // 
            resources.ApplyResources(this.toolStripDeleteRecord, "toolStripDeleteRecord");
            this.toolStripDeleteRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDeleteRecord.Image = global::TESVSnip.Properties.Resources.deletecell;
            this.toolStripDeleteRecord.Name = "toolStripDeleteRecord";
            this.toolStripDeleteRecord.Click += new System.EventHandler(this.toolStripDeleteRecord_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // toolStripMoveRecordUp
            // 
            resources.ApplyResources(this.toolStripMoveRecordUp, "toolStripMoveRecordUp");
            this.toolStripMoveRecordUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMoveRecordUp.Image = global::TESVSnip.Properties.Resources.move_task_up;
            this.toolStripMoveRecordUp.Name = "toolStripMoveRecordUp";
            this.toolStripMoveRecordUp.Click += new System.EventHandler(this.toolStripMoveRecordUp_Click);
            // 
            // toolStripMoveRecordDown
            // 
            resources.ApplyResources(this.toolStripMoveRecordDown, "toolStripMoveRecordDown");
            this.toolStripMoveRecordDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMoveRecordDown.Image = global::TESVSnip.Properties.Resources.move_task_down;
            this.toolStripMoveRecordDown.Name = "toolStripMoveRecordDown";
            this.toolStripMoveRecordDown.Click += new System.EventHandler(this.toolStripMoveRecordDown_Click);
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            // 
            // toolStripEditSubrecord
            // 
            resources.ApplyResources(this.toolStripEditSubrecord, "toolStripEditSubrecord");
            this.toolStripEditSubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripEditSubrecord.Image = global::TESVSnip.Properties.Resources.editclear;
            this.toolStripEditSubrecord.Name = "toolStripEditSubrecord";
            this.toolStripEditSubrecord.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.toolStripEditSubrecord.Click += new System.EventHandler(this.toolStripEditSubrecord_Click);
            // 
            // toolStripEditSubrecordHex
            // 
            resources.ApplyResources(this.toolStripEditSubrecordHex, "toolStripEditSubrecordHex");
            this.toolStripEditSubrecordHex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripEditSubrecordHex.Image = global::TESVSnip.Properties.Resources.xdays;
            this.toolStripEditSubrecordHex.Name = "toolStripEditSubrecordHex";
            this.toolStripEditSubrecordHex.Click += new System.EventHandler(this.toolStripEditSubrecordHex_Click);
            // 
            // toolStripPasteSubrecord
            // 
            resources.ApplyResources(this.toolStripPasteSubrecord, "toolStripPasteSubrecord");
            this.toolStripPasteSubrecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripPasteSubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripPasteSubrecord.Image = global::TESVSnip.Properties.Resources.Paste;
            this.toolStripPasteSubrecord.Name = "toolStripPasteSubrecord";
            this.toolStripPasteSubrecord.Click += new System.EventHandler(this.toolStripPasteSubrecord_Click);
            // 
            // toolStripCopySubrecord
            // 
            resources.ApplyResources(this.toolStripCopySubrecord, "toolStripCopySubrecord");
            this.toolStripCopySubrecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripCopySubrecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripCopySubrecord.Image = global::TESVSnip.Properties.Resources.Copy;
            this.toolStripCopySubrecord.Name = "toolStripCopySubrecord";
            this.toolStripCopySubrecord.Click += new System.EventHandler(this.toolStripCopySubrecord_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // SubrecordListEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.subrecordPanel);
            this.MinimumSize = new System.Drawing.Size(200, 120);
            this.Name = "SubrecordListEditor";
            this.SizeChanged += new System.EventHandler(this.SubrecordListEditor_SizeChanged);
            this.subrecordPanel.ResumeLayout(false);
            this.subrecordPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listSubrecord)).EndInit();
            this.toolStripSubRecord.ResumeLayout(false);
            this.toolStripSubRecord.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel subrecordPanel;
        private Windows.Controls.ObjectBindingListView listSubrecord;
        private System.Windows.Forms.ToolStrip toolStripSubRecord;
        private System.Windows.Forms.ToolStripDropDownButton toolStripInsertRecord;
        private System.Windows.Forms.ToolStripButton toolStripDeleteRecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripMoveRecordUp;
        private System.Windows.Forms.ToolStripButton toolStripMoveRecordDown;
        private System.Windows.Forms.ToolStripSeparator toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripEditSubrecord;
        private System.Windows.Forms.ToolStripButton toolStripEditSubrecordHex;
        private System.Windows.Forms.ToolStripButton toolStripPasteSubrecord;
        private System.Windows.Forms.ToolStripButton toolStripCopySubrecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
