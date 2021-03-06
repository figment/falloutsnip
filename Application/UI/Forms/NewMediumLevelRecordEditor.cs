using FalloutSnip.UI.Scripts;
using FalloutSnip.Framework;
using FunctionOperation = FalloutSnip.UI.Scripts.FunctionOperation;
using PyInterpreter = FalloutSnip.UI.Scripts.PyInterpreter;

namespace FalloutSnip.UI.Forms
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Linq;

    using Domain.Data.Structure;
    using FalloutSnip.Domain.Model;
    using FalloutSnip.Framework.Collections;
    using FalloutSnip.Properties;
    using FalloutSnip.UI.RecordControls;

    internal partial class NewMediumLevelRecordEditor : Form
    {
        private readonly OrderedDictionary<ElementStructure, IElementControl> controlMap = new OrderedDictionary<ElementStructure, IElementControl>();

        private readonly SubRecord sr;

        private readonly string strWarnOnSave;

        private Plugin p;

        private Record r;

        private SubrecordStructure ss;

        public NewMediumLevelRecordEditor(Plugin p, Record r, SubRecord sr, SubrecordStructure ss)
        {
            this.InitializeComponent();
            Icon = Resources.fosnip;
            SuspendLayout();
            this.sr = sr;
            this.ss = ss;
            this.p = p;
            this.r = r;

            // walk each element in standard fashion
            int panelOffset = 0;
            try
            {
                this.fpanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100.0f);
                int maxWidth = this.fpanel1.Width - SystemInformation.VerticalScrollBarWidth - 8;
                int leftOffset = 0; // 8;
                foreach (var elem in ss.elements)
                {
                    Control c = null;
                    if (elem.options != null && elem.options.Length > 1)
                    {
                        c = new OptionsElement();
                    }
                    else if (elem.flags != null && elem.flags.Length > 1)
                    {
                        c = new FlagsElement();
                    }
                    else
                    {
                        switch (elem.type)
                        {
                            case ElementValueType.LString:
                                c = new LStringElement();
                                break;
                            case ElementValueType.FormID:
                                c = new FormIDElement();
                                break;
                            case ElementValueType.Blob:
                                c = new HexElement();
                                break;
                            default:
                                c = new TextElement();
                                break;
                        }
                    }

                    if (c is IElementControl)
                    {
                        var ec = c as IElementControl;
                        ec.formIDLookup = p.GetRecordByID;
                        ec.formIDScan = p.EnumerateRecords;
                        ec.strIDLookup = p.LookupFormStrings;
                        ec.Element = elem;

                        if (elem.repeat > 0)
                        {
                            var ge = new RepeatingElement();
                            c = ge;
                            c.Left = leftOffset;
                            c.Width = maxWidth;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;

                            ge.InnerControl = ec;
                            ge.Element = elem;
                            ec = ge;
                        }
                        else if (elem.optional > 0)
                        {
                            var re = new OptionalElement();
                            c = re;
                            c.Left = leftOffset;
                            c.Width = maxWidth;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;

                            re.InnerControl = ec;
                            re.Element = elem;
                            ec = re;
                            c = re;
                        }
                        else
                        {
                            c.Left = leftOffset;
                            c.Width = maxWidth;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;
                        }

                        this.controlMap.Add(elem, ec);
                        int idx = this.fpanel1.RowCount - 1;
                        this.fpanel1.Controls.Add(c, 0, idx);
                        var info = new RowStyle(SizeType.Absolute, c.Size.Height+2);
                        if (idx == 0)
                            this.fpanel1.RowStyles[0] = info;
                        else
                            this.fpanel1.RowStyles.Add(info);
                        panelOffset = 0;
                        ++this.fpanel1.RowCount;
                    }
                }

                foreach (Element elem in r.EnumerateElements(sr, true))
                {
                    var es = elem.Structure;

                    IElementControl c;
                    if (this.controlMap.TryGetValue(es, out c))
                    {
                        if (c is IGroupedElementControl)
                        {
                            var gc = c as IGroupedElementControl;
                            gc.Elements.Add(elem.Data);
                        }
                        else
                        {
                            c.Data = elem.Data;
                        }
                    }
                }
            }
            catch
            {
                this.strWarnOnSave = "The subrecord doesn't appear to conform to the expected structure.\nThe formatted information may be incorrect.";
                this.Error.SetError(this.bSave, this.strWarnOnSave);
                this.Error.SetIconAlignment(this.bSave, ErrorIconAlignment.MiddleLeft);
                AcceptButton = this.bCancel; // remove save as default button when exception occurs
                CancelButton = this.bCancel;
                UpdateDefaultButton();
            }

            ResumeLayout();
        }

        private bool FocusFirstControl(Control c)
        {
            if (c.CanFocus && c.CanSelect)
            {
                c.Focus();
                return true;
            }

            foreach (Control child in c.Controls)
            {
                if (this.FocusFirstControl(child))
                {
                    return true;
                }
            }

            return false;
        }

        private void NewMediumLevelRecordEditor_Load(object sender, EventArgs e)
        {
            // If more elements than default panel size then allow increasing overall form to 3/4 current monitor size
            if (this.fpanel1.PreferredSize.Height > this.fpanel1.Height)
            {
                var screen = Screen.FromPoint(Location);
                int maxHeight = Owner == null ? screen.WorkingArea.Height : Owner.Height;
                int workingSize = Math.Min(maxHeight, screen.WorkingArea.Height*3/4);
                int offset = this.fpanel1.PreferredSize.Height - this.fpanel1.Height + 40; // height of scrollbar?
                Height = Math.Min(workingSize, Height + offset);

                if (Owner != null)
                {
                    int yOff = (Owner.Height - Height)/2;
                    Top = Owner.Top + yOff;
                }
            }
        }

        private void NewMediumLevelRecordEditor_Shown(object sender, EventArgs e)
        {
            // forward focus to first child control
            this.FocusFirstControl(this.fpanel1);
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            // warn user about data corruption.  But this may be case of fixing using falloutsnip to fix corruption so still allow
            if (this.strWarnOnSave != null)
            {
                if (DialogResult.Yes
                    !=
                    MessageBox.Show(
                        this,
                        this.strWarnOnSave + "\n\nData maybe lost if saved. Do you want to continue saving?",
                        "Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2))
                {
                    return;
                }
            }

            using (var str = new MemoryStream())
            {
                foreach (var kvp in this.controlMap)
                {
                    var c = kvp.Value;
                    if (c is IGroupedElementControl)
                    {
                        var gc = c as IGroupedElementControl;
                        foreach (var elem in gc.Elements)
                        {
                            str.Write(elem.Array, elem.Offset, elem.Count);
                        }
                    }
                    else
                    {

                        var elem = c.Data;
                        if (elem.Count > 0 && elem.Array != null)
                        {
                            switch (kvp.Key.type)
                            {
                                case ElementValueType.UInt:
                                    {
                                        var sf = new ArraySegment<byte>(elem.Array, elem.Offset, elem.Count);
                                        var value = TypeConverter.h2i(sf);
                                        if (!string.IsNullOrWhiteSpace(kvp.Key.funcw))
                                        {
                                            bool valueIsChanged = (kvp.Value).Changed;
                                            if (kvp.Value is OptionalElement)
                                                valueIsChanged = (((OptionalElement) (kvp.Value)).InnerControl).Changed;
                                            if (valueIsChanged)
                                            {
                                                value = PyInterpreter.ExecuteFunction<uint>(kvp.Key, value, FunctionOperation.ForWriting);
                                                var b = TypeConverter.i2h(value);
                                                Buffer.BlockCopy(b, 0, elem.Array, elem.Offset, elem.Count);
                                            }
                                        }
                                    }
                                    break;
                                case ElementValueType.FormID:
                                    {
                                    }
                                    break;

                                case ElementValueType.Int:
                                    {
                                        var sf = new ArraySegment<byte>(elem.Array, elem.Offset, elem.Count);
                                        var value = TypeConverter.h2si(sf);
                                        if (!string.IsNullOrWhiteSpace(kvp.Key.funcw))
                                        {
                                            bool valueIsChanged = (kvp.Value).Changed;
                                            if (kvp.Value is OptionalElement)
                                                valueIsChanged = (((OptionalElement)(kvp.Value)).InnerControl).Changed;
                                            if (valueIsChanged)
                                            {
                                                value = PyInterpreter.ExecuteFunction<int>(kvp.Key, value, FunctionOperation.ForWriting);
                                                var b = TypeConverter.si2h(value);
                                                Buffer.BlockCopy(b, 0, elem.Array, elem.Offset, elem.Count);
                                            }
                                        }
                                    }
                                    break;

                                case ElementValueType.Float:
                                    {
                                        var sf = new ArraySegment<byte>(elem.Array, elem.Offset, elem.Count);
                                        var value = TypeConverter.h2f(sf);
                                        if(!string.IsNullOrWhiteSpace(kvp.Key.funcw))
                                        {
                                            bool valueIsChanged = (kvp.Value).Changed;
                                            if (kvp.Value is OptionalElement)
                                                valueIsChanged = (((OptionalElement) (kvp.Value)).InnerControl).Changed;           
                                            if (valueIsChanged)
                                            {
                                                value = PyInterpreter.ExecuteFunction<float>(kvp.Key, value, FunctionOperation.ForWriting);
                                                var b = TypeConverter.f2h(value);
                                                Buffer.BlockCopy(b, 0, elem.Array, elem.Offset, elem.Count);
                                            }
                                        }
                                    }
                                    break;

                                case ElementValueType.UShort:
                                    {
                                        var sf = new ArraySegment<byte>(elem.Array, elem.Offset, elem.Count);
                                        var value = TypeConverter.h2s(sf);
                                        if (!string.IsNullOrWhiteSpace(kvp.Key.funcw))
                                        {
                                            bool valueIsChanged = (kvp.Value).Changed;
                                            if (kvp.Value is OptionalElement)
                                                valueIsChanged = (((OptionalElement)(kvp.Value)).InnerControl).Changed;
                                            if (valueIsChanged)
                                            {
                                                value = PyInterpreter.ExecuteFunction<ushort>(kvp.Key, value, FunctionOperation.ForWriting);
                                                var b = TypeConverter.s2h(value);
                                                Buffer.BlockCopy(b, 0, elem.Array, elem.Offset, elem.Count);
                                            }
                                        }
                                    }
                                    break;

                                case ElementValueType.Short:
                                    {
                                        var sf = new ArraySegment<byte>(elem.Array, elem.Offset, elem.Count);
                                        var value = TypeConverter.h2ss(sf);
                                        if (!string.IsNullOrWhiteSpace(kvp.Key.funcw))
                                        {
                                            bool valueIsChanged = (kvp.Value).Changed;
                                            if (kvp.Value is OptionalElement)
                                                valueIsChanged = (((OptionalElement)(kvp.Value)).InnerControl).Changed;
                                            if (valueIsChanged)
                                            {
                                                value = PyInterpreter.ExecuteFunction<short>(kvp.Key, value, FunctionOperation.ForWriting);
                                                var b = TypeConverter.ss2h(value);
                                                Buffer.BlockCopy(b, 0, elem.Array, elem.Offset, elem.Count);
                                            }
                                        }
                                    }
                                    break;

                                case ElementValueType.Byte:
                                    {
                                    }
                                    break;
                                case ElementValueType.SByte:
                                    {
                                    }
                                    break;

                                case ElementValueType.String:
                                    break;

                                case ElementValueType.BString:
                                    break;

                                case ElementValueType.IString:
                                    break;

                                case ElementValueType.LString:
                                    {
                                        // not handled
                                    }
                                    break;

                                case ElementValueType.Str4:
                                    {
                                    }
                                    break;
                            }
                            str.Write(elem.Array, elem.Offset, elem.Count);
                        }
                    }
                }

                byte[] newData = str.ToArray();
                this.sr.SetData(newData);
            }
        }

        private void fpanel1_Resize(object sender, EventArgs e)
        {
            //this.fpanel1.SuspendLayout();
            //foreach (Control c in this.fpanel1.Controls)
            //{
            //    c.MinimumSize = new Size(Width - c.Left - SystemInformation.VerticalScrollBarWidth - 4, c.MinimumSize.Height);
            //}
            //this.fpanel1.GetRowHeights().Sum();

            //this.fpanel1.ResumeLayout();
        }
    }
}
