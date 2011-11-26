namespace TESsnip.Forms
{
    partial class StringsEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddString = new System.Windows.Forms.Button();
            this.txtID = new System.Windows.Forms.TextBox();
            this.txtString = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnEditString = new System.Windows.Forms.Button();
            this.tip = new System.Windows.Forms.ToolTip(this.components);
            this.btnLookup = new System.Windows.Forms.Button();
            this.btnDeleteString = new System.Windows.Forms.Button();
            this.error = new System.Windows.Forms.ErrorProvider(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.cboPlugins = new System.Windows.Forms.ComboBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnNewItem = new System.Windows.Forms.Button();
            this.listStrings = new TESsnip.Windows.Controls.BindingListView();
            this.btnApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.error)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 327);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "ID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddString
            // 
            this.btnAddString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddString.Image = global::TESsnip.Properties.Resources.edit_add;
            this.btnAddString.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddString.Location = new System.Drawing.Point(117, 374);
            this.btnAddString.Name = "btnAddString";
            this.btnAddString.Size = new System.Drawing.Size(56, 23);
            this.btnAddString.TabIndex = 2;
            this.btnAddString.Text = "Add";
            this.btnAddString.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.tip.SetToolTip(this.btnAddString, "Add String To Plugin");
            this.btnAddString.UseVisualStyleBackColor = true;
            this.btnAddString.Click += new System.EventHandler(this.btnAddString_Click);
            // 
            // txtID
            // 
            this.txtID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtID.Location = new System.Drawing.Point(54, 324);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(100, 20);
            this.txtID.TabIndex = 3;
            this.txtID.TextChanged += new System.EventHandler(this.txtID_TextChanged);
            this.txtID.Validating += new System.ComponentModel.CancelEventHandler(this.txtID_Validating);
            // 
            // txtString
            // 
            this.txtString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtString.Location = new System.Drawing.Point(54, 348);
            this.txtString.Name = "txtString";
            this.txtString.Size = new System.Drawing.Size(500, 20);
            this.txtString.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 351);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "String:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnEditString
            // 
            this.btnEditString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditString.Location = new System.Drawing.Point(560, 346);
            this.btnEditString.Name = "btnEditString";
            this.btnEditString.Size = new System.Drawing.Size(26, 23);
            this.btnEditString.TabIndex = 2;
            this.btnEditString.Text = "...";
            this.tip.SetToolTip(this.btnEditString, "Multiline Text Editor");
            this.btnEditString.UseVisualStyleBackColor = true;
            this.btnEditString.Click += new System.EventHandler(this.btnEditString_Click);
            // 
            // btnLookup
            // 
            this.btnLookup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLookup.Image = global::TESsnip.Properties.Resources.find;
            this.btnLookup.Location = new System.Drawing.Point(159, 322);
            this.btnLookup.Name = "btnLookup";
            this.btnLookup.Size = new System.Drawing.Size(26, 23);
            this.btnLookup.TabIndex = 6;
            this.tip.SetToolTip(this.btnLookup, "Lookup Text");
            this.btnLookup.UseVisualStyleBackColor = true;
            this.btnLookup.Click += new System.EventHandler(this.btnLookup_Click);
            // 
            // btnDeleteString
            // 
            this.btnDeleteString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteString.Image = global::TESsnip.Properties.Resources.edit_remove;
            this.btnDeleteString.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDeleteString.Location = new System.Drawing.Point(179, 374);
            this.btnDeleteString.Name = "btnDeleteString";
            this.btnDeleteString.Size = new System.Drawing.Size(62, 23);
            this.btnDeleteString.TabIndex = 2;
            this.btnDeleteString.Text = "Delete";
            this.btnDeleteString.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.tip.SetToolTip(this.btnDeleteString, "Remove String From Plugin");
            this.btnDeleteString.UseVisualStyleBackColor = true;
            this.btnDeleteString.Click += new System.EventHandler(this.btnDeleteString_Click);
            // 
            // error
            // 
            this.error.ContainerControl = this;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(228, 327);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Plugin:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboPlugins
            // 
            this.cboPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboPlugins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPlugins.FormattingEnabled = true;
            this.cboPlugins.Location = new System.Drawing.Point(273, 323);
            this.cboPlugins.Name = "cboPlugins";
            this.cboPlugins.Size = new System.Drawing.Size(121, 21);
            this.cboPlugins.TabIndex = 8;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(525, 374);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(62, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.button1_Click);
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(465, 323);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(121, 21);
            this.cboType.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(425, 327);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Type:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnNewItem
            // 
            this.btnNewItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNewItem.Image = global::TESsnip.Properties.Resources.insertcell;
            this.btnNewItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNewItem.Location = new System.Drawing.Point(55, 374);
            this.btnNewItem.Name = "btnNewItem";
            this.btnNewItem.Size = new System.Drawing.Size(56, 23);
            this.btnNewItem.TabIndex = 2;
            this.btnNewItem.Text = "New";
            this.btnNewItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnNewItem.UseVisualStyleBackColor = true;
            this.btnNewItem.Click += new System.EventHandler(this.btnNewItem_Click);
            // 
            // listStrings
            // 
            this.listStrings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listStrings.AutoScroll = false;
            this.listStrings.DataSource = null;
            this.listStrings.FullRowSelect = true;
            this.listStrings.GridLines = true;
            this.listStrings.HideSelection = false;
            this.listStrings.ItemCount = 0;
            this.listStrings.Location = new System.Drawing.Point(12, 12);
            this.listStrings.Name = "listStrings";
            this.listStrings.Size = new System.Drawing.Size(574, 308);
            this.listStrings.TabIndex = 0;
            this.listStrings.UseCompatibleStateImageBehavior = false;
            this.listStrings.View = System.Windows.Forms.View.Details;
            this.listStrings.VirtualMode = true;
            this.listStrings.Click += new System.EventHandler(this.listStrings_Click);
            this.listStrings.DoubleClick += new System.EventHandler(this.listStrings_DoubleClick);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnApply.Location = new System.Drawing.Point(457, 374);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(62, 23);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // StringsEditor
            // 
            this.AcceptButton = this.btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(598, 409);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboPlugins);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnLookup);
            this.Controls.Add(this.txtString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtID);
            this.Controls.Add(this.btnEditString);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDeleteString);
            this.Controls.Add(this.btnNewItem);
            this.Controls.Add(this.btnAddString);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listStrings);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StringsEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Strings Editor";
            this.Load += new System.EventHandler(this.StringsEditor_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StringsEditor_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.StringsEditor_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.error)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Windows.Controls.BindingListView listStrings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddString;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.TextBox txtString;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnEditString;
        private System.Windows.Forms.ToolTip tip;
        private System.Windows.Forms.ErrorProvider error;
        private System.Windows.Forms.Button btnLookup;
        private System.Windows.Forms.Button btnDeleteString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboPlugins;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnNewItem;
        private System.Windows.Forms.Button btnApply;
    }
}