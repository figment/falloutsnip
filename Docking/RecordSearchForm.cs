using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace TESVSnip.Docking
{
    public partial class RecordSearchForm : BaseDockContent
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
