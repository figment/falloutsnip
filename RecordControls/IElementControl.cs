using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using TESVSnip.Collections.Generic;

namespace TESVSnip.RecordControls
{
    interface IElementControl
    {
        ElementStructure Element { get; set; }
        ArraySegment<byte> Data { get; set; }
        dFormIDLookupR formIDLookup { get; set; }
        dFormIDScanRec formIDScan { get; set; }
        dLStringLookup strIDLookup { get; set; }
        bool Changed { get; set; }

        void CommitChanges();
        event EventHandler DataChanged;
    }

    interface IOuterElementControl : IElementControl
    {
        IElementControl InnerControl { get; set; }
    }

    interface ITextElementControl : IElementControl
    {
        Label LabelType { get; }
        Label Label { get; }
        TextBoxBase TextBox { get; }
    }

    interface IGroupedElementControl : IElementControl
    {
        IList<ArraySegment<byte>> Elements { get; }
    }
}
