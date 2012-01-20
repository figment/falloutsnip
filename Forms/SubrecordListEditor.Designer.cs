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
            this.subrecordPanel.Controls.Add(this.listSubrecord);
            this.subrecordPanel.Controls.Add(this.toolStripSubRecord);
            this.subrecordPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subrecordPanel.Location = new System.Drawing.Point(0, 0);
            this.subrecordPanel.MinimumSize = new System.Drawing.Size(225, 100);
            this.subrecordPanel.Name = "subrecordPanel";
            this.subrecordPanel.Size = new System.Drawing.Size(230, 257);
            this.subrecordPanel.TabIndex = 7;
            // 
            // listSubrecord
            // 
            this.listSubrecord.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listSubrecord.AllowDrop = true;
            this.listSubrecord.DataSource = null;
            this.listSubrecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listSubrecord.EnableSearchByKeyboard = false;
            this.listSubrecord.FullRowSelect = true;
            this.listSubrecord.GridLines = true;
            this.listSubrecord.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listSubrecord.HideSelection = false;
            this.listSubrecord.ItemCount = 0;
            this.listSubrecord.Location = new System.Drawing.Point(0, 25);
            this.listSubrecord.Name = "listSubrecord";
            this.listSubrecord.OwnerDraw = true;
            this.listSubrecord.ShowItemToolTips = true;
            this.listSubrecord.Size = new System.Drawing.Size(230, 232);
            this.listSubrecord.TabIndex = 0;
            this.listSubrecord.UseCompatibleStateImageBehavior = false;
            this.listSubrecord.View = System.Windows.Forms.View.Details;
            this.listSubrecord.VirtualMode = true;
            this.listSubrecord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listSubrecord_KeyDown);
            this.listSubrecord.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSubrecord_MouseDoubleClick);
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
            this.toolStripSubRecord.Size = new System.Drawing.Size(230, 25);
            this.toolStripSubRecord.TabIndex = 1;
            // 
            // toolStripInsertRecord
            // 
            this.toolStripInsertRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripInsertRecord.Image = global::TESVSnip.Properties.Resources.insertcell;
            this.toolStripInsertRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripInsertRecord.Name = "toolStripInsertRecord";
            this.toolStripInsertRecord.Size = new System.Drawing.Size(29, 22);
            this.toolStripInsertRecord.Text = "Insert Record";
            this.toolStripInsertRecord.DropDownClosed += new System.EventHandler(this.toolStripInsertRecord_DropDownClosed);
            this.toolStripInsertRecord.DropDownOpening += new System.EventHandler(this.toolStripInsertRecord_DropDownOpening);
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
            // SubrecordListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.subrecordPanel);
            this.Name = "SubrecordListEditor";
            this.Size = new System.Drawing.Size(230, 257);
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
