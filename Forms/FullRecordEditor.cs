﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESVSnip.Properties;

namespace TESVSnip.Forms
{
    public partial class FullRecordEditor : Form
    {
        public FullRecordEditor()
        {
            InitializeComponent();
        }

        public FullRecordEditor(Record rec) : this()
        {
            this.Record = rec;
        }


        public Record Record
        {
            get { return this.panelRecordEditor.Record; }
            set
            {
                this.panelRecordEditor.Record = value;
                if (value != null)
                {
                    this.Text = string.Format("{0} - [{1:X8}] {2}", Resources.FullRecordEditorTitle, value.FormID,
                                              value.DescriptiveName);
                }
                
            }
        }
    }

}