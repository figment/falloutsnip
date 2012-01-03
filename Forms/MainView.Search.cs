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

        internal TreeNode IncrementalSearch(TreeNode tn, bool first, bool forward, bool wrapAround, Predicate<TreeNode> searchFunc)
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
                        tn = GetNextNode(tn);
                    else
                        tn = GetPreviousNode(tn);
                }
                if (wrapAround)
                {
                    if (forward)
                        tn = PluginTree.Nodes[0];
                    else
                        tn = GetLastNode(PluginTree.Nodes[PluginTree.Nodes.Count - 1]);
                }
            } while (tn != null);
            return null;
        }

        private TreeNode GetNextNode(TreeNode tn)
        {
            if (tn.FirstNode != null)
            {
                tn = tn.FirstNode;
            }
            else
            {
                while (tn != null && tn.NextNode == null)
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

        /// <summary>
        /// Helper routine for doing an actual search
        /// </summary>
        /// <param name="type">Type of search to perform</param>
        /// <param name="tn">Starting node to search with</param>
        /// <param name="text">Text to search for</param>
        /// <param name="first">Whether this is the first search (if not current node can be matched)</param>
        /// <param name="partial">Allow for partial text matches</param>
        /// <param name="forward">Search forward or backward</param>
        /// <param name="wrapAround">Whether to wrap around when reach top or bottom</param>
        /// <param name="updateFunc">Function to call to update the UI when doing select</param>
        /// <returns></returns>
        private TreeNode PerformSearch(SearchType type, TreeNode tn, string text, bool first, bool partial, bool forward, bool wrapAround, Predicate<TreeNode> updateFunc)
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
                    if (updateFunc != null && updateFunc(node)) return true;
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
                    if (updateFunc != null && updateFunc(node)) return true;
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
                    if (updateFunc != null && updateFunc(node)) return true;
                    return false;
                };
            }
            return IncrementalSearch(tn, first, forward, wrapAround, searchFunction);
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

        private void BackgroundNonConformingRecordIncrementalSearch(TreeNode tn, bool forward, bool wrapAround)
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

            Predicate<TreeNode> searchFunc = (TreeNode n) =>
            {
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

            StartBackgroundWork(() => { foundNode = IncrementalSearch(tn, false, forward, wrapAround, searchFunc); }
                , () =>
                {
                    if (IsBackroundProcessCanceled())
                    {
                        toolStripIncrInvalidRecStatus.Text = "Search Canceled";
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

        internal bool findNonConformingRecordIncremental(TreeNode tn, bool forward, bool wrapAround)
        {
            var node = IncrementalSearch(tn, false, forward, wrapAround, new Predicate<TreeNode>(IsNonConformingRecord));
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
            BackgroundIncrementalSearch(PluginTree.SelectedNode, true);
        }


        private void BackgroundIncrementalSearch(TreeNode start, bool forward)
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
            toolStripIncrFindStatus.Text = "";

            // Grab selected node before searching as it can only be accessed from UI thread
            if (start == null) start = PluginTree.SelectedNode != null ? PluginTree.SelectedNode : PluginTree.Nodes[0];

            Predicate<TreeNode> updateFunc = (TreeNode n) =>
            {
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

            var item = toolStripIncrFindType.SelectedItem as ComboHelper<SearchType, string>;
            var text = toolStripIncrFindText.Text;
            var partial = !toolStripIncrFindExact.Checked;
            var wrapAround = toolStripIncrFindWrapAround.Checked;
            var first = toolStripIncrFindText.Tag == null ? true : (bool)toolStripIncrFindText.Tag;
            if (string.IsNullOrEmpty(text))
            {
                System.Media.SystemSounds.Beep.Play();
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.Focus();
                return;
            }

            StartBackgroundWork(() => { foundNode = PerformSearch(item.Key, start, text, first, partial, forward, wrapAround, updateFunc); }
                , () =>
                {
                    if (IsBackroundProcessCanceled())
                    {
                        toolStripIncrFindStatus.Text = "Search Canceled";
                        toolStripIncrFindStatus.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        if (foundNode != null)
                        {
                            toolStripIncrFindStatus.Text = "Match Found";
                            toolStripIncrFindStatus.ForeColor = System.Drawing.Color.Black;
                            PluginTree.SelectedNode = foundNode;
                            toolStripIncrFindText.Tag = false;
                        }
                        else
                        {
                            toolStripIncrFindText.Tag = true;

                            toolStripIncrFindStatus.Text = "No Matches Found";
                            toolStripIncrFindStatus.ForeColor = System.Drawing.Color.Maroon;
                            System.Media.SystemSounds.Beep.Play();
                        }
                        toolStripIncrFind.Focus();
                        toolStripIncrFindText.Select();
                        toolStripIncrFindText.Focus();                        
                    }
                }
            );
        }

        private void PerformSearch(TreeNode start, bool forward)
        {
            BackgroundIncrementalSearch(start, forward);
        }
        private void PerformSearch(TreeNode start, bool forward, Predicate<TreeNode> updateFunc)
        {
            var item = toolStripIncrFindType.SelectedItem as ComboHelper<SearchType, string>;
            var text = toolStripIncrFindText.Text;
            var partial = !toolStripIncrFindExact.Checked;
            var wrapAround = toolStripIncrFindWrapAround.Checked;
            var first = toolStripIncrFindText.Tag == null ? true : (bool)toolStripIncrFindText.Tag;
            if (string.IsNullOrEmpty(text))
            {
                System.Media.SystemSounds.Beep.Play();
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.Focus();
                return;
            }
            var node = PerformSearch(item.Key, start, text, first, partial, forward, wrapAround, updateFunc);
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
            BackgroundIncrementalSearch(PluginTree.SelectedNode, false);
        }

        private void toolStripIncrFindRestart_Click(object sender, EventArgs e)
        {
            // use tag to indicate text changed and therefore reset the search
            toolStripIncrFindText.Tag = true;
            BackgroundIncrementalSearch(PluginTree.Nodes.Count > 0 ? PluginTree.Nodes[0] : null, true);
        }

        private void toolStripIncrFindCancel_Click(object sender, EventArgs e)
        {
            toolStripIncrFind.Visible = false;
        }

        private void toolStripIncrFind_VisibleChanged(object sender, EventArgs e)
        {
            findToolStripMenuItem.Checked = toolStripIncrFind.Visible;
            toolStripIncrFindStatus.Text = "";
        }

        private void toolStripIncrFindText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                BackgroundIncrementalSearch(PluginTree.SelectedNode, !e.Shift);
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