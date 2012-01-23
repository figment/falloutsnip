using System;
using System.Windows.Forms;
using System.Collections.Generic;
using TESVSnip.Windows.Controls;
using System.Linq;
using TESVSnip.RecordControls;
using TESVSnip.Collections.Generic;
using System.Drawing;

namespace TESVSnip
{
    internal partial class NewMediumLevelRecordEditor : Form
    {
        private Plugin p;
        private Record r;
        private SubRecord sr;
        private SubrecordStructure ss;
        private string strWarnOnSave = null;
        OrderedDictionary<ElementStructure, IElementControl> controlMap = new OrderedDictionary<ElementStructure, IElementControl>();

        public NewMediumLevelRecordEditor(Plugin p, Record r, SubRecord sr, SubrecordStructure ss)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.tesv_ico;
            SuspendLayout();
            this.sr = sr;
            this.ss = ss;
            this.p = p;
            this.r = r;

            // walk each element in standard fashion
            int panelOffset = 0;
            try
            {
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
                        ec.formIDLookup = new dFormIDLookupR(p.GetRecordByID);
                        ec.formIDScan = new dFormIDScanRec(p.EnumerateRecords);
                        ec.strIDLookup = new dLStringLookup(p.LookupFormStrings);
                        ec.Element = elem;

                        if (elem.repeat > 0)
                        {
                            var ge = new RepeatingElement();
                            c = ge;
                            c.Left = 8;
                            c.Width = fpanel1.Width - 16;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;

                            ge.InnerControl = ec;
                            ge.Element = elem;
                            ec = ge;
                        }
                        else if (elem.optional)
                        {
                            var re = new OptionalElement();
                            c = re;
                            c.Left = 8;
                            c.Width = fpanel1.Width - 16;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;

                            re.InnerControl = ec;
                            re.Element = elem;
                            ec = re;
                            c = re;
                        }
                        else
                        {
                            c.Left = 8;
                            c.Width = fpanel1.Width - 16;
                            c.Top = panelOffset;
                            c.Anchor = c.Anchor | AnchorStyles.Left | AnchorStyles.Right;
                        }
                        c.MinimumSize = c.Size;

                        controlMap.Add(elem, ec);
                        this.fpanel1.Controls.Add(c);
                        panelOffset = c.Bottom;
                    }
                }

                foreach (Element elem in r.EnumerateElements(sr, true))
                {
                    var es = elem.Structure;

                    IElementControl c;
                    if (controlMap.TryGetValue(es, out c))
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
                strWarnOnSave = "The subrecord doesn't appear to conform to the expected structure.\nThe formatted information may be incorrect.";
                this.Error.SetError(this.bSave, strWarnOnSave);
                this.Error.SetIconAlignment(this.bSave, ErrorIconAlignment.MiddleLeft);
                this.AcceptButton = this.bCancel; // remove save as default button when exception occurs
                this.CancelButton = this.bCancel;
                this.UpdateDefaultButton();
            }
            ResumeLayout();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            // warn user about data corruption.  But this may be case of fixing using tesvsnip to fix corruption so still allow
            if (strWarnOnSave != null)
            {
                if (DialogResult.Yes != MessageBox.Show(this, strWarnOnSave + "\n\nData maybe lost if saved. Do you want to continue saving?"
                    , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    return;
                }
            }

            using (var str = new System.IO.MemoryStream())
            {
                foreach ( KeyValuePair<ElementStructure, IElementControl> kvp in controlMap )
                {
                    var c = kvp.Value;
                    if (c is IGroupedElementControl)
                    {
                        var gc = c as IGroupedElementControl;
                        foreach (var elem in gc.Elements)
                            str.Write(elem.Array, elem.Offset, elem.Count);
                    }
                    else
                    {
                        var elem = c.Data;
                        str.Write(elem.Array, elem.Offset, elem.Count);
                    }
                }
                byte[] newData = str.ToArray();
#if DEBUG
                byte[] originalData = sr.GetReadonlyData();
                if (!ByteArrayCompare(originalData, newData))
                    MessageBox.Show("Data Changed", "Debug", MessageBoxButtons.OK);
#endif
                sr.SetData(newData);
            }
        }

        [System.Runtime.InteropServices.DllImportAttribute("msvcrt.dll")]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {

        }

        private void fpanel1_Resize(object sender, EventArgs e)
        {
            fpanel1.SuspendLayout();
            foreach (Control c in fpanel1.Controls)
            {
                c.MinimumSize = new System.Drawing.Size(this.Width - c.Left - 30, c.MinimumSize.Height);
            }
            fpanel1.ResumeLayout();
        }

        private void NewMediumLevelRecordEditor_Load(object sender, EventArgs e)
        {
            // If more elements than default panel size then allow increasing overall form to 3/4 current monitor size

            if (this.fpanel1.PreferredSize.Height > this.fpanel1.Height)
            {
                var screen = Screen.FromPoint(this.Location);
                int maxHeight = this.Owner == null ? screen.WorkingArea.Height : this.Owner.Height;
                int workingSize = Math.Min(maxHeight, screen.WorkingArea.Height * 3 / 4);
                int offset = this.fpanel1.PreferredSize.Height - this.fpanel1.Height + 40; // height of scrollbar?
                this.Height = Math.Min(workingSize, this.Height + offset);

                if (this.Owner != null)
                {
                    int yOff = (this.Owner.Height - this.Height) / 2;
                    this.Top = this.Owner.Top + yOff;
                }
            }
        }

        private bool FocusFirstControl(Control c)
        {
            if (c.CanFocus && c.CanSelect)
            {
                c.Focus();
                return true;
            }
            foreach ( Control child in c.Controls)
            {
                if ( FocusFirstControl(child) )
                    return true;
            }
            return false;
        }

        private void NewMediumLevelRecordEditor_Shown(object sender, EventArgs e)
        {
            // forward focus to first child control
            FocusFirstControl(this.fpanel1);
        }
    }
}