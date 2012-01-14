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
    internal partial class LStringElement : TextElement
    {
        public LStringElement()
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
            try
            {
                this.chkUseText.CheckedChanged -= new System.EventHandler(this.chkUseText_CheckedChanged);

                switch (this.Element.type)
                {
                    case ElementValueType.UInt:
                        SetTextByID();
                        break;

                    case ElementValueType.String:
                        SetTextAsString();
                        break;

                    case ElementValueType.LString: // can be either
                        bool isString = TypeConverter.IsLikelyString(data);
                        if (isString)
                            SetTextAsString();
                        else
                            SetTextByID();
                        break;
                }
            }
            finally
            {
                this.chkUseText.CheckedChanged += new System.EventHandler(this.chkUseText_CheckedChanged);
            }
        }

        private void SetTextAsString()
        {
            this.TextBox.ReadOnly = false;
            this.txtString.ReadOnly = false;
            this.txtString.Text = TypeConverter.GetString(data);
            this.Error.SetError(this.txtString, null);
            this.TextBox.Text = 0.ToString("X8");
            this.chkUseText.Checked = true;
            this.TextBox.ReadOnly = true;
        }

        private void SetTextByID()
        {
            var data = this.Data;
            this.txtString.ReadOnly = false;
            uint id = TypeConverter.h2i(data);
            string s = "";
            this.TextBox.Text = id.ToString("X8");
            if (strIDLookup != null)
                s = strIDLookup(id);
            if (s != null)
            {
                this.txtString.Text = s;
                this.Error.SetError(this.txtString, null);
            }
            else
            {
                // dont override text as user could just be toggling checkbox
                this.Error.SetError(this.txtString, "Could not locate string");
                this.Error.SetIconAlignment(this.txtString, ErrorIconAlignment.MiddleLeft);
                this.Error.Icon = global::TESVSnip.Properties.Resources.warning;
            }
            this.txtString.ReadOnly = true;
            this.chkUseText.Checked = false;
        }
        private void chkUseText_CheckedChanged(object sender, EventArgs e)
        {
            SaveText();
            this.txtString.ReadOnly = !chkUseText.Checked;
            this.TextBox.ReadOnly = chkUseText.Checked;
        }
        protected override void SaveText()
        {
            if (chkUseText.Checked)
            {
                this.Data = new ArraySegment<byte>(TypeConverter.str2h(this.txtString.Text));
                this.Error.SetError(TextBox, null);
            }
            else
            {
                string tbText = TextBox.Text;
                System.Globalization.NumberStyles numStyle = System.Globalization.NumberStyles.HexNumber;
                uint i;
                if (!uint.TryParse(tbText, numStyle, null, out i))
                    this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                else
                {
                    this.Error.SetError(TextBox, null);
                    this.Data = new ArraySegment<byte>(TypeConverter.i2h(i));
                }
            }
        }

        private void txtString_Validated(object sender, EventArgs e)
        {
            SaveText();
        }
    }
}
