using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TESVSnip.Controls
{
    class CustomTreeView : System.Windows.Forms.TreeView
    {
        int contextMenuSet = -1;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x210: //WM_PARENTNOTIFY
                    contextMenuSet = 1;
                    break;
                case 0x21:  //WM_MOUSEACTIVATE
                    contextMenuSet++;
                    break;
                case 0x7b:  //WM_CONTEXTMENU
                    if (contextMenuSet == 1) // ignore mouse activate
                        if (OnContextMenuKey != null) 
                            OnContextMenuKey(this, EventArgs.Empty);
                    break;
            }
        }
        public event EventHandler OnContextMenuKey;
    }
}
