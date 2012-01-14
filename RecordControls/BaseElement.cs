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
    internal partial class BaseElement : UserControl, IElementControl
    {
        protected ArraySegment<byte> data;
        protected ElementStructure element;

        public BaseElement()
        {
            InitializeComponent();
        }

        #region IElementControl Members
        public dFormIDLookupR formIDLookup { get; set; }
        public dFormIDScanRec formIDScan { get; set; }
        public dLStringLookup strIDLookup { get; set; }

        public bool Changed { get; set; }

        public ElementStructure Element
        {
            get { return this.element; }
            set { this.element = value; UpdateElement(); }
        }

        public virtual ArraySegment<byte> Data
        {
            get { return data; }
            set
            {
                if (!EqualsArraySegment<byte>(data, value))
                {
                    if (data != null && data.Array != null)
                        this.Changed = true;
                    data = value;
                    UpdateAllControls();
                }
            }
        }
        private bool EqualsArraySegment<T>(ArraySegment<T> first, ArraySegment<T> second)
        {
            if (first.Count != second.Count) return false;
            for (int i = 0; i < first.Count; ++i)
                if (!first.Array[first.Offset + i].Equals(second.Array[second.Offset + i]))
                    return false;
            return true;
        }

        protected virtual void UpdateElement()
        {
        }

        protected virtual void UpdateAllControls()
        {
        }
        #endregion

        private void BaseElement_Enter(object sender, EventArgs e)
        {
            // forward focus to first child control
            foreach (Control c in this.Controls)
            {
                if (c.CanFocus)
                    c.Focus();
            }
        }
    }
}
