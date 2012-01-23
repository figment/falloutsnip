using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using System.Drawing;
using System.Text;
using RTF;
using TESVSnip.Data;

namespace TESVSnip
{

    /// <summary>
    /// Master Plugin Handler
    /// </summary>
    class PluginList : BaseRecord, IGroupRecord
    {
        List<Plugin> pluginList = new List<Plugin>();
        static PluginList plugins = new PluginList();

        public static PluginList All
        {
            get { return plugins; }
        }

        public override BaseRecord Parent
        {
            get { return null; }
            internal set { }
        }

        public override long Size
        {
            get { return this.pluginList.Sum(x => x.Size2); }
        }

        public override long Size2
        {
            get { return this.pluginList.Sum(x => x.Size2); }
        }

        public override string GetDesc() { return "Master List"; }

        public override bool DeleteRecord(BaseRecord br)
        {
            var p = br as Plugin;
            if (p != null)
            {
                p.Parent = null;
                bool ok = this.pluginList.Remove(p);
                FireRecordListUpdate(this, this);
                return ok;
            }
            return false;
        }

        public override void AddRecord(BaseRecord br)
        {
            var p = br as Plugin;
            if (p != null)
            {
                p.Parent = this;
                this.pluginList.Add(p);
            }
            FireRecordListUpdate(this, null);
        }
        public override void InsertRecord(int idx, BaseRecord br)
        {
            var r = br as Plugin;
            r.Parent = this;
            if (idx < 0 || idx > this.pluginList.Count)
                idx = this.pluginList.Count;
            pluginList.Insert(idx, r);
            FireRecordListUpdate(this, null);
        }

        public override int IndexOf(BaseRecord br)
        {
            return this.pluginList.IndexOf(br as Plugin);
        }

        public void Clear()
        {
            this.pluginList.Clear();
        }

        public static void FixMasters()
        {
            var plugins = All.Records.OfType<Plugin>().ToArray();
            foreach (var plugin in PluginList.All.Records.OfType<Plugin>())
                plugin.UpdateReferences(plugins);
        }


        internal override List<string> GetIDs(bool lower)
        {
            throw new NotImplementedException();
        }

        internal override void SaveData(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }

        public override BaseRecord Clone()
        {
            throw new NotImplementedException();
        }

        public override IList Records
        {
            get { return pluginList; }
        }
        public override bool While(Predicate<BaseRecord> action)
        {
            foreach (BaseRecord r in this.Records)
                if (!r.While(action))
                    return false;
            return true;
        }

        public override void ForEach(Action<BaseRecord> action)
        {
            foreach (BaseRecord r in this.Records) r.ForEach(action);
        }

        public override IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            foreach (BaseRecord r in this.Records)
                foreach (var itm in r.Enumerate(match))
                    yield return itm;
        }
    }
}
