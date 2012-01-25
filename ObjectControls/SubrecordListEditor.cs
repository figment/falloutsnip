using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESVSnip.Properties;

namespace TESVSnip.Forms
{
    public partial class SubrecordListEditor : UserControl
    {
        SelectionContext _context = null;
        private TESVSnip.Collections.Generic.AdvancedList<SubRecord> _subrecords = null;

        public SubrecordListEditor()
        {
            InitializeComponent();
            InitializeSubrecordForm();
            this.SetContext(new SelectionContext());
        }

        public void SetContext(SelectionContext context)
        {
            //this.context = context.Clone();
            if ( this._context != context)
            {
                if (this._context != null)
                {
                    this._context.RecordChanged -= new EventHandler(context_RecordChanged);
                    this._context.SubRecordChanged -= new EventHandler(context_SubRecordChanged);
                }
                this._context = context;
                if (this._context != null)
                {
                    this._context.SubRecordChanged += new EventHandler(context_SubRecordChanged);
                    this._context.RecordChanged += new EventHandler(context_RecordChanged);
                }
            }
        }

        public TESVSnip.Collections.Generic.AdvancedList<SubRecord> SubRecords
        {
            get { return this._subrecords; }
            set
            {
                this._subrecords = value;
                this.listSubrecord.DataSource = _subrecords;
            }
        }

        void context_RecordChanged(object sender, EventArgs e)
        {
            OnRecordChanged();
        }

        void context_SubRecordChanged(object sender, EventArgs e)
        {
            HandleSubrecordChanged();
        }

        void OnRecordChanged()
        {
            _context.SubRecord = null;
            var r = _context.Record as Record;
            _subrecords = r != null ? r.SubRecords : null;
            this.listSubrecord.DataSource = _subrecords;
        }

