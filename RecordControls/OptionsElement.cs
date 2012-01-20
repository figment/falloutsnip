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
    internal partial class OptionsElement : TextElement
    {
        private class comboBoxItem
        {
            public readonly string name;
            public readonly int value;

            public comboBoxItem(string name, int value)
            {
                this.name = name;
                this.value = value;
            }

            public override string ToString()
            {
                return name;
            }
        }


        public OptionsElement()
        {
            InitializeComponent();
        }

        protected override void UpdateElement()
        {
            base.UpdateElement();
        }

        protected override void UpdateLabel()
        {
            base.UpdateLabel();
        }

        protected override void UpdateAllControls()
        {
            base.UpdateAllControls();
            var data = GetCurrentData();
            if (this.element.options != null)
            {
                var value = TypeConverter.h2si(data);
                this.cboOptions.Items.Clear();
                int idx = -1;
                for (int j = 0; j < element.options.Length; j += 2)
                {
                    int cbVal;
                    if ( int.TryParse(element.options[j+1], out cbVal) )
                    {
                        this.cboOptions.Items.Add(new comboBoxItem(element.options[j], cbVal ));
                        if (cbVal == value)
                            idx = this.cboOptions.Items.Count-1;
                    }
                }
                if (idx < this.cboOptions.Items.Count)
                    this.cboOptions.SelectedIndex = idx;
                else
                    this.cboOptions.SelectedIndex = -1;
            }
        }

        private void cboOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            var data = GetCurrentData();
            int oldIndex = TypeConverter.h2si(data);
            var cbi = this.cboOptions.SelectedItem as comboBoxItem;
            if (cbi != null)
            {
                int newIndex = cbi.value;
                if (oldIndex != newIndex && newIndex != -1)
                {
                    oldIndex = newIndex;
                    SetCurrentData(new ArraySegment<byte>(TypeConverter.si2h(newIndex)));
                    this.Changed = true;
                    UpdateText();
                }
            }
        }

        private void cboOptions_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
    }
}
