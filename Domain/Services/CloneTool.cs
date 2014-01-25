using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TESVSnip.Domain.Data.RecordStructure;
using TESVSnip.Domain.Model;
using TESVSnip.Framework;
using TESVSnip.Framework.Collections;

namespace TESVSnip.Domain.Services
{
    /// <summary>
    /// Helper class for cloning records.  
    ///   Keeps track of masters used by various plugins before adding
    /// </summary>
    public class CloneTool
    {
        private static readonly HashSet<string> LooseGroups = new HashSet<string>(RecordLayout.LooseGroups);
        private static readonly HashSet<string> NoNewCopyTypes = new HashSet<string>(RecordLayout.NoNewCopyTypes);
        

        private readonly Dictionary<Plugin, PluginInfo> pluginMap = new Dictionary<Plugin, PluginInfo>();
        private readonly Plugin plugin;
        private readonly OrderedDictionary<string, string> masters = new OrderedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        class PluginInfo
        {
            public string[] Masters = new string[0];
            public readonly Dictionary<uint, uint> References = new Dictionary<uint, uint>();
        }

        public CloneTool(Plugin destPlugin, bool bOverride = true)
        {
            this.plugin = destPlugin;
            this.Override = bOverride;
        }

        bool Override { get; set; }

        /// <summary>
        /// Search through records and determine what masters to add prior to replacement
        /// </summary>
        /// <param name="records"></param>
        void PrepMasters(IEnumerable<BaseRecord> records)
        {
            // ensure masters are added
            bool changed = false;
            foreach (var key in plugin.GetMasters())
                this.masters.Add(key, key);
            var recordList = records.SelectMany(x => x.Enumerate()).OfType<Record>().ToList();

            // use 0xFF as temporary master placeholder
            uint newid = (plugin.GetNewFormID(increment: false) & 0x00FFFFFF) | 0xFF000000;

            // build list of records getting new ids.  So we know if need to masters for references
            if (!Override)
            {
                foreach (var sn in recordList.Where(rec => !NoNewCopyTypes.Contains(rec.Name))) // skip loose references
                {
                    var p = sn.GetPlugin();
                    PluginInfo info;
                    if (!pluginMap.TryGetValue(p, out info))
                        pluginMap.Add(p, info = new PluginInfo() { Masters = p.GetMasters() });
                    info.References[sn.FormID] = newid++; // not an override
                }
            }


            foreach (var rec in recordList)
            {
                var p = rec.GetPlugin();
                PluginInfo info;
                if (!pluginMap.TryGetValue(p, out info))
                    pluginMap.Add(p, info = new PluginInfo() { Masters = p.GetMasters() });

                // now handle child references
                rec.MatchRecordStructureToRecord();
                var ids = rec.SubRecords.SelectMany(sr => sr.EnumerateElements())
                    .Where(elem => elem.Structure != null && elem.Structure.type == ElementValueType.FormID)
                    .Select(elem => TypeConverter.h2i(elem.Data)).Distinct().ToList();
                ids.Add(rec.FormID);
                ids.Sort(); // processing in sort order should keep master orders slightly more sane

                foreach (var id in ids)
                {
                    if (info.References.ContainsKey(id))
                        continue;

                    var idx = id >> 24;
                    var masterName = idx >= info.Masters.Length ? p.Name : info.Masters[idx];
                    if (!masters.ContainsKey(masterName))
                    {
                        plugin.AddMaster(masterName);
                        this.masters.Add(masterName, masterName);
                        changed = true;
                    }

                    var newidx = this.masters.FindIndex(masterName);
                    if (newidx < 0) newidx = 0xFF;
                    info.References[id] = (id & 0x00FFFFFF) | (uint)(newidx << 24);
                }
            }

            // fix up the new references after all masters have been added
            var masterId = (uint)(this.masters.Count << 24);
            foreach (var info in this.pluginMap.Values)
            {
                foreach (
                    var kvp in info.References.Where(kvp => ((kvp.Value & 0xFF000000) == 0xFF000000)).ToArray())
                {
                    info.References[kvp.Key] = (kvp.Value & 0x00FFFFFF) | masterId;
                }
            }

            plugin.UpdateNextFormID(newid & 0x00FFFFFF);
            if (changed) PluginList.FixMasters();
        }

