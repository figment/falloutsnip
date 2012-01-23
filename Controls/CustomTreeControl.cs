using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TESVSnip.Controls
{
    class CustomTreeView : BrightIdeasSoftware.TreeListView
    {
        int _contextMenuSet = -1;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x210: //WM_PARENTNOTIFY
                    _contextMenuSet = 1;
                    break;
                case 0x21:  //WM_MOUSEACTIVATE
                    _contextMenuSet++;
                    break;
                case 0x7b:  //WM_CONTEXTMENU
                    if (_contextMenuSet == 1) // ignore mouse activate
                        if (OnContextMenuKey != null)
                            OnContextMenuKey(this, EventArgs.Empty);
                    break;
            }
        }
        public event EventHandler OnContextMenuKey;

        //protected new TreeNode SelectedNode { get { return base.SelectedNode; } set { base.SelectedNode = value; } }

        public TESVSnip.BaseRecord SelectedRecord
        {
            get
            {
                return base.SelectedObject as TESVSnip.BaseRecord;
            }
            set
            {
                if (value != null)
                    this.EnsureModelVisible(value);
                base.SelectObject(value, true);
            }
        }

        public IEnumerable<BaseRecord> SelectedRecords
        {
            get
            {
                return this.SelectedObjects.OfType<BaseRecord>();
            }
            set
            {
                foreach (var r in value)
                    this.EnsureModelVisible(r);
                base.SelectObjects(value.ToList());
            }
        }


        public override void Expand(object model)
        {
            var rec = model as IRecord;
            var parent = rec != null ? rec.Parent : null;
            if (parent != null) Expand(parent);
            base.Expand(model);
        }

        public override void EnsureModelVisible(object modelObject)
        {
            if (modelObject == null || modelObject is PluginList)
                return;

            var rec = modelObject as IRecord;
            var parent = rec != null ? rec.Parent : null;
            if (parent != null) Expand(parent);
            base.EnsureModelVisible(modelObject);
        }

        /// <summary>
        /// Remove any sorting and revert to the given order of the model objects
        /// </summary>
        public override void Unsort()
        {
            this.ShowGroups = false;
            this.PrimarySortColumn = null;
            this.PrimarySortOrder = SortOrder.None;
            this.TreeModel.Unsort();
            this.Roots = _baseRoots;
            RebuildAll(true);
            this.ShowSortIndicator(this.LastSortColumn, this.LastSortOrder);
        }
        object[] _baseRoots = new object[0];

        public override System.Collections.IEnumerable Roots
        {
            get { return base.Roots; }
            set
            {
                _baseRoots = value.OfType<object>().ToArray();
                base.Roots = _baseRoots;
            }
        }
    }

    //class CustomTreeView : BrightIdeasSoftware.TreeListView
    //{

    //}
}
