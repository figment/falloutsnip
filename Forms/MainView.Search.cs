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
            FullSearch,
            TypeEditorIdSearch,
            TypeFullSearch,
            FormIDRef,
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
        #endregion

        void InitializeToolStripFind()
        {
            ComboHelper<SearchType, string>[] items = new ComboHelper<SearchType, string>[]
            {
                new ComboHelper<SearchType, string>(SearchType.EditorID, "Editor ID"),
                new ComboHelper<SearchType, string>(SearchType.FormID, "Form ID"),
                new ComboHelper<SearchType, string>(SearchType.FullSearch, "Full Search"),
                new ComboHelper<SearchType, string>(SearchType.TypeEditorIdSearch, "Name w/Type"),
                new ComboHelper<SearchType, string>(SearchType.TypeFullSearch, "Full w/Type"),
                new ComboHelper<SearchType, string>(SearchType.FormIDRef, "Form ID Ref."),               
            };
            toolStripIncrFindType.Items.Clear();
            foreach (var itm in items) toolStripIncrFindType.Items.Add(itm);
            toolStripIncrFindType.SelectedIndex = 0;
            //toolStripIncrFindType.SelectedItem = toolStripIncrFindType.Items[0];
            ResetSearch();
            toolStripIncrFindStatus.Text = "";

            if (!RecordStructure.Loaded)
                RecordStructure.Load();
            var recitems = RecordStructure.Records.Keys.OfType<object>().ToArray();
            //var recitems = TESVSnip.Properties.Settings.Default.AllESMRecords != null
            //    ? TESVSnip.Properties.Settings.Default.AllESMRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).OfType<object>().ToArray()
            //    : new object[0];
            toolStripIncrFindTypeFilter.Sorted = true;
            toolStripIncrFindTypeFilter.Items.Clear();
            toolStripIncrFindTypeFilter.Items.AddRange(recitems);
            toolStripIncrFindTypeFilter.SelectedIndex = 0;
        }

        private void toolStripIncrFindType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = toolStripIncrFindType.SelectedItem as ComboHelper<SearchType, string>;
            if (item != null && (item.Key == SearchType.TypeEditorIdSearch || item.Key == SearchType.TypeFullSearch))
            {
                toolStripIncrFindTypeFilter.Visible = true;
                toolStripIncrFindExact.Visible = false;
            }
            else
            {
                toolStripIncrFindTypeFilter.Visible = false;
                toolStripIncrFindExact.Visible = true;
            }
        }

        private void toolStripIncrFindTypeFilter_VisibleChanged(object sender, EventArgs e)
        {
            if (toolStripIncrFindTypeFilter.Visible == true)
            {
                if (Selection != null)
                {
                    Selection.RecordChanged += new EventHandler(toolStripIncrFindSelection_RecordChanged);
                    toolStripIncrFindSelection_RecordChanged(null, null);
                }
            }
            else
            {
                Selection.RecordChanged -= new EventHandler(toolStripIncrFindSelection_RecordChanged);
            }
        }

        void toolStripIncrFindSelection_RecordChanged(object sender, EventArgs e)
        {
            if (Selection != null && Selection.Record != null)
            {
                int idx = toolStripIncrFindTypeFilter.FindStringExact(Selection.Record.Name);
                if (idx >= 0) toolStripIncrFindTypeFilter.SelectedIndex = idx;
            }
        }

        #region class RecursiveRecordIterator
        class RecursiveRecordIterator : IEnumerator<BaseRecord>
        {
            Stack<RecordIterator> stack;
            bool forward;
            bool needMoveNext = false;
            public RecursiveRecordIterator(BaseRecord tn, bool forward)
            {
                this.forward = forward;
                Push(tn);
            }

            public BaseRecord Current
            {
                get { return stack.Peek().Current; }
            }

            public void Dispose()
            {
                foreach ( var itr in this.stack) itr.Dispose();
                this.stack.Clear();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                while (this.stack.Count > 0)
                {
                    var itr = stack.Peek();
                    if (needMoveNext && !itr.MoveNext()) return false;
                    needMoveNext = false;

                    var r = itr.Current;
                    if (r == null) return false;
                    if (r.Records.Count > 0)
                    {
                        itr = new RecordIterator(itr.Current, -1, forward);
                        stack.Push(itr);
                    }
                    while(true)
                    {
                        bool ok = itr.MoveNext();
                        if (ok) return true;
                        this.stack.Pop();
                        if (this.stack.Count == 0)
                            return false;
                        itr = stack.Peek();                        
                    }
                }
                return false;                
            }

            public void Reset()
            {
                while (this.stack.Count > 1)
                {
                    var itr = this.stack.Pop();
                    itr.Dispose();
                }
                if (this.stack.Count == 1)
                    this.stack.Peek().Reset();
                needMoveNext = true;
            }

            public void Push(BaseRecord tn)
            {
                var queue = new Stack<RecordIterator>();
                for (; tn != null && tn.Parent != null; tn = tn.Parent)
                    queue.Push(new RecordIterator(tn, forward));
                if (this.stack == null || this.stack.Count == 0)
                {
                    this.stack = new Stack<RecordIterator>(queue);
                }
                else
                {
                    foreach (var itm in queue) this.stack.Push(itm);
                }                
            }
        }
        #endregion
        
        #region class RecordIterator
        class RecordIterator : IEnumerator<BaseRecord>
        {
            BaseRecord parent;
            int current;
            bool forward;

            public RecordIterator(BaseRecord rec, bool forward)
            {
                this.parent = rec.Parent;
                this.current = parent.Records.IndexOf(rec);
                this.forward = forward;
            }

            public RecordIterator(BaseRecord parent, int index, bool forward)
            {
                this.parent = parent;
                this.current = (index != -1) ? index : (forward ? -1 : parent.Records.Count);
                this.forward = forward;
            }

            public BaseRecord Current
            {
                get 
                {
                    if (parent == null || current < 0 || current >= parent.Records.Count)
                        return null;
                    return parent.Records[current] as BaseRecord;
                }
            }


            public void Dispose()
            {
                current = -1;
                this.parent = null;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (forward) ++current;
                else --current;
                return !(parent == null || current < 0 || current >= parent.Records.Count);
            }

            public void Reset()
            {
                if (parent != null)
                {
                    if (forward) current = -1;
                    else current = parent.Records.Count;
                }
            }
        }
        #endregion

        internal BaseRecord IncrementalSearch(BaseRecord tn, bool first, bool forward, bool wrapAround, Predicate<BaseRecord> searchFunc)
        {
            using (RecursiveRecordIterator itr = new RecursiveRecordIterator(tn, forward))
            {
                BaseRecord startNode = null;
                System.Diagnostics.Debug.Assert(tn.Equals(itr.Current));
                bool keep = first;
                do
                {
                    do
                    {
                        tn = itr.Current;
                        if (keep && searchFunc(tn))
                            return tn;
                        keep = true;
                        if (startNode == null)
                            startNode = tn;
                        else if (startNode.Equals(tn))
                            return null;
                    } while (itr.MoveNext());

                    if (!wrapAround)
                        break;
                    itr.Push(PluginList.All.Records[0] as BaseRecord);
                    itr.Reset();
                } while (itr.MoveNext());
            }
            return null;
        }

        class SearchSettings
        {
            public SearchType type;
            public BaseRecord startNode;
            public string text;
            public string rectype;
            public bool first;
            public bool partial;
            public bool forward;
            public bool wrapAround;
            public Predicate<BaseRecord> updateFunc;

            public SearchSettings()
            {
                this.type = SearchType.EditorID;
                this.startNode = null;
                this.text = null;
                this.first = true;
                this.partial = true;
                this.forward = true;
                this.wrapAround = true;
                this.updateFunc = null;
                this.rectype = null;
            }
            public SearchSettings(SearchType type, BaseRecord tn, string text, bool first, bool partial, bool forward, bool wrapAround, Predicate<BaseRecord> updateFunc)
            {
                this.type = type;
                this.startNode = tn;
                this.text = text;
                this.first = first;
                this.partial = partial;
                this.forward = forward;
                this.wrapAround = wrapAround;
                this.updateFunc = updateFunc;
                this.rectype = null;
            }
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
        private BaseRecord PerformSearch(SearchSettings ctx)
        {
            Predicate<BaseRecord> searchFunction = null;

            if (ctx.type == SearchType.FormID)
            {
                if (string.IsNullOrEmpty(ctx.text))
                    return null;

                uint searchID;
                if (!uint.TryParse(ctx.text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out searchID))
                {
                    MessageBox.Show("Invalid FormID");
                    return null;
                }
                searchFunction = (BaseRecord node) =>
                {
                    var rec = node as Record;
                    if (ctx.updateFunc != null && ctx.updateFunc(node)) return true;
                    return (rec != null) ? rec.FormID == searchID : false;
                };
            }
            else if (ctx.type == SearchType.EditorID || ctx.type == SearchType.TypeEditorIdSearch)
            {
                if (ctx.type == SearchType.TypeEditorIdSearch && string.IsNullOrEmpty(ctx.rectype))
                    return null;
                if (ctx.type == SearchType.EditorID && string.IsNullOrEmpty(ctx.text))
                    return null;

                string searchString = string.IsNullOrEmpty(ctx.text) ? null : ctx.text.ToLowerInvariant();
                searchFunction = (BaseRecord node) =>
                {
                    var rec = node as Record;
                    if (rec != null)
                    {
                        bool typeOk = true;
                        if (ctx.type == SearchType.TypeEditorIdSearch)
                            typeOk = !string.IsNullOrEmpty(rec.Name) && string.Compare(rec.Name, ctx.rectype, true) == 0;
                        if (typeOk)
                        {
                            if (string.IsNullOrEmpty(searchString))
                            {
                                return true;
                            }
                            else if (ctx.partial)
                            {
                                var val = rec.DescriptiveName.ToLowerInvariant();
                                if (val.Contains(searchString))
                                    return true;
                            }
                            else
                            {
                                var val = rec.DescriptiveName.ToLowerInvariant().Substring(2, rec.DescriptiveName.Length - 3);
                                if (val == searchString)
                                    return true;
                            }
                        }
                    }
                    if (ctx.updateFunc != null && ctx.updateFunc(node)) return true;
                    return false;
                };
            }
            else if (ctx.type == SearchType.FullSearch || ctx.type == SearchType.TypeFullSearch)
            {
                if (ctx.type == SearchType.TypeFullSearch && string.IsNullOrEmpty(ctx.rectype))
                    return null;
                if (ctx.type == SearchType.FullSearch && string.IsNullOrEmpty(ctx.text))
                    return null;
                string searchString = ctx.text.ToLowerInvariant();
                searchFunction = (BaseRecord node) =>
                {
                    var rec = node as Record;
                    if (rec != null)
                    {
                        bool typeOk = true;
                        if (ctx.type == SearchType.TypeFullSearch)
                            typeOk = !string.IsNullOrEmpty(rec.Name) && string.Compare(rec.Name, ctx.rectype, true) == 0;
                        if (typeOk)
                        {
                            foreach (SubRecord sr in rec.SubRecords)
                            {
                                var val = sr.GetStrData();
                                if (!string.IsNullOrEmpty(val))
                                {
                                    val = val.ToLowerInvariant();
                                    if ((ctx.partial && val.Contains(searchString)) || (val == searchString))
                                        return true;
                                }
                            }
                        }
                    }
                    if (ctx.updateFunc != null && ctx.updateFunc(node)) return true;
                    return false;
                };
            }
            else if (ctx.type == SearchType.FormIDRef) // Back reference form id search
            {
                if (string.IsNullOrEmpty(ctx.text))
                    return null;

                uint searchID;
                if (!uint.TryParse(ctx.text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out searchID))
                {
                    MessageBox.Show("Invalid FormID");
                    return null;
                }
                searchFunction = (BaseRecord node) =>
                {
                    var rec = node as Record;
                    if (rec != null)
                    {
                        rec.MatchRecordStructureToRecord();
                        foreach (var sr in rec.SubRecords)
                        {
                            foreach (var elem in rec.EnumerateElements(sr) )
                            {
                                var es = elem.Structure;
                                if (es != null && es.type == ElementValueType.FormID)
                                {
                                    if (searchID == TypeConverter.h2i(elem.Data))
                                        return true;
                                }                                    
                            }
                        }
                    }
                    if (ctx.updateFunc != null && ctx.updateFunc(node)) return true;
                    return false;
                };
            }
            return IncrementalSearch(ctx.startNode, ctx.first, ctx.forward, ctx.wrapAround, searchFunction);
        }

        #region Non-Conforming Records
        private void findNonconformingRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripIncrInvalidRec.Visible = !toolStripIncrInvalidRec.Visible;
            if (toolStripIncrInvalidRec.Visible)
            {
                toolStripIncrInvalidRec.Focus();
                toolStripIncrInvalidRecNext.Select();
            }
        }

        private void BackgroundNonConformingRecordIncrementalSearch(BaseRecord tn, bool forward, bool wrapAround)
        {
            float totalNodes = PluginList.All.Enumerate(x => x != null).Count();
            if (totalNodes == 0)
            {
                toolStripIncrInvalidRecStatus.Text = "No Plugins Loaded";
                toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Maroon;
                if (!TESVSnip.Properties.Settings.Default.NoWindowsSounds)
                    System.Media.SystemSounds.Beep.Play();
                return;
            }
            tn = PluginTree.SelectedRecord ?? PluginTree.TopRecord ?? null;

            int prevCount = 0;
            float currentCount = 0.0f;
            BaseRecord foundNode = null;
            toolStripIncrInvalidRecStatus.Text = "";

            Predicate<BaseRecord> searchFunc = (BaseRecord n) =>
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
                            PluginTree.SelectedRecord = foundNode;
                        }
                        else
                        {
                            toolStripIncrInvalidRecStatus.Text = "No Invalid Records Found";
                            toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Maroon;
                            if (!TESVSnip.Properties.Settings.Default.NoWindowsSounds)
                                System.Media.SystemSounds.Beep.Play();
                        }
                    }
                }
            );
        }

        internal bool findNonConformingRecordIncremental(BaseRecord tn, bool forward, bool wrapAround)
        {
            var node = IncrementalSearch(tn, false, forward, wrapAround, new Predicate<BaseRecord>(IsNonConformingRecord));
            if (node != null)
                PluginTree.SelectedRecord = node;
            return node != null;
        }

        private bool IsNonConformingRecord(BaseRecord tn)
        {
            if (tn is Record)
            {
                var r = tn as Record;
                return (r != null && !r.MatchRecordStructureToRecord());
            }
            return false;
        }

        #endregion


        #region Increment Record Search

        private void toolStripIncrFindNext_Click(object sender, EventArgs e)
        {
            BackgroundIncrementalSearch(PluginTree.SelectedRecord, true);
        }


        private void BackgroundIncrementalSearch(BaseRecord start, bool forward)
        {
            float totalNodes = PluginList.All.Enumerate(x => x != null).Count();
            if (totalNodes == 0)
            {
                toolStripIncrInvalidRecStatus.Text = "No Plugins Loaded";
                toolStripIncrInvalidRecStatus.ForeColor = System.Drawing.Color.Maroon;
                if (!TESVSnip.Properties.Settings.Default.NoWindowsSounds)
                    System.Media.SystemSounds.Beep.Play();
                return;
            }
            int prevCount = 0;
            float currentCount = 0.0f;
            BaseRecord foundNode = null;
            toolStripIncrFindStatus.Text = "";

            // Grab selected node before searching as it can only be accessed from UI thread
            if (start == null) start = PluginTree.SelectedRecord != null ? PluginTree.SelectedRecord : PluginTree.TopRecord;

            Predicate<BaseRecord> updateFunc = (BaseRecord n) =>
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

            var searchContext = new SearchSettings();

            var item = toolStripIncrFindType.SelectedItem as ComboHelper<SearchType, string>;
            searchContext.startNode = start;
            searchContext.type = item.Key;
            searchContext.text = toolStripIncrFindText.Text;
            searchContext.partial = !toolStripIncrFindExact.Checked;
            searchContext.wrapAround = toolStripIncrFindWrapAround.Checked;
            searchContext.first = toolStripIncrFindText.Tag == null ? true : (bool)toolStripIncrFindText.Tag;
            searchContext.rectype = toolStripIncrFindTypeFilter.SelectedItem as string;
            searchContext.forward = forward;
            searchContext.updateFunc = updateFunc;

            // exclude null text searches except for when type is specified
            if (searchContext.type != SearchType.TypeEditorIdSearch && string.IsNullOrEmpty(searchContext.text))
            {
                if (!TESVSnip.Properties.Settings.Default.NoWindowsSounds)
                    System.Media.SystemSounds.Beep.Play();
                toolStripIncrFind.Focus();
                toolStripIncrFindText.Select();
                toolStripIncrFindText.Focus();
                return;
            }

            StartBackgroundWork(() => { foundNode = PerformSearch(searchContext); }
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
                            PluginTree.SelectedRecord = foundNode;
                            toolStripIncrFindText.Tag = false;
                        }
                        else
                        {
                            toolStripIncrFindText.Tag = true;

                            toolStripIncrFindStatus.Text = "No Matches Found";
                            toolStripIncrFindStatus.ForeColor = System.Drawing.Color.Maroon;
                            if (!TESVSnip.Properties.Settings.Default.NoWindowsSounds)
                                System.Media.SystemSounds.Beep.Play();
                        }
                        toolStripIncrFind.Focus();
                        toolStripIncrFindText.Select();
                        toolStripIncrFindText.Focus();                        
                    }
                }
            );
        }

        private void toolStripIncrFindPrev_Click(object sender, EventArgs e)
        {
            BackgroundIncrementalSearch(PluginTree.SelectedRecord, false);
        }

        private void toolStripIncrFindRestart_Click(object sender, EventArgs e)
        {
            ResetSearch();
            BackgroundIncrementalSearch(PluginTree.TopRecord, true);
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
                BackgroundIncrementalSearch(PluginTree.SelectedRecord, !e.Shift);
            }
        }

        void ResetSearch()
        {
            // use tag to indicate text changed and therefore reset the search
            toolStripIncrFindText.Tag = true;
            toolStripIncrFindStatus.Text = "";
        }
        private void toolStripIncrFindText_TextChanged(object sender, EventArgs e)
        {
            ResetSearch();
        }
        private void toolStripIncrFindTypeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetSearch();
        }


        #endregion


    }
}