        void HandleSubrecordChanged()
        {
            var selection = GetSelectedSubrecord();
            if (selection != null)
            {
                if (selection.Equals(_context.SubRecord))
                    return;
            }
            this.listSubrecord.ClearSelection();
            var list = this.listSubrecord.DataSource as IList<SubRecord>;
            if (list != null)
            {
                int idx = list.IndexOf(selection);
                if ( idx >= 0 )
                    this.listSubrecord.SelectItem(idx);
            }
            this.listSubrecord.Refresh();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected SelectionContext Selection
        {
            get { return _context; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Record Record
        {
            get { return _context.Record as Record; }
            set { _context.Record = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SubRecord SubRecord
        {
            get { return _context.SubRecord; }
            set { _context.SubRecord = value; }
        }

        public event EventHandler SelectionChanged;
        protected void FireSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        public event EventHandler DataChanged;

        protected void FireDataChanged()
        {
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);
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

        internal SubRecord GetSelectedSubrecord()
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

        internal List<SubRecord> GetSelectedSubrecords()
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


        private void toolStripInsertRecord_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;

            var tsMenuItem = sender as ToolStripMenuItem;
            try
            {
                var br = Selection.Record;
                var srs = tsMenuItem != null ? tsMenuItem.Tag as SubrecordStructure : null;
                if (br is Record)
                {
                    if (listSubrecord.SelectedIndices.Count == 1)
                    {
                        int idx = listSubrecord.SelectedIndices[0];
                        if (idx < 0 || idx >= (listSubrecord.Items.Count - 1))
                        {
                            SubRecord p = new SubRecord(srs);
                            br.AddRecord(p);
                        }
                        else
                        {
                            Record r = (Record)br;
                            SubRecord p = new SubRecord(srs);
                            r.InsertRecord(idx, p);
                        }
                    }
                    else
                    {
                        SubRecord p = new SubRecord(srs);
                        br.AddRecord(p);
                    }
                }
                FireDataChanged();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        class CallbackAction<T>
        {
            T item;
            Action<T> callback;
            public CallbackAction(T item, Action<T> callback)
            {
                this.item = item;
                this.callback = callback;
            }
            public void Execute()
            {
                this.callback(this.item);
            }
            public void ExecuteEvent(object sender, EventArgs e)
            {
                Execute();
            }
        }
        private void toolStripInsertRecord_DropDownOpening(object sender, EventArgs e)
        {
            // find current subrecord and guess likely candidates
            // this could be much smarter
            try
            {
                toolStripInsertRecord.DropDownItems.Clear();

                var br = Selection.Record as Record;
                var sr = Selection.SubRecord;
                if (br != null)
                {
                    RecordStructure rs;
                    if (RecordStructure.Records.TryGetValue(br.Name, out rs))
                    {
                        var usedNames = new System.Collections.Specialized.StringDictionary();
                        var delayedAddItems = new List<ToolStripMenuItem>();
                        var srs = (sr != null) ? sr.Structure : null;
                        bool found = (srs == null);

                        int idx = listSubrecord.GetFocusedItem();
                        if (idx < 0)
                        {
                            var indicies = listSubrecord.GetSelectionIndices();
                            idx = indicies != null && indicies.Length > 0 ? indicies[0] : -1;
                        }
                        foreach (var s in rs.subrecords)
                        {
                            if (!found && sr.Structure.Equals(s))
                                found = true;

                            if (usedNames.ContainsKey(s.name))
                                continue;

                            usedNames.Add(s.name, s.name);

                            ToolStripMenuItem item = new ToolStripMenuItem(s.name, null,
                                new CallbackAction<SubrecordStructure>(s,
                                    delegate(SubrecordStructure subItem)
                                    {
                                        if (idx == -1) br.AddRecord(new SubRecord(subItem));
                                        else br.InsertRecord(idx, new SubRecord(subItem));
                                        br.MatchRecordStructureToRecord(this.SubRecords.ToArray());
                                        FireDataChanged();
                                    }
                                    ).ExecuteEvent);
                            item.Tag = s;
                            if (found)
                                toolStripInsertRecord.DropDownItems.Add(item);
                            else
                                delayedAddItems.Add(item);
                        }
                        if (delayedAddItems.Count > 0)
                        {
                            if (toolStripInsertRecord.DropDownItems.Count > 0)
                                toolStripInsertRecord.DropDownItems.Add("-");
                            toolStripInsertRecord.DropDownItems.AddRange(delayedAddItems.ToArray());
                        }
                    }
                }
                else
                {
                    toolStripInsertRecord.DropDownItems.Add("NEW_");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripInsertRecord_DropDownClosed(object sender, EventArgs e)
        {
            toolStripInsertRecord.DropDownItems.Clear();
        }



        private void toolStripDeleteRecord_Click(object sender, EventArgs e)
        {
            if (!ValidateMakeChange())
                return;
            var rec = Selection.Record as Record;
            if (rec != null)
            {
                if (listSubrecord.SelectedIndices.Count < 1) return;
                _subrecords.RemoveAt(listSubrecord.SelectedIndices[0]);
                rec.MatchRecordStructureToRecord(this.SubRecords.ToArray());
            }
            Selection.SubRecord = GetSelectedSubrecord();
            FireDataChanged();
        }

        private void toolStripMoveRecordUp_Click(object sender, EventArgs e)
        {
            if (listSubrecord.SelectedIndices.Count < 1) return;
            int idx = listSubrecord.SelectedIndices[0];
            if (idx < 1 || idx >= (listSubrecord.Items.Count))
                return;

            var rec = Selection.Record as Record;
            if (rec != null)
            {
                SubRecord sr = _subrecords[idx];
                _subrecords.RemoveAt(idx);
                _subrecords.Insert(idx - 1, sr);

                listSubrecord.ClearSelection();
                listSubrecord.SelectItem(idx - 1);
                listSubrecord.FocusItem(idx - 1);
                listSubrecord.EnsureVisible(idx - 1);

                Selection.SubRecord = GetSelectedSubrecord();
                rec.MatchRecordStructureToRecord(this.SubRecords.ToArray());
                FireDataChanged();
            }
        }

        private void toolStripMoveRecordDown_Click(object sender, EventArgs e)
        {
            if (listSubrecord.SelectedIndices.Count < 1) return;
            int idx = listSubrecord.SelectedIndices[0];
            if (idx < 0 || idx >= (listSubrecord.Items.Count - 1))
                return;

            var rec = Selection.Record as Record;
            SubRecord sr = _subrecords[idx];
            _subrecords.RemoveAt(idx);
            _subrecords.Insert(idx + 1, sr);

            listSubrecord.ClearSelection();
            listSubrecord.SelectItem(idx + 1);
            listSubrecord.FocusItem(idx + 1);
            listSubrecord.EnsureVisible(idx + 1);


            Selection.SubRecord = GetSelectedSubrecord();
            rec.MatchRecordStructureToRecord(this.SubRecords.ToArray());
            FireDataChanged();
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

        private Plugin GetPluginFromNode(BaseRecord node)
        {
            BaseRecord tn = node;
            var pluginFromNode = tn as Plugin;
            if (pluginFromNode != null) return pluginFromNode;
            while (!(tn is Plugin) && tn != null) tn = tn.Parent;
            if (tn != null) return tn as Plugin;
            return null;
        }


        public delegate void EditSubrecordHandler(SubRecord sr, bool hexView);
        public event EditSubrecordHandler OnEditSubrecord;


        internal void EditSelectedSubrecordHex()
        {
            try
            {
                var rec = Selection.Record as Record;
                if (rec == null) return;
                var p = GetPluginFromNode(rec);
                if (p == null) return;
                var sr = GetSelectedSubrecord();
                if (sr == null) return;

                if (OnEditSubrecord != null)
                {
                    OnEditSubrecord(sr, true);
                    return;
                }


                using (var form = new HexDataEdit(sr.Name, sr.GetData(), p.LookupFormIDS))
                {
                    DialogResult result = form.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        sr.SetData(HexDataEdit.result);
                        sr.Name = HexDataEdit.resultName;
                        rec.MatchRecordStructureToRecord(this.SubRecords.ToArray());

                        if (sr.Parent is Record)
                            sr.Parent.UpdateShortDescription();                        

                        listSubrecord.Refresh();
                        FireSubrecordChanged(sr);
                        FireDataChanged();
                    }
                }
            }
            catch
            {
                
            }
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
            else if (e.KeyCode == Keys.Delete & !e.Control && !e.Alt && !e.Shift)
            {
                DeleteSelection();
            }
        }

        private string[] FormIDScan(string type)
        {
            List<string> ret = new List<string>();
            if (Selection != null && Selection.Record != null)
            {
                var p = GetPluginFromNode(Selection.Record);
                if (p != null)
                {
                    foreach (var pair in p.EnumerateRecords(type))
                    {
                        ret.Add(pair.Value.DescriptiveName);
                        ret.Add(pair.Key.ToString("X8"));
                    }                    
                }
            }
            return ret.ToArray();
        }

        public void EditSelectedSubrecord()
        {
            var context = Selection;
            var rec = Selection.Record as Record;
            if (rec == null) return;
            var p = GetPluginFromNode(rec);
            if (p == null) return;
            var sr = GetSelectedSubrecord();
            if (sr == null) return;

            if (OnEditSubrecord != null)
            {
                OnEditSubrecord(sr, false);
                return;
            }

            if (!global::TESVSnip.Properties.Settings.Default.UseOldSubRecordEditor
                && sr.Structure != null
                && sr.Structure.elements != null
                && sr.Structure.elements[0].type != ElementValueType.Blob && !sr.Structure.UseHexEditor)
            {
                Form re;
                try
                {
                    if (global::TESVSnip.Properties.Settings.Default.UseOldSubRecordEditor)
                    {
                        var r = context.Record;
                        var formIDLookup = new dFormIDLookupS(p.LookupFormIDS);
                        var formIDScan = new dFormIDScan(this.FormIDScan);
                        var strIDLookup = new dLStringLookup(p.LookupFormStrings);
                        re = new MediumLevelRecordEditor(sr, sr.Structure, formIDLookup, formIDScan, strIDLookup);
                    }
                    else
                    {
                        re = new NewMediumLevelRecordEditor(p, rec, sr, sr.Structure);
                    }
                }
                catch
                {
                    MessageBox.Show("Subrecord doesn't seem to conform to the expected structure.", Resources.ErrorText);
                    re = null;
                }
                if (re != null)
                {
                    if (DialogResult.OK == re.ShowDialog(this))
                    {
                        if (sr.Parent is Record)
                            sr.Parent.UpdateShortDescription();                        
                        listSubrecord.Refresh();
                        FireSubrecordChanged(sr);
                        FireDataChanged();
                    }
                    return;
                }
            }
            else
            {
                EditSelectedSubrecordHex();                
            }
        }
        private void CopySelectedSubRecord()
        {
            var sr = GetSelectedSubrecords();
            if (sr == null) return;
            MainView.Clipboard = sr.Select(ss => { return (SubRecord)ss.Clone(); }).ToArray();
            UpdateToolStripSelection();
        }

        private void UpdateToolStripSelection()
        {
            if (Selection.Record != null)
            {
                toolStripInsertRecord.Enabled = true;
                toolStripPasteSubrecord.Enabled = MainView.HasClipboardData<SubRecord[]>();
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
        void PasteSubRecord()
        {
            if (!ValidateMakeChange())
                return;

            if (!MainView.HasClipboardData<SubRecord[]>())
                return;

            try
            {
                BaseRecord br = Selection.Record;

                int insertIdx = listSubrecord.SelectedIndices.Count == 0 ? -1 : listSubrecord.GetFocusedItem();
                var nodes = MainView.GetClipboardData<SubRecord[]>();
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
                MessageBox.Show(ex.ToString(), Resources.ErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void RebuildSelection()
        {
            if (Selection == null || Selection.Record == null)
            {
                return;
            }
            UpdateToolStripSelection();
            listSubrecord.Refresh();
        }

        private bool DragDropInProgress;
        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            UpdateSubRecordSelection(null);
            //EditSelectedSubrecord();
        }
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
            var rec = Selection.Record as Record;
            SubRecord sr = _subrecords[toswap];
            if (listSubrecord.SelectedIndices.Count == 0)
            {
                _subrecords.RemoveAt(toswap);
                _subrecords.Add(sr);
            }
            else if (listSubrecord.SelectedIndices.Count >= 1)
            {
                int moveto = listSubrecord.SelectedIndices[0];
                if (toswap == moveto) return;
                _subrecords.RemoveAt(toswap);
                _subrecords.Insert(moveto, sr);
            }
            else return;
            RebuildSelection();
            return;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (!DragDropInProgress) return;
            e.Effect = DragDropEffects.Move;
            DragDropInProgress = false;
        }

        private void listSubrecord_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSubRecordSelection(null);
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
            FireSelectionChanged();
            UpdateToolStripSelection();
        }

        public void DeleteSelection()
        {
            var objects = listSubrecord.SelectedObjects.OfType<SubRecord>().ToList();
            if (!objects.Any())
                return;
            _subrecords.RemoveRange(objects);
            listSubrecord.RemoveObjects(objects);
        }

        public void PasteFromClipboard()
        {
            PasteSubRecord();
        }

        private void SubrecordListEditor_SizeChanged(object sender, EventArgs e)
        {
            int totalWidth = listSubrecord.Columns.OfType<BrightIdeasSoftware.OLVColumn>().Sum(column => column.Width);
            var descColumn = listSubrecord.Columns.OfType<BrightIdeasSoftware.OLVColumn>().LastOrDefault();
            if (descColumn != null)
            {
                totalWidth -= descColumn.Width;
                descColumn.Width = this.Width - totalWidth - SystemInformation.VerticalScrollBarWidth - SystemInformation.FrameBorderSize.Width;
            }
        }

        internal event EventHandler<RecordChangeEventArgs> OnSubrecordChanged;

        void FireSubrecordChanged(SubRecord sr)
        {
            if (OnSubrecordChanged != null)
            {
                OnSubrecordChanged(this, new RecordChangeEventArgs(sr));
            }
        }

        public void RefreshObject(object model)
        {
            //this.listSubrecord.RefreshObject(model);
            this.listSubrecord.Refresh();
        }
    }
}
