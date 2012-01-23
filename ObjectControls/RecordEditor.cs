using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.Forms
{
    public partial class RecordEditor : UserControl
    {
        public RecordEditor()
        {
            InitializeComponent();
            this.comboBox1.SetItems(FlagDefs.RecFlags1);
        }
    }
}
