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
    internal partial class TextElement : BaseElement, ITextElementControl
    {
        public TextElement()
        {
            InitializeComponent();
        }

        #region ITextElementControl Members

        public Label Label
        {
            get { return this.lblText; }
        }

        public TextBoxBase TextBox
        {
            get { return this.textBox; }
        }

        #endregion

        protected override void UpdateElement()
        {
            UpdateLabel();
        }

        protected virtual void UpdateLabel()
        {
            if (this.element != null && !string.IsNullOrEmpty(this.element.name))
            {
                //this.lblText.AutoSize = false;
                this.lblText.Text = string.Format("{0}: {1}", this.element.type, this.element.name)
                    + (!string.IsNullOrEmpty(element.desc) ? (" (" + element.desc + ")") : "");
                //this.lblText.Width = this.lblText.PreferredWidth;
            }
        }

        protected virtual void UpdateText()
        {
            if (this.element == null || this.data == null || this.data.Array == null)
            {
                this.textBox.Text = "<error>";
            }
            else
            {
                bool fitTextBoxToWidth = false;
                var es = element;
                var tb = textBox;
                bool hasFlags = (es.options.Length == 0 && es.flags.Length > 1);
                switch (element.type)
                {
                    case ElementValueType.UInt:
                        {
                            var v = TypeConverter.h2i(data);
                            textBox.Text = element.hexview ? "0x" + v.ToString("X8") : v.ToString();
                        } break;
                    case ElementValueType.Int:
                        {
                            var v = TypeConverter.h2si(data);
                            textBox.Text = hasFlags || es.hexview ? "0x" + v.ToString("X8") : v.ToString();
                        } break;
                    case ElementValueType.FormID:
                        textBox.Text = TypeConverter.h2i(data).ToString("X8");
                        break;
                    case ElementValueType.Float:
                        textBox.Text = TypeConverter.h2f(data).ToString();
                        break;
                    case ElementValueType.UShort:
                        {
                            var v = TypeConverter.h2s(data);
                            textBox.Text = hasFlags || es.hexview ? "0x" + v.ToString("X4") : v.ToString();
                        } break;
                    case ElementValueType.Short:
                        {
                            var v = TypeConverter.h2ss(data);
                            tb.Text = hasFlags || es.hexview ? "0x" + v.ToString("X4") : v.ToString();
                        } break;
                    case ElementValueType.Byte:
                        {
                            var v = TypeConverter.h2b(data);
                            tb.Text = hasFlags || es.hexview ? "0x" + v.ToString("X2") : v.ToString();
                        } break;
                    case ElementValueType.SByte:
                        {
                            var v = TypeConverter.h2sb(data);
                            tb.Text = hasFlags || es.hexview ? "0x" + v.ToString("X2") : v.ToString();
                        } break;
                    case ElementValueType.String:
                        tb.Text = TypeConverter.GetZString(data);
                        fitTextBoxToWidth = true;
                        break;
                    case ElementValueType.BString:
                        tb.Text = TypeConverter.GetBString(data);
                        fitTextBoxToWidth = true;
                        break;
                    case ElementValueType.fstring:
                        tb.Text = TypeConverter.GetString(data);
                        fitTextBoxToWidth = true;
                        break;
                    case ElementValueType.LString:
                        {
                            uint id = TypeConverter.IsLikelyString(data) ? 0 : TypeConverter.h2i(data);
                            tb.Text = id.ToString("X8");
                        } break;
                    case ElementValueType.Str4:
                        {
                            tb.Text = (data.Count >= 4) ? TESVSnip.Encoding.CP1252.GetString(data.Array, data.Offset, 4) : "";
                            tb.MaxLength = 4;
                        } break;
                    default:
                        {
                            tb.Text = "<Error>";
                            tb.Enabled = false;
                        } break;
                }
                if (fitTextBoxToWidth)
                {
                    this.lblText.Left = ((this.Width - this.lblText.Width - 50) /  50) * 50;
                    this.textBox.Width = (this.lblText.Left - 20 - this.textBox.Left);
                    this.textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }
            }
        }

        protected override  void UpdateAllControls()
        {
            UpdateText();
        }

        private void textBox_Validated(object sender, EventArgs e)
        {
            SaveText();
        }

        protected virtual void SaveText()
        {
            if (this.element == null)
                return;
            var es = element;
            var tb = textBox;

            string tbText = textBox.Text;
            System.Globalization.NumberStyles numStyle = System.Globalization.NumberStyles.Any;
            if (tbText.StartsWith("0x"))
            {
                numStyle = System.Globalization.NumberStyles.HexNumber;
                tbText = tbText.Substring(2);
            }

            bool hasFlags = (es.options.Length == 0 && es.flags.Length > 1);
            switch (element.type)
            {
                case ElementValueType.UInt:
                case ElementValueType.FormID:
                    {
                        uint i;
                        if (element.type == ElementValueType.FormID)
                            numStyle = System.Globalization.NumberStyles.HexNumber;
                        if (!uint.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.i2h(i));
                        }
                    } break;
                case ElementValueType.Int:
                    {
                        int i;
                        if (!int.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.si2h(i));
                        }
                    } break;

                case ElementValueType.Float:
                    {
                        float i;
                        if (!float.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.f2h(i));
                        }
                    } break;
                case ElementValueType.UShort:
                    {
                        ushort i;
                        if (!ushort.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.s2h(i));
                        }
                    } break;
                case ElementValueType.Short:
                    {
                        short i;
                        if (!short.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.ss2h(i));
                        }
                    } break;
                case ElementValueType.Byte:
                    {
                        byte i;
                        if (!byte.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.b2h(i));
                        }
                    } break;
                case ElementValueType.SByte:
                    {
                        sbyte i;
                        if (!sbyte.TryParse(tbText, numStyle, null, out i))
                            this.Error.SetError(TextBox, string.Format("Invalid {0} Format", element.type));
                        else
                        {
                            this.Error.SetError(TextBox, null);
                            this.Data = new ArraySegment<byte>(TypeConverter.sb2h(i));
                        }
                    } break;
                case ElementValueType.String:
                    this.Data = new ArraySegment<byte>(TypeConverter.str2h(textBox.Text));
                    break;
                case ElementValueType.BString:
                    this.Data = new ArraySegment<byte>(TypeConverter.bstr2h(textBox.Text));
                    break;
                case ElementValueType.fstring:
                    this.Data = new ArraySegment<byte>(TypeConverter.str2h(textBox.Text));
                    break;
                case ElementValueType.LString:
                    {
                        // not handled
                    } break;
                case ElementValueType.Str4:
                    {
                        byte[] txtbytes = new byte[] { 0x32, 0x32, 0x32, 0x32 };
                        System.Text.Encoding.Default.GetBytes(tbText, 0, Math.Min(4, tbText.Length), txtbytes, 0);
                        this.data = new ArraySegment<byte>(txtbytes);
                    } break;
            }
        }

        private void TextElement_Load(object sender, EventArgs e)
        {

        }
    }
}
