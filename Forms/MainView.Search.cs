using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TESVSnip
{
    /// <summary>
    /// This file contains the incremental search related functionality for the main form
    /// </summary>
    internal partial class MainView
    {
        #region Search Helpers
        enum SearchType
        {
            EditorID,
            FormID,
            FullSearch
        }
        class ComboHelper<T, U>
        {
            public ComboHelper(T key, U value)
            {
                this.Key = key;
                this.Value = value;
            }
            public T Key { get; set; }
            public U Value { get; set; }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        void InitializeToolStripFind()
        {
            ComboHelper<SearchType, string>[] items = new ComboHelper<SearchType, string>[]
            {
                new ComboHelper<SearchType, string>(SearchType.EditorID, "Editor ID"),
                new ComboHelper<SearchType, string>(SearchType.FormID, "Form ID"),
                new ComboHelper<SearchType, string>(SearchType.FullSearch, "Full Search"),
            };
            toolStripIncrFindType.Items.Clear();
            foreach (var itm in items) toolStripIncrFindType.Items.Add(itm);
            toolStripIncrFindType.SelectedItem = toolStripIncrFindType.Items[0];
            toolStripIncrFindText.Tag = true; // text tag first search
        }

        private void RecurseFullSearch(List<TreeNode> matches, TreeNode node, string searchString, bool partial)
        {
            Record rec = node.Tag as Record;
            if (rec != null)
            {
                foreach (SubRecord sr in rec.SubRecords)
                {
                    if (partial)
                    {
                        if (sr.GetStrData().ToLowerInvariant().Contains(searchString)) matches.Add(node);
                    }
                    else
                    {
                        if (sr.GetStrData().ToLowerInvariant() == searchString) matches.Add(node);
                    }
                }
            }
            else
            {
                for (int i = 0; i < node.Nodes.Count; i++)
                    RecurseFullSearch(matches, node.Nodes[i], searchString, partial);
            }
        }

        internal TreeNode IncrementalSearch(TreeNode tn, bool first, bool forward, bool downOnly, Predicate<TreeNode> searchFunc)
        {
            if (PluginTree.Nodes.Count == 0)
                return null;
            if (tn == null)
                tn = PluginTree.SelectedNode != null ? PluginTree.SelectedNode : PluginTree.Nodes[0];
            if (tn == null)
                return null;
            TreeNode startNode = null;
            bool keep = first;
            do
            {
                var prevNode = tn;
                while (tn != null)
                {
                    prevNode = tn;
                    if (keep && searchFunc(tn))
                        return tn;
                    keep = true;
                    if (startNode == null) // set the start node
                        startNode = tn;
                    else if (startNode == tn) // if we found the start node again then fail
                        return null;
                    if (forward)
                        tn = GetNextNode(tn, downOnly);
                    else
                        tn = GetPreviousNode(tn);
                }
                if (!downOnly)
                {
                    if (forward)
                        tn = PluginTree.Nodes[0];
                    else
                        tn = GetLastNode(PluginTree.Nodes[PluginTree.Nodes.Count - 1]);
                }
            } while (tn != null);
            return null;
        }

        private TreeNode GetNextNode(TreeNode tn, bool downOnly)
        {
            if (tn.FirstNode != null)
            {
                tn = tn.FirstNode;
            }
            else
            {
                while (!downOnly && tn != null && tn.NextNode == null)
                    tn = tn.Parent;
                tn = (tn != null && tn.NextNode != null) ? tn.NextNode : null;
            }
            return tn;
        }
        private TreeNode GetPreviousNode(TreeNode n)
        {
            while (n != null && n.PrevNode == null)
                n = n.Parent;
            if (n != null)
                n = n.PrevNode;
            if (n != null)
                n = GetLastNode(n);
            return n;
        }

        private static TreeNode GetLastNode(TreeNode n)
        {
            // Find last item
            while (n.FirstNode != null)
            {
                n = n.FirstNode;
                while (n != null && n.NextNode != null)
                    n = n.NextNode;
            }
            return n;
        }

        private TreeNode PerformSearch(SearchType type, TreeNode tn, string text, bool first, bool partial, bool forward, bool downOnly)
        {
            Predicate<TreeNode> searchFunction = null;

            if (type == SearchType.FormID)
            {
                uint searchID;
                if (!uint.TryParse(text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out searchID))
                {
                    MessageBox.Show("Invalid FormID");
                    return null;
                }
                searchFunction = (TreeNode node) =>
                {
                    var rec = node.Tag as Record;
                    return (rec != null) ? rec.FormID == searchID : false;
                };
            }
            else if (type == SearchType.EditorID)
            {
                string searchString = text.ToLowerInvariant();
                searchFunction = (TreeNode node) =>
                {
                    var rec = node.Tag as Record;
                    if (rec != null && !string.IsNullOrEmpty(rec.descriptiveName))
                    {
                        if (partial)
                        {
                            var val = rec.descriptiveName.ToLowerInvariant();
                            if (val.Contains(searchString))
                                return true;
                        }
                        else
                        {
                            var val = rec.descriptiveName.ToLowerInvariant().Substring(2, rec.descriptiveName.Length - 3);
                            if (val == searchString)
                                return true;
                        }
                    }
                    return false;
                };

            }
            else if (type == SearchType.FullSearch)
            {
                string searchString = text.ToLowerInvariant();
                searchFunction = (TreeNode node) =>
                {
                    var rec = node.Tag as Record;
                    if (rec != null)
                    {
                        foreach (SubRecord sr in rec.SubRecords)
                        {
                            var val = sr.GetStrData();
                            if (!string.IsNullOrEmpty(val))
                            {
                                val = val.ToLowerInvariant();
                                if ((partial && val.Contains(searchString)) || (val == searchString))
                                    return true;
                            }
                        }
                    }
                    return false;
                };
            }
            return IncrementalSearch(tn, first, forward, downOnly, searchFunction);
        }


        #region Non-Conforming Records
        private bool findNonConformingRecordInternal(TreeNode tn)
        {
            if (tn.Tag is Record)
            {
                if (IsNonConformingRecord(tn))
                {
                    PluginTree.SelectedNode = tn;
                    return true;
                }
            }
            else
            {
                foreach (TreeNode tn2 in tn.Nodes) if (findNonConformingRecordInternal(tn2)) return true;
            }
            return false;
        }
        private void findNonconformingRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripIncrInvalidRec.Visible = !toolStripIncrInvalidRec.Visible;
            if (toolStripIncrInvalidRec.Visible)
            {
                toolStripIncrInvalidRec.Focus();
                toolStripIncrInvalidRecNext.Select();
            }
        }

        private void BackgroundNonConformingRecordIncrementalSearch(TreeNode tn, bool forward, bool downOnly)
        {
            float totalNodes = (int)PluginTree.GetNodeCount(true);
            if (totalNodes == 0)
            {
                toolStripIncrInvalidRecStatus.Text = "No Plugins Loaded";
                toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Maroon;
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            int prevCount = 0;
            float currentCount = 0.0f;
            TreeNode foundNode = null;
            toolStripIncrInvalidRecStatus.Text = "";
            if (tn == null) tn = PluginTree.SelectedNode != null ? PluginTree.SelectedNode : PluginTree.Nodes[0];

            Predicate<TreeNode> searchFunc = (TreeNode n) => {
                if (IsNonConformingRecord(n))
                    return true;
                if (IsBackroundProcessCanceled()) // returning true will stop it
                    return true;
                int counter = (int)((float)++currentCount / totalNodes * 100.0f);
                if (counter != prevCount)
                {
                    prevCount = counter;
                    if (counter % 10 == 0) UpdateBackgroundProgress(counter);
                }
                return false;
            };

            StartBackgroundWork(() => { foundNode = IncrementalSearch(tn, false, forward, downOnly, searchFunc); }
                , () => {
                    if (IsBackroundProcessCanceled())
                    {
                        toolStripIncrInvalidRecStatus.Text = "Search Cancelled";
                        toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        if (foundNode != null)
                        {
                            toolStripIncrInvalidRecStatus.Text = "Invalid Record Found";
                            toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Black;
                            PluginTree.SelectedNode = foundNode;
                        }
                        else
                        {
                            toolStripIncrInvalidRecStatus.Text = "No Invalid Records Found";
                            toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Maroon;
                            System.Media.SystemSounds.Beep.Play();
                        }
                    }
                }
            ); 
        }

        internal bool findNonConformingRecordIncremental(TreeNode tn, bool forward, bool downOnly)
        {
            var node = IncrementalSearch(tn, false, forward, downOnly, new Predicate<TreeNode>(IsNonConformingRecord));
            if (node != null)
                PluginTree.SelectedNode = node;
            return node != null;
        }

        private bool IsNonConformingRecord(TreeNode tn)
        {
            if (tn.Tag is Record)
            {
                Record r = tn.Tag as Record;

                if (!MatchRecordStructureToRecord(r))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #endregion


        #region Increment Record Search

        private void toolStripIncrFindMatch_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

            }
        }

        private void toolStripIncrFindNext_Click(object sender, EventArgs e)
        {
            PerformSearch(PluginTree.SelectedNode, true);
        }

        private void PerformSearch(TreeNode start, bool forward)
        {
            var item = toolStripIncrFindType.SelectedItem as ComboHelper<SearchType, string>;
            var text = toolStripIncrFindText.Text;
            var partial = !toolStripIncrFindExact.Checked;
            var downOnly = toolStripIncrFindDown.Checked;
            var first = toolStripIncrFindText.Tag == null ? true : (bool)toolStripIncrFindText.Tag;
            if (string.IsNullOrEmpty(text))
            {
                System.Media.SystemSounds.Beep.Play();
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.Focus();
                return;
            }
            var node = PerformSearch(item.Key, start, text, first, partial, forward, downOnly);
            if (node != null)
            {
                PluginTree.SelectedNode = node;
                toolStripIncrFindText.Tag = false;
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.Focus();
                toolStripIncrFindText.Tag = true;
            }
        }

        private void toolStripIncrFindPrev_Click(object sender, EventArgs e)
        {
            PerformSearch(PluginTree.SelectedNode, false);
        }

        private void toolStripIncrFindRestart_Click(object sender, EventArgs e)
        {
            // use tag to indicate text changed and therefore reset the search
            toolStripIncrFindText.Tag = true;

            PerformSearch(PluginTree.Nodes.Count > 0 ? PluginTree.Nodes[0] : null, true);
        }

        private void toolStripIncrFindCancel_Click(object sender, EventArgs e)
        {
            toolStripIncrFind.Visible = false;
        }

        private void toolStripIncrFind_VisibleChanged(object sender, EventArgs e)
        {
            findToolStripMenuItem.Checked = toolStripIncrFind.Visible;
        }

        private void toolStripIncrFindText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                PerformSearch(PluginTree.SelectedNode, !e.Shift);
            }
        }
        private void toolStripIncrFindText_TextChanged(object sender, EventArgs e)
        {
            // use tag to indicate text changed and therefore reset the search
            toolStripIncrFindText.Tag = true;
        }

        #endregion


    }
}