using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESVSnip.Collections.Generic;

namespace TESVSnip.RecordControls
{
    internal partial class RepeatingElement : BaseElement, IOuterElementControl, IGroupedElementControl
    {
        IElementControl innerControl = null;
        AdvancedList<ArraySegment<byte>> elements = new AdvancedList<ArraySegment<byte>>();
        BindingSource bs = new BindingSource();

        public RepeatingElement()
        {
            InitializeComponent();
            
            bs.DataSource = elements;
            this.bindingNavigator.BindingSource = bs;
            bs.CurrentChanged += new EventHandler(bs_CurrentChanged);
        }

        void bs_CurrentChanged(object sender, EventArgs e)
        {
            if (this.innerControl != null)
            {
                var data = (ArraySegment<byte>)bs.Current;
                this.innerControl.Data = data;
            }
        }

        #region IGroupedElementControl Members

        public IList<ArraySegment<byte>> Elements
        {
            get { return elements; }
        }

        #endregion

        protected override void UpdateElement()
        {
            if (this.Element != null && !string.IsNullOrEmpty(this.Element.name))
            {
                this.groupBox1.Text = string.Format("{0}: {1}", this.Element.type, this.Element.name)
                    + (!string.IsNullOrEmpty(Element.desc) ? (" (" + Element.desc + ")") : "");
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
                        c.MinimumSize = new Size(this.Width - c.Left - 24, c.Height - this.controlPanel.Height + c.MinimumSize.Height + 8);
                        this.MinimumSize = new Size(this.Width, this.Size.Height - this.controlPanel.Height + c.MinimumSize.Height + 16);
                        this.controlPanel.MinimumSize = new Size(c.MinimumSize.Width, c.MinimumSize.Height + 8);
                        this.controlPanel.Controls.Add(c);
                        //this.Size = this.MinimumSize;
                    }
                    this.ResumeLayout();
                }
            }
        }

        private void RepeatingElement_Resize(object sender, EventArgs e)
        {
            if (innerControl != null)
            {
                var c = this.innerControl as Control;
                //c.MinimumSize = new Size(this.Width - c.Left - 24, this.MinimumSize.Height);
            }
        }

        private void bindingNavigatorPositionItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
            }
        }
    }
}
