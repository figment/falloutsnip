using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESsnip.Collections.Generic;

namespace TESsnip.Forms
{
    public partial class StringsEditor : Form
    {

        class StringHolder
        {
            public Plugin Plugin { get; set; }
            public uint ID { get; set; }
            public string Value { get; set; }
            public LocalizedStringFormat Format { get; set; }
        }
        private AdvancedList<StringHolder> strings = new AdvancedList<StringHolder>();
        private List<StringHolder> addStrings = new List<StringHolder>();
        private List<StringHolder> remStrings = new List<StringHolder>();
        private List<StringHolder> updateStrings = new List<StringHolder>();

        public Plugin[] Plugins { get; set; }

        public StringsEditor()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.tesv_ico;

            this.cboType.DataSource = Enum.GetNames(typeof(LocalizedStringFormat));
            this.cboType.SelectedItem = LocalizedStringFormat.Base.ToString();

            this.listStrings.DataSource = strings;
            this.listStrings.AddBindingColumn("ID", "ID", 80, new Func<StringHolder, string>(a => a.ID.ToString("X8")));
            this.listStrings.AddBindingColumn("Plugin", "Source", 80, new Func<StringHolder, string>(a => a.Plugin.Name));
            this.listStrings.AddBindingColumn("Format", "Format", 50, HorizontalAlignment.Center);
            this.listStrings.AddBindingColumn("Value", "Value", 500);
        }


