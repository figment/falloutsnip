using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.RecordControls
{
    internal partial class FormIDElement : TextElement
    {
        private class comboBoxItem
        {
            public readonly string name;
            public readonly uint value;
            public comboBoxItem(string name, uint value)
            {
                this.name = name;
                this.value = value;
            }
            public override string ToString()
            {
                return name;
            }
        }

        public FormIDElement()
        {
            InitializeComponent();

            var recitems = RecordStructure.Records.Keys.OfType<object>().ToArray();
            cboRecType.Sorted = true;
            cboRecType.Items.Clear();
            cboRecType.Items.Add("<All>");
            cboRecType.Items.AddRange(recitems);
            cboRecType.SelectedIndex = 0;
        }
        protected override void UpdateText()
        {
            base.UpdateText();
            UpdateRecordList();
        }
        protected override void UpdateElement()
        {
            base.UpdateElement();
            if (!string.IsNullOrEmpty(element.FormIDType))
            {
                cboRecType.SelectedIndex = cboRecType.FindStringExact(element.FormIDType);
            }
            else
                cboRecType.SelectedIndex = 0;
            UpdateRecordList();
        }

        protected override void UpdateLabel()
        {
            base.UpdateLabel();
        }

        protected override void UpdateAllControls()
        {
            base.UpdateAllControls();
        }

        protected virtual void UpdateRecordList()
        {
            // Enumerate all known records.
            cboFormID.Items.Clear();
            if (formIDScan != null)
            {
                var str = cboRecType.Text;
                if (str == "<All>") str = null;
                var options = formIDScan(str);
                if (options == null)
                {
                    this.cboFormID.SelectedIndex = -1;
                }
                else
                {
                    var value = TypeConverter.h2si(data);
                    this.cboFormID.Items.Clear();
                    int idx = -1;
                    foreach (var cbVal in options)
                    {
                        this.cboFormID.Items.Add( new comboBoxItem(cbVal.Value.DescriptiveName, cbVal.Key) );
                        if (cbVal.Key == value)
                            idx = this.cboFormID.Items.Count - 1;
                    }
                    if (idx < this.cboFormID.Items.Count)
                        this.cboFormID.SelectedIndex = idx;
                    else
                        this.cboFormID.SelectedIndex = -1;
                }
            }
        }

        private void cboRecType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRecordList();
        }

        private void cboFormID_SelectedIndexChanged(object sender, EventArgs e)
        {
            uint oldIndex = TypeConverter.h2i(data);
            var cbi = this.cboFormID.SelectedItem as comboBoxItem;
            if (cbi != null)
            {
                uint newIndex = cbi.value;
                if (oldIndex != newIndex)
                {
                    oldIndex = newIndex;
                    this.Data = new ArraySegment<byte>(TypeConverter.i2h(newIndex));
                    this.Changed = true;
                    UpdateText();
                }
            }
        }

        private void FormIDElement_SizeChanged(object sender, EventArgs e)
        {
            this.cboFormID.Width = this.Width - this.cboFormID.Left - 8;
        }
    }
}
