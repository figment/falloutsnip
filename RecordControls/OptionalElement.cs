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
    internal partial class OptionalElement : BaseElement, IOuterElementControl
    {
        IElementControl innerControl = null;

        public OptionalElement()
        {
            InitializeComponent();
            chkUseValue.Checked = false;
            UpdateStatus();
        }

        public override ArraySegment<byte> Data
        {
            get 
            { 
                return this.chkUseValue.Checked && this.innerControl != null 
                    ? this.innerControl.Data 
                    : default(ArraySegment<byte>); 
            }
            set
            {
                if (this.InnerControl != null)
                {
                    this.InnerControl.Data = value;
                    chkUseValue.Checked = this.innerControl != null && this.innerControl.Data.Count > 0;
                }
                UpdateAllControls();
            }
        }

        public IElementControl InnerControl
        {
            get { return innerControl; }
            set 
            {
                if (innerControl != value)
                {
                    innerControl = value;
                    this.controlPanel.Controls.Clear();
                    Control c = innerControl as Control;
                    this.SuspendLayout();
                    if (c != null)
                    {
                        c.Dock = DockStyle.Fill;
                        this.controlPanel.MinimumSize = c.MinimumSize;
                        this.controlPanel.Controls.Add(c);
                        this.MinimumSize = this.controlPanel.MinimumSize + new Size(0, chkUseValue.Height);
                    }
                    this.ResumeLayout();
                    UpdateStatus();
                }
            }
        }

        private void chkUseValue_CheckedChanged(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (this.innerControl == null)
                this.controlPanel.Enabled = false;
            else
                this.controlPanel.Enabled = chkUseValue.Checked;
        }

        protected override void UpdateAllControls()
        {
            base.UpdateAllControls();
            UpdateStatus();
        }
    }
}
