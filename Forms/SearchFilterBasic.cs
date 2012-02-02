using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using TESVSnip.Model;

namespace TESVSnip.Forms
{
    internal partial class SearchFilterBasic : Form
    {
        private RecordStructure rec;
        private OLVColumn olvColumnName;
        private OLVColumn olvColumnCond;
        private OLVColumn olvColumnValue;

        public SearchFilterBasic()
        {
            InitializeComponent();
            this.Icon = TESVSnip.Properties.Resources.tesv_ico;
        }

        internal SearchFilterBasic(RecordStructure rec)
            : this()
        {
            ConfigureRecord(rec);
        }

        private void SearchFilter_Load(object sender, EventArgs e)
        {
            InitializeComboBox();
            InitializeTreeList();
        }

        class ComboBoxItem<T> where T:class 
        {
            public string Name { get; set; }
            public T Value { get; set; }
            public override string ToString() { return Name; }
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is T) return this.Value.Equals(obj);
                if (obj is ComboBoxItem<T>) return this.Value.Equals(((ComboBoxItem<T>) obj).Value);
                return false;
            }
        }

        private void InitializeComboBox()
        {
            var records = RecordStructure.Records.Values.Select(x =>
                new ComboBoxItem<RecordStructure>{Name=string.Format("{0}: {1}", x.name, x.description), Value = x}
                ).OrderBy(x => x.Name).OfType<object>().ToArray();
            this.cboRecordType.Items.Clear();
            this.cboRecordType.Items.AddRange(records);
            if (this.Criteria != null && !string.IsNullOrEmpty(this.Criteria.Type))
            {
                this.cboRecordType.SelectedItem = RecordStructure.Records[this.Criteria.Type];
            }
            else if (this.rec != null)
            {
                this.cboRecordType.SelectedItem = this.rec;
            }
        }

        private void InitializeTreeList()
        {
            this.filterTree.SelectionChanged += this.filterTree_SelectionChanged;
            this.filterTree.SelectedIndexChanged += this.filterTree_SelectedIndexChanged;
            this.filterTree.SizeChanged += this.filterTree_SizeChanged;
            this.filterTree.Enter += this.filterTree_Enter;
            this.filterTree.KeyDown += this.filterTree_KeyDown;
            this.filterTree.MouseDoubleClick += this.filterTree_MouseDoubleClick;

            filterTree.MultiSelect = true;
            filterTree.CanExpandGetter = x => (x is SearchSubrecord);
            filterTree.ChildrenGetter = x =>
            {
                var r = x as SearchSubrecord;
                return (r != null) ? r.Children : null;
            };

            olvColumnName = new OLVColumn
            {
                Name = "Name", Text = "Name", AspectName = "Name", Width = 175, IsVisible = true, IsEditable = false,
                AspectGetter = x => { var r = x as SearchCriteria; return (r != null) ? r.Name : x;}
            };
            olvColumnCond = new OLVColumn
            {
                Name = "Cond", Text = "Cond", AspectName = "Cond", Width = 100, IsVisible = true, IsEditable = true,
                AspectGetter = x => (x is SearchSubrecord) ? (object)((SearchSubrecord)x).Type : (x is SearchElement) ? (object)((SearchElement)x).Type : null,
                AspectPutter = (x,v) =>
                                   {
                                       if (x is SearchSubrecord) ((SearchSubrecord) x).Type = (SearchCondRecordType) v;
                                       if (x is SearchElement) ((SearchElement) x).Type = (SearchCondElementType) v;
                                   },
            };
            olvColumnValue = new OLVColumn
            {
                Name = "Value", Text = "Value", AspectName = "Value", Width = 100, IsVisible = true, IsEditable = true,
                AspectGetter = x => { var r = x as SearchElement; return (r != null) ? r.Value : null; }
            };
            filterTree.Columns.Add(olvColumnName);
            filterTree.Columns.Add(olvColumnCond);
            filterTree.Columns.Add(olvColumnValue);
            filterTree.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

            filterTree.Roots = filterTree.Roots;

            var checkedItems = new ArrayList();
            var recStruct = this.cboRecordType.SelectedItem as ComboBoxItem<RecordStructure>;
            if (this.Criteria != null && recStruct != null && this.Criteria.Type == recStruct.Value.name)
            {
                var modelItems = filterTree.Roots.OfType<SearchSubrecord>();

                foreach (var item in this.Criteria.Items.OfType<SearchSubrecord>())
                {
                    var modelItem = modelItems.FirstOrDefault(x => x.Name == item.Name);
                    if (modelItem != null)
                    {
                        modelItem.Checked = true;
                        modelItem.Type = item.Type;
                        checkedItems.Add(modelItem);
                    }
                }
                foreach (var item in this.Criteria.Items.OfType<SearchElement>())
                {
                    var modelItem = modelItems.FirstOrDefault(x => x.Name == item.Parent.Name);
                    if (modelItem != null)
                    {
                        filterTree.Expand(modelItem);
                        var modelElem = modelItem.Children.FirstOrDefault(x => x.Name == item.Name);
                        if (modelElem != null)
                        {
                            modelElem.Checked = true;
                            modelElem.Type = item.Type;
                            modelElem.Value = item.Value;
                            checkedItems.Add(modelItem);
                        }
                    }
                }
                this.filterTree.CheckObjects(checkedItems);
            }
        }

        public void ConfigureRecord(RecordStructure rec)
        {
            this.rec = rec;
            this.cboRecordType.SelectedItem = this.rec;
            if (rec == null)
                filterTree.Roots = null;
            else
            {
                var srs = (from sr in rec.subrecords
                           let children = sr.elements.Select(
                                se => new SearchElement() { Checked = false, Name = se.name, Parent = null, Record = se, Type = SearchCondElementType.Exists }
                           ).ToList()
                           select new SearchSubrecord()
                           {
                               Name = string.Format("{0}: {1}", sr.name, sr.desc),
                               Checked = false,
                               Record = sr,
                               Children = children
                           }).ToList();
                // fix parents after assignments
                foreach (var sr in srs)
                    foreach (var se in sr.Children)
                        se.Parent = sr;

                filterTree.Roots = srs;

            }
        }

        private void filterTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void filterTree_KeyDown(object sender, KeyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void filterTree_Enter(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void filterTree_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                int width = this.filterTree.Columns.OfType<OLVColumn>().Sum(x => x.Width);
                var col = this.filterTree.Columns.OfType<OLVColumn>().LastOrDefault(x => x.IsVisible);
                if (col != null)
                {
                    col.Width = this.filterTree.Width - width + col.Width
                              - SystemInformation.VerticalScrollBarWidth - SystemInformation.FrameBorderSize.Width;
                }
            }
            catch
            {
            }
        }

        private void filterTree_SelectedIndexChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void filterTree_SelectionChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void cboRecordType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var recStruct = this.cboRecordType.SelectedItem as ComboBoxItem<RecordStructure>;
            ConfigureRecord(recStruct != null ? recStruct.Value : null);
        }

        public SearchCriteriaSettings Criteria { get; set; }
 

        private bool ApplySettings()
        {
            var recStruct = this.cboRecordType.SelectedItem as ComboBoxItem<RecordStructure>;
            if (recStruct == null)
            {
                this.DialogResult = DialogResult.None;
                MessageBox.Show(this, "No record was selected.", "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                var checkeditems = this.filterTree.CheckedObjectsEnumerable;
                var items = checkeditems.OfType<SearchCriteria>().ToArray();
                if (items.Length == 0)
                {
                    this.DialogResult = DialogResult.None;
                    MessageBox.Show(this, "No search criteria was selected.", "Warning", MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    this.Criteria = new SearchCriteriaSettings();
                    this.Criteria.Type = recStruct.Value.name;
                    this.Criteria.Items = items;
                    return true;
                }
            }
            return false;
        }

        private void bApply_Click(object sender, EventArgs e)
        {
            if (ApplySettings())
            {
                this.DialogResult = DialogResult.No; // Apply.  No to search immediately
            }
        }
        private void bSave_Click(object sender, EventArgs e)
        {
            if (ApplySettings())
            {
                this.DialogResult = DialogResult.Yes; // Search.  Yes to search immediately
            }
        }
    }
}
