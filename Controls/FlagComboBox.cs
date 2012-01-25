#region License

// Original Source:
//  http://www.codeproject.com/KB/combobox/checkedcombobox.aspx
//
// The Code Project Open License (CPOL) 1.02
//   http://www.codeproject.com/info/cpol10.aspx

#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TESVSnip.Windows.Controls
{
    public partial class FlagComboBox : CheckedComboBox
    {
        public class CCBoxItem
        {
            public CCBoxItem()
                : this(null, 0)
            {
            }

            public CCBoxItem(string name, uint val)
            {
                Name = name;
                Value = val;
            }

            public string Name { get; set; }
            public uint Value { get; set; }

            public override string ToString()
            {
                return string.Format("'{0}' : {1}", Name, Value);
            }
        }

        private readonly List<CCBoxItem> items = new List<CCBoxItem>();

        public FlagComboBox()
        {
            ValueSeparator = ",";
            DataSource = null;
        }

        public void SetItemsFromType(Type t)
        {
            string[] names = Enum.GetNames(t);
            Array values = Enum.GetValues(t);
            SetItems(names, (uint[]) values);
        }

        public void SetItems(string[] names)
        {
            if (names == null || names.Length == 0)
            {
                DataSource = null;
                return;
            }
            var values = new uint[names.Length];
            for (int i = 0; i < names.Length; ++i)
                values[i] = (uint) 1 << i;
            SetItems(names, values);
        }

        public void SetItems(string[] names, uint[] values)
        {
            if (names == null || names.Length == 0 || values == null || values.Length == 0 ||
                names.Length != values.Length)
            {
                DataSource = null;
                return;
            }
            items.Clear();
            for (int i = 0; i < names.Length; ++i)
            {
                string s = names[i];
                if (string.IsNullOrEmpty(s))
                    continue;
                items.Add(new CCBoxItem(s, values[i]));
            }
            MaxDropDownItems = Math.Min(values.Length, 16);
            DisplayMember = "Name";
            ValueSeparator = ",";
            //this.ValueMember = "Value";

            foreach (var item in items) Items.Add(item);
        }

        public void SetState(uint value)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                SetItemChecked(i, ((value & items[i].Value) == items[i].Value));
            }
        }

        public uint GetState()
        {
            uint value = 0;

            for (int i = 0; i < items.Count; ++i)
            {
                var state = GetItemCheckState(i);
                if (state != CheckState.Unchecked)
                    value |= items[i].Value;
            }
            return value;
        }
    }

    // end public class CheckedComboBox
}