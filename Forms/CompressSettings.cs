using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.Forms
{
    public partial class CompressSettings : Form
    {
        public CompressSettings()
        {
            InitializeComponent();
            this.Icon = global::TESVSnip.Properties.Resources.tesv_ico;
        }

        private void LoadSettings_Load(object sender, EventArgs e)
        {
            this.rdoDefaultCompressRecords.Checked = global::TESVSnip.Properties.Settings.Default.UseDefaultRecordCompression;
            this.rdoNeverCompressRecords.Checked = !global::TESVSnip.Properties.Settings.Default.UseDefaultRecordCompression;
            this.chkEnableAutoCompress.Checked = TESVSnip.Properties.Settings.Default.EnableAutoCompress;
            this.chkEnableCompressLimit.Checked = TESVSnip.Properties.Settings.Default.EnableCompressionLimit;
            this.txtCompressLimit.Text = TESVSnip.Properties.Settings.Default.CompressionLimit.ToString();

            // Groups
            var records = TESVSnip.Properties.Settings.Default.AutoCompressRecords != null
                ? TESVSnip.Properties.Settings.Default.AutoCompressRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];
            var allGroups = TESVSnip.Properties.Settings.Default.AllESMRecords != null
                ? TESVSnip.Properties.Settings.Default.AllESMRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
            foreach (var str in records)
            {
                this.listRecordFilter.Items.Add(str, CheckState.Checked);
                allGroups.Remove(str);
            }
            allGroups.Sort();
            foreach (var str in allGroups)
            {
                this.listRecordFilter.Items.Add(str, CheckState.Unchecked);
            }
            UpdateState();
        }

        private void chkEnableAutoCompress_CheckedChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

        private void rdoNeverCompressRecords_CheckedChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

        void UpdateState()
        {
            this.grpCompSettings.Enabled = rdoDefaultCompressRecords.Checked;
            this.listRecordFilter.Enabled = this.chkEnableAutoCompress.Checked;
            this.txtCompressLimit.Enabled = chkEnableCompressLimit.Checked;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TESVSnip.Properties.Settings.Default.EnableCompressionLimit = this.chkEnableCompressLimit.Checked;
            TESVSnip.Properties.Settings.Default.CompressionLimit = uint.Parse(this.txtCompressLimit.Text);
            TESVSnip.Properties.Settings.Default.EnableAutoCompress = this.chkEnableAutoCompress.Checked;
            TESVSnip.Properties.Settings.Default.AutoCompressRecords = string.Join(";", this.listRecordFilter.CheckedItems.Cast<string>().ToArray());           
            TESVSnip.Properties.Settings.Default.Save();
        }

        private void btnToggleAll_Click(object sender, EventArgs e)
        {
            bool anyChecked = this.listRecordFilter.CheckedItems.Count > 0;
            for (int i = 0, n = this.listRecordFilter.Items.Count; i < n; ++i)
                this.listRecordFilter.SetItemChecked(i, !anyChecked);
        }

        private void chkEnableCompressLimit_CheckedChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

    }
}