        private void PopulateStrings()
        {
            if (Plugins == null)
                return;
            foreach (var plugin in Plugins)
            {
                foreach (var kvp in plugin.Strings)
                {
                    this.strings.Add(new StringHolder()
                        {
                            ID = kvp.Key,
                            Plugin = plugin,
                            Value = kvp.Value,
                            Format = LocalizedStringFormat.Base
                        }
                    );
                }
                foreach (var kvp in plugin.ILStrings)
                {
                    this.strings.Add(new StringHolder()
                    {
                        ID = kvp.Key,
                        Plugin = plugin,
                        Value = kvp.Value,
                        Format = LocalizedStringFormat.IL
                    }
                    );
                }
                foreach (var kvp in plugin.DLStrings)
                {
                    this.strings.Add(new StringHolder()
                    {
                        ID = kvp.Key,
                        Plugin = plugin,
                        Value = kvp.Value,
                        Format = LocalizedStringFormat.DL
                    }
                    );
                }
            }
        }
        private void txtID_Validating(object sender, CancelEventArgs e)
        {
            string strID = txtID.Text;
            if (string.IsNullOrEmpty(strID))
                return;
            uint uiID;
            if (!uint.TryParse(strID, System.Globalization.NumberStyles.HexNumber, null, out uiID))
            {
                this.error.SetError(txtID, "Invalid String ID");
                this.txtString.Enabled = false;
                this.btnEditString.Enabled = false;
                this.btnAddString.Enabled = false;
                this.btnDeleteString.Enabled = false;
            }
            else
            {
                this.error.SetError(txtID, null);
                this.txtString.Enabled = true;
                this.btnEditString.Enabled = true;
                this.btnAddString.Enabled = true;
                this.btnDeleteString.Enabled = true;
            }
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnEditString_Click(object sender, EventArgs e)
        {
            using (MultilineStringEditor editor = new MultilineStringEditor())
            {
                editor.Text = txtString.Text;
                DialogResult result = editor.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    txtString.Text = editor.Text;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void StringsEditor_Load(object sender, EventArgs e)
        {
            Settings.GetWindowPosition("StringEditor", this);

            Reload(this.Plugins);
        }

        private void listStrings_DoubleClick(object sender, EventArgs e)
        {
        }

        private void listStrings_Click(object sender, EventArgs e)
        {
            var indices = this.listStrings.SelectedIndices;
            if (indices.Count > 0)
            {
                int idx = indices[0];
                var str = this.strings[idx];
                SetSelectedItem(str);
            }
        }

        private void SetSelectedItem(TESsnip.Forms.StringsEditor.StringHolder str)
        {
            str.ID = str.ID;
            txtID.Text = str.ID.ToString("X8");
            txtString.Text = str.Value;
            cboPlugins.SelectedItem = str.Plugin.Name;
            cboType.SelectedItem = str.Format.ToString();
        }
        private void StringsEditor_ResizeEnd(object sender, EventArgs e)
        {
            this.listStrings.AutoFitColumnHeaders();
        }
        public void Reload(Plugin[] plugins)
        {
            this.Plugins = plugins;
            this.listStrings.DataSource = null;
            this.strings.Clear();
            this.addStrings.Clear();
            this.remStrings.Clear();
            this.updateStrings.Clear();

            List<string> strPlugins = new List<string>();
            foreach (var plugin in Plugins)
                strPlugins.Add(plugin.Name);
            this.cboPlugins.DataSource = strPlugins;
            this.cboPlugins.SelectedIndex = 0;
            PopulateStrings();
            this.listStrings.DataSource = this.strings;
            FitColumns();
            UpdateStatusBar();
        }

        private void btnLookup_Click(object sender, EventArgs e)
        {
            string strID = txtID.Text;
            if (string.IsNullOrEmpty(strID))
            {
                MessageBox.Show(this, "ID Field is empty.  Please specify a string ID to find.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            uint uiID;
            if (!uint.TryParse(strID, System.Globalization.NumberStyles.HexNumber, null, out uiID))
            {
                MessageBox.Show(this, "Unable to parse string id", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                StringHolder holder = this.strings.FirstOrDefault(a => a.ID == uiID);
                if (holder != null)
                    SetSelectedItem(holder);
            }
        }

        private void btnNewItem_Click(object sender, EventArgs e)
        {
            ResetEntry();
        }


        private bool TryGetCurrentID(out uint uiID)
        {
            uiID = 0;

            string strID = txtID.Text;
            if (string.IsNullOrEmpty(strID))
                return false;
            return uint.TryParse(strID, System.Globalization.NumberStyles.HexNumber, null, out uiID);
        }

        private bool TryGetCurrentFormat(out LocalizedStringFormat format)
        {
            format = (LocalizedStringFormat)Enum.Parse(typeof(LocalizedStringFormat), this.cboType.SelectedItem.ToString(), true);
            return true;
        }

        private bool TryGetCurrentPlugin(out Plugin plugin)
        {
            string pluginName = this.cboPlugins.SelectedItem.ToString();
            plugin = Plugins.FirstOrDefault(a => a.Name == pluginName);
            return (plugin != null);
        }

        private void btnAddString_Click(object sender, EventArgs e)
        {
            uint uiID;
            LocalizedStringFormat format;
            Plugin plugin;
            string text = this.txtString.Text;
            if (!TryGetCurrentID(out uiID))
            {
                MessageBox.Show(this, "ID Field is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TryGetCurrentFormat(out format))
            {
                MessageBox.Show(this, "Format is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TryGetCurrentPlugin(out plugin))
            {
                MessageBox.Show(this, "Plugin is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool doResize = this.strings.Count == 0;

            StringHolder str = this.strings.FirstOrDefault(
                a => (a.ID == uiID && a.Plugin.Equals(plugin) && a.Format == format));
            if (str == null)
            {
                str = new StringHolder()
                {
                    ID = uiID,
                    Plugin = plugin,
                    Value = text,
                    Format = format
                };
                this.strings.Add(str);
            }

            StringHolder addStr = this.addStrings.FirstOrDefault(
                a => (a.ID == uiID && a.Plugin.Equals(plugin) && a.Format == format));
            if (addStr == null)
                addStrings.Add(str);

            if (doResize)
                FitColumns();

            UpdateStatusBar();
        }
        private void btnDeleteString_Click(object sender, EventArgs e)
        {
            uint uiID;
            LocalizedStringFormat format;
            Plugin plugin;
            string text = this.txtString.Text;
            if (!TryGetCurrentID(out uiID))
            {
                MessageBox.Show(this, "ID Field is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TryGetCurrentFormat(out format))
            {
                MessageBox.Show(this, "Format is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TryGetCurrentPlugin(out plugin))
            {
                MessageBox.Show(this, "Plugin is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool doResize = this.strings.Count == 0;

            StringHolder str = this.strings.FirstOrDefault(
                a => (a.ID == uiID && a.Plugin.Equals(plugin) && a.Format == format));
            if (str != null)
                this.strings.Remove(str);

            StringHolder remStr = this.remStrings.FirstOrDefault(
                a => (a.ID == uiID && a.Plugin.Equals(plugin) && a.Format == format));
            if (remStr == null)
                remStrings.Add(str);

            if (doResize)
                FitColumns();

            UpdateStatusBar();
        }


        private void FitColumns()
        {
            Application.DoEvents(); // handle outstanding events then do column sizing
            this.listStrings.AutoFitColumnHeaders();
        }

        private LocalizedStringDict GetStringDict(Plugin plugin, LocalizedStringFormat format)
        {
            LocalizedStringDict strings = null;
            switch (format)
            {
                case LocalizedStringFormat.Base: strings = plugin.Strings; break;
                case LocalizedStringFormat.DL: strings = plugin.DLStrings; break;
                case LocalizedStringFormat.IL: strings = plugin.ILStrings; break;
            }
            return strings;
        }

        private void ResetEntry()
        {
            Plugin plugin;
            if (!TryGetCurrentPlugin(out plugin))
            {
                this.txtID.Text = "";
                this.txtString.Text = "";
            }
            else
            {
                LocalizedStringFormat format;
                if (!TryGetCurrentFormat(out format))
                {
                    this.txtID.Text = "";
                    this.txtString.Text = "";
                }
                else
                {
                    LocalizedStringDict strings = GetStringDict(plugin, format);
                    if (strings != null)
                    {
                        if (strings.Count == 0)
                        {
                            this.txtID.Text = 1.ToString("X8");
                            this.txtString.Text = "";
                        }
                        else
                        {
                            uint max = strings.Max(a => a.Key);
                            this.txtID.Text = (max + 1).ToString("X8");
                            this.txtString.Text = "";
                        }
                    }
                }
            }
        }

        private void StringsEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.SetWindowPosition("StringEditor", this);
        }

        private void UpdateStatusBar()
        {
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            foreach (var change in remStrings)
            {
                LocalizedStringDict dict = GetStringDict(change.Plugin, change.Format);
                if (dict != null)
                {
                    dict.Remove(change.ID);
                    change.Plugin.StringsDirty = true;
                }
            }
            foreach (var change in addStrings)
            {
                LocalizedStringDict dict = GetStringDict(change.Plugin, change.Format);
                if (dict != null)
                {
                    dict[change.ID] = change.Value;
                    change.Plugin.StringsDirty = true;
                }
            }
            foreach (var change in updateStrings)
            {
                LocalizedStringDict dict = GetStringDict(change.Plugin, change.Format);
                if (dict != null)
                {
                    dict[change.ID] = change.Value;
                    change.Plugin.StringsDirty = true;
                }
            }
            UpdateStatusBar();
        }

    }
}