        public static Plugin GetPluginFromNode(BaseRecord node)
        {
            BaseRecord tn = node;
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

            return null;
        }

        BaseRecord Clone(BaseRecord record, bool bRecursive)
        {
            var p = GetPluginFromNode(record);
            PluginInfo info;
            if (!pluginMap.TryGetValue(p, out info))
                pluginMap.Add(p, info = new PluginInfo() { Masters = p.GetMasters() });

            var result = record.Clone(recursive: bRecursive);
            foreach (var rec in result.Enumerate().OfType<Record>())
            {
                uint newid;
                if (info.References.TryGetValue(rec.FormID, out newid) && rec.FormID != newid)
                    rec.FormID = newid;

                // now handle child references
                rec.MatchRecordStructureToRecord();
                foreach (var elem in rec.SubRecords
                    .SelectMany(sr => sr.EnumerateElements())
                    .Where(elem => elem.Structure != null && elem.Structure.type == ElementValueType.FormID)
                    )
                {
                    var value = elem.GetValue<uint>();
                    if (info.References.TryGetValue(value, out newid) && value != newid)
                        elem.AssignValue<uint>(newid);
                }
            }
            return result;
        }
        IEnumerable<BaseRecord> Clone(IEnumerable<BaseRecord> records, bool bRecursive)
        {
            return records.Select(record => Clone(record, bRecursive)).ToList();
        }

        /// <summary>
        /// Actually perform the copy
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public int CopyRecordsTo(BaseRecord[] src)
        {
            int count = 0;

            PrepMasters(src);

            var dstRec = this.Clone(src.Where(x => !LooseGroups.Contains(x.Name)), true).ToArray();
            if (dstRec.All(x => x is Record))
            {
                // put records into appropriate groups
                var groups = plugin.Records.OfType<GroupRecord>().ToList();
                var lookup = dstRec.GroupBy(r => r.Name)
                    .Select(g => new { key = g.Key, value = g.ToArray() })
                    .ToLookup(k => k.key, v => v.value);
                foreach (var kvp in lookup)
                {
                    if (LooseGroups.Contains(kvp.Key))
                    {
                        plugin.AddRecords(dstRec);
                    }
                    else
                    {
                        var gr = groups.FirstOrDefault(x => x.ContentsType == kvp.Key);
                        if (gr == null)
                        {
                            gr = new GroupRecord(kvp.Key);
                            plugin.AddRecord(gr);
                        }

                        foreach (var list in kvp)
                        {
                            gr.AddRecords(list);
                        }
                    }
                }
            }
            else
            {
                plugin.AddRecords(dstRec);
            }

            // handle loose groups by creating copy of parent groups
            foreach (var srcRec in src.Where(x => LooseGroups.Contains(x.Name)))
            {
                var dstnodes = new Stack<BaseRecord>();

                dstnodes.Push(this.Clone(srcRec, true));
                for (var n = srcRec.Parent; n is GroupRecord; n = n.Parent)
                {
                    dstnodes.Push(n.Clone(recursive: false));
                }

                var par = plugin as IGroupRecord;
                foreach (var baseRecord in dstnodes)
                {
                    if (par == null)
                    {
                        break;
                    }

                    if (baseRecord is GroupRecord)
                    {
                        var gr = baseRecord as GroupRecord;
                        var pargr = par.Records.OfType<GroupRecord>().FirstOrDefault(x => x.IsEquivalent(gr));
                        if (pargr != null)
                        {
                            par = pargr;
                            continue;
                        }
                    }

                    par.AddRecord(baseRecord);
                    par = baseRecord as IGroupRecord;
                }

                count += dstnodes.Count;
            }
            return count;
        }
    }
}
