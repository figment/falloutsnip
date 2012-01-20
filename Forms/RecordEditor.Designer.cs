namespace TESVSnip.Forms
{
    partial class RecordEditor
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
            this.horizontalSplitContainer = new System.Windows.Forms.SplitContainer();
            this.verticalSplitContainer = new System.Windows.Forms.SplitContainer();
            this.comboBox1 = new TESVSnip.Windows.Controls.FlagComboBox();
            this.tbFlags3 = new System.Windows.Forms.TextBox();
            this.tbFlags2 = new System.Windows.Forms.TextBox();
            this.tbFormID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.subRecordEditor = new TESVSnip.Forms.SubrecordListEditor();
            this.button1 = new TESVSnip.Forms.SubrecordEditor();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.horizontalSplitContainer.Panel1.SuspendLayout();
            this.horizontalSplitContainer.Panel2.SuspendLayout();
            this.horizontalSplitContainer.SuspendLayout();
            this.verticalSplitContainer.Panel1.SuspendLayout();
            this.verticalSplitContainer.Panel2.SuspendLayout();
            this.verticalSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // horizontalSplitContainer
            // 
            this.horizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.horizontalSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.horizontalSplitContainer.Name = "horizontalSplitContainer";
            // 
            // horizontalSplitContainer.Panel1
            // 
            this.horizontalSplitContainer.Panel1.Controls.Add(this.verticalSplitContainer);
            // 
            // horizontalSplitContainer.Panel2
            // 
            this.horizontalSplitContainer.Panel2.Controls.Add(this.button1);
            this.horizontalSplitContainer.Size = new System.Drawing.Size(626, 351);
            this.horizontalSplitContainer.SplitterDistance = 222;
            this.horizontalSplitContainer.TabIndex = 0;
            // 
            // verticalSplitContainer
            // 
            this.verticalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.verticalSplitContainer.IsSplitterFixed = true;
            this.verticalSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.verticalSplitContainer.Name = "verticalSplitContainer";
            this.verticalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // verticalSplitContainer.Panel1
            // 
            this.verticalSplitContainer.Panel1.Controls.Add(this.comboBox1);
            this.verticalSplitContainer.Panel1.Controls.Add(this.tbFlags3);
            this.verticalSplitContainer.Panel1.Controls.Add(this.textBox1);
            this.verticalSplitContainer.Panel1.Controls.Add(this.tbFlags2);
            this.verticalSplitContainer.Panel1.Controls.Add(this.tbFormID);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label3);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label2);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label1);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label4);
            this.verticalSplitContainer.Panel1.Controls.Add(this.tbName);
            // 
            // verticalSplitContainer.Panel2
            // 
            this.verticalSplitContainer.Panel2.Controls.Add(this.subRecordEditor);
            this.verticalSplitContainer.Size = new System.Drawing.Size(222, 351);
            this.verticalSplitContainer.SplitterDistance = 69;
            this.verticalSplitContainer.TabIndex = 0;
            // 
            // comboBox1
            // 
            this.comboBox1.AllowResizeDropDown = true;
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.CheckOnClick = true;
            this.comboBox1.ControlSize = new System.Drawing.Size(47, 16);
            this.comboBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboBox1.DropDownSizeMode = TESVSnip.Windows.Controls.CustomComboBox.SizeMode.UseDropDownSize;
            this.comboBox1.DropSize = new System.Drawing.Size(121, 106);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(105, 24);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(114, 21);
            this.comboBox1.TabIndex = 50;
            this.comboBox1.ValueSeparator = ",";
            // 
            // tbFlags3
            // 
            this.tbFlags3.Location = new System.Drawing.Point(150, 46);
            this.tbFlags3.MaxLength = 8;
            this.tbFlags3.Name = "tbFlags3";
            this.tbFlags3.Size = new System.Drawing.Size(69, 20);
            this.tbFlags3.TabIndex = 49;
            this.tbFlags3.Text = "00000000";
            // 
            // tbFlags2
            // 
            this.tbFlags2.Location = new System.Drawing.Point(42, 46);
            this.tbFlags2.MaxLength = 8;
            this.tbFlags2.Name = "tbFlags2";
            this.tbFlags2.Size = new System.Drawing.Size(57, 20);
            this.tbFlags2.TabIndex = 48;
            this.tbFlags2.Text = "00000000";
            // 
            // tbFormID
            // 
            this.tbFormID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFormID.Location = new System.Drawing.Point(150, 3);
            this.tbFormID.MaxLength = 8;
            this.tbFormID.Name = "tbFormID";
            this.tbFormID.Size = new System.Drawing.Size(69, 20);
            this.tbFormID.TabIndex = 46;
            this.tbFormID.Text = "00000000";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(112, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 45;
            this.label3.Text = "Flags3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 45;
            this.label2.Text = "Flags2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "Flags1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 45;
            this.label4.Text = "Name";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.tbName.Location = new System.Drawing.Point(42, 3);
            this.tbName.MaxLength = 4;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(108, 20);
            this.tbName.TabIndex = 44;
            // 
            // subRecordEditor
            // 
            this.subRecordEditor.AutoSize = true;
            this.subRecordEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subRecordEditor.Location = new System.Drawing.Point(0, 0);
            this.subRecordEditor.Name = "subRecordEditor";
            this.subRecordEditor.Owner = null;
            this.subRecordEditor.Plugin = null;
            this.subRecordEditor.Size = new System.Drawing.Size(222, 278);
            this.subRecordEditor.SubRecord = null;
            this.subRecordEditor.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.MinimumSize = new System.Drawing.Size(300, 200);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(400, 351);
            this.button1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(42, 24);
            this.textBox1.MaxLength = 8;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(57, 20);
            this.textBox1.TabIndex = 48;
            this.textBox1.Text = "00000000";
            // 
            // RecordEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.horizontalSplitContainer);
            this.Name = "RecordEditor";
            this.Size = new System.Drawing.Size(626, 351);
            this.horizontalSplitContainer.Panel1.ResumeLayout(false);
            this.horizontalSplitContainer.Panel2.ResumeLayout(false);
            this.horizontalSplitContainer.ResumeLayout(false);
            this.verticalSplitContainer.Panel1.ResumeLayout(false);
            this.verticalSplitContainer.Panel1.PerformLayout();
            this.verticalSplitContainer.Panel2.ResumeLayout(false);
            this.verticalSplitContainer.Panel2.PerformLayout();
            this.verticalSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer horizontalSplitContainer;
        private System.Windows.Forms.SplitContainer verticalSplitContainer;
        private SubrecordListEditor subRecordEditor;
        private SubrecordEditor button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbFormID;
        private TESVSnip.Windows.Controls.FlagComboBox comboBox1;
        private System.Windows.Forms.TextBox tbFlags3;
        private System.Windows.Forms.TextBox tbFlags2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;

    }
}
