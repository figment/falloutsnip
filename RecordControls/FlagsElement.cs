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
    internal partial class FlagsElement : TextElement
    {
        public FlagsElement()
        {
            InitializeComponent();
            cboFlags.TextChanged += new EventHandler(cboFlags_TextChanged);
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
            if (this.element.flags != null)
            {
                uint value = TypeConverter.h2i(Data);
                cboFlags.SetItems(this.element.flags);
                cboFlags.SetState(value);
            }
        }

        private void cboFlags_TextUpdate(object sender, EventArgs e)
        {
            uint value = cboFlags.GetState();
            uint oldValue = TypeConverter.h2i(Data);
            if (value != oldValue)
            {
                byte[] data = TypeConverter.i2h(value);
                SetCurrentData(new ArraySegment<byte>(data, 0, data.Length));
                TextBox.Text = "0x" + value.ToString("X");
                this.Changed = true;
            }
        }

        void cboFlags_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
