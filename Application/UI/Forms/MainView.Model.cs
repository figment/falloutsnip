using System.IO;
using FalloutSnip.Domain.Data;
using FalloutSnip.Properties;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace FalloutSnip.UI.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Media;
    using System.Windows.Forms;

    using Domain.Data.Structure;
    using FalloutSnip.Domain.Model;
    using FalloutSnip.Framework;

    using Settings = FalloutSnip.Properties.Settings;

    /// <summary>
    /// This file contains the incremental search related functionality for the main form.
    /// </summary>
    internal partial class MainView
    {

        private string LookupFormIDI(uint id)
        {
            return LookupFormIDI(Selection, id);
        }

        private string LookupFormIDI(SelectionContext context, uint id)
        {
            if (context != null && context.Record != null)
            {
                var p = GetPluginFromNode(context.Record);
                if (p != null)
                {
                    p.LookupFormID(id);
                }
            }

            return "No selection";
        }

        private string LookupFormStrings(uint id)
        {
            if (Selection != null && Selection.Record != null)
            {
                var p = GetPluginFromNode(Selection.Record);
                if (p != null)
                {
                    return p.LookupFormStrings(id);
                }
            }

            return null;
        }



        private Plugin GetPluginFromNode(BaseRecord node)
        {
            var tn = node;
            if (tn is Plugin)
            {
                return (Plugin)tn;
            }

            while (!(tn is Plugin) && tn != null)
            {
                tn = tn.Parent;
            }

            if (tn != null)
            {
                return tn as Plugin;
            }

            return new Plugin();
        }

        private Record GetRecordByID(uint id)
        {
            if (Selection != null && Selection.Record != null)
            {
                var p = GetPluginFromNode(Selection.Record);
                if (p != null)
                {
                    return p.GetRecordByID(id);
                }
            }

            return null;
        }

        private SelectionContext GetSelectedContext()
        {
            return Selection;

            // context.Record = this.parentRecord
            // context.SubRecord = GetSelectedSubrecord();
        }

        private SubRecord GetSelectedSubrecord()
        {
            return SubrecordList.GetSelectedSubrecord();
        }

        private IEnumerable<SubRecord> GetSelectedSubrecords()
        {
            return SubrecordList.GetSelectedSubrecords();
        }

    }
}
