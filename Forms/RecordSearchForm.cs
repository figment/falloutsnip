using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.Forms
{
    public partial class RecordSearchForm : Form
    {
        public RecordSearchForm()
        {
            InitializeComponent();
        }

        private void RecordSearchForm_Shown(object sender, EventArgs e)
        {
            this.subrecordPanel.FocusText();
        }
    }
}
