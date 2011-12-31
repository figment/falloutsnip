using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.Forms
{
    public partial class MultilineStringEditor : Form
    {
        public MultilineStringEditor()
        {
            InitializeComponent();
        }
        public MultilineStringEditor(string text)
        {
            this.Text = text;
            InitializeComponent();
        }

        public new String Text { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Text = this.textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void MultilineStringEditor_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = this.Text;
        }

    }
}
