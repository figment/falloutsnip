using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TESVSnip.Properties;

namespace TESVSnip
{
    /// <summary>
    /// This file contains the miscellaneous spells for the main form
    /// </summary>

    internal partial class MainView
    {
        private readonly string[] SanitizeOrder = new string[] {
            "GMST", "KYWD", "LCRT", "AACT", "TXST", "GLOB", "CLAS", "FACT", "HDPT", "HAIR", "EYES", "RACE", "SOUN", "ASPC", "MGEF", 
            "SCPT", "LTEX", "ENCH", "SPEL", "SCRL", "ACTI", "TACT", "ARMO", "BOOK", "CONT", "DOOR", "INGR", "LIGH", "MISC", "APPA", 
            "STAT", "SCOL", "MSTT", "PWAT", "GRAS", "TREE", "CLDC", "FLOR", "FURN", "WEAP", "AMMO", "NPC_", "LVLN", "KEYM", "ALCH", 
            "IDLM", "COBJ", "PROJ", "HAZD", "SLGM", "LVLI", "WTHR", "CLMT", "SPGD", "RFCT", "REGN", "NAVI", "CELL", "WRLD", "DIAL", 
            "QUST", "IDLE", "PACK", "CSTY", "LSCR", "LVSP", "ANIO", "WATR", "EFSH", "EXPL", "DEBR", "IMGS", "IMAD", "FLST", "PERK",
            "BPTD", "ADDN", "AVIF", "CAMS", "CPTH", "VTYP", "MATT", "IPCT", "IPDS", "ARMA", "ECZN", "LCTN", "MESG", "RGDL", "DOBJ", 
            "LGTM", "MUSC", "FSTP", "FSTS", "SMBN", "SMQN", "SMEN", "DLBR", "MUST", "DLVW", "WOOP", "SHOU", "EQUP", "RELA", "SCEN", 
            "ASTP", "OTFT", "ARTO", "MATO", "MOVT", "SNDR", "DUAL", "SNCT", "SOPM", "COLL", "CLFM", "REVB"
        };
        private int sanitizeCountRecords(Rec r)
        {
            if (r is Record) return 1;
            else
            {
                int i = 1;
                foreach (Rec r2 in ((GroupRecord)r).Records) i += sanitizeCountRecords(r2);
                return i;
            }
        }
        private void sanitizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelected, Resources.ErrorText);
                return;
            }
            var p = GetPluginFromNode(PluginTree.SelectedRecord);

            var hdr = p.Records.OfType<Rec>().FirstOrDefault(x => x.Name == "TES4");
            if (hdr == null)
            {
                MessageBox.Show(Resources.PluginLacksAValidTes4RecordCannotContinue);
                return;
            }

            // performance update to prevent lists from updating currently selected record
            bool oldHoldUpdates = BaseRecord.HoldUpdates;
            try
            {
                BaseRecord.HoldUpdates = true;

                var toParse = new Queue<BaseRecord>(p.Records.OfType<BaseRecord>().Where(x => !x.Equals(hdr)));
                p.Clear();
                p.AddRecord(hdr);

                var groups = new Dictionary<string, GroupRecord>();

                foreach (string s in SanitizeOrder)
                {
                    var gr = new GroupRecord(s);
                    p.AddRecord(gr);
                    groups[s] = gr;
                }

                bool looseGroupsWarning = false;
                bool unknownRecordsWarning = false;
                while (toParse.Count > 0)
                {
                    var r = toParse.Dequeue();
                    if (r is GroupRecord)
                    {
                        var gr = (GroupRecord) r;
                        if (gr.ContentsType == "CELL" || gr.ContentsType == "WRLD" || gr.ContentsType == "DIAL")
                        {
                            var gr2 = groups[gr.ContentsType];
                            foreach (BaseRecord r2 in gr.Records) gr2.AddRecord(r2);
                            gr.Clear();
                        }
                        else
                        {
                            foreach (BaseRecord r2 in gr.Records) toParse.Enqueue(r2);
                            gr.Clear();
                        }
                    }
                    else if (r is Record)
                    {
                        var r2 = (Record) r;
                        if (r2.Name == "CELL" || r2.Name == "WRLD" || r2.Name == "REFR" || r2.Name == "ACRE" ||
                            r2.Name == "ACHR" || r2.Name == "NAVM" || r2.Name == "DIAL" || r2.Name == "INFO")
                        {
                            looseGroupsWarning = true;
                            p.AddRecord(r2);
                        }
                        else
                        {
                            if (groups.ContainsKey(r2.Name)) groups[r2.Name].AddRecord(r2);
                            else
                            {
                                unknownRecordsWarning = true;
                                p.AddRecord(r2);
                            }
                        }
                    }
                }

                foreach (GroupRecord gr2 in groups.Values)
                {
                    if (gr2.Records.Count == 0) p.DeleteRecord(gr2);
                }

                if (looseGroupsWarning)
                {
                    MessageBox.Show(Resources.CannotSanitizeLooseGroups, Resources.WarningText);
                }
                if (unknownRecordsWarning)
                {
                    MessageBox.Show(Resources.CannotSanitizeUnknownRecords, Resources.WarningText);
                }
                p.InvalidateCache();

                int reccount = -1 + p.Records.Cast<Rec>().Sum(r => sanitizeCountRecords(r));
                var tes4 = p.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
                if (tes4 != null)
                {
                    if (tes4.SubRecords.Count > 0 && tes4.SubRecords[0].Name == "HEDR" && tes4.SubRecords[0].Size >= 8)
                    {
                        byte[] data = tes4.SubRecords[0].GetData();
                        byte[] reccountbytes = TypeConverter.si2h(reccount);
                        for (int i = 0; i < 4; i++) data[4 + i] = reccountbytes[i];
                        tes4.SubRecords[0].SetData(data);
                    }
                }
            }
            finally
            {
                BaseRecord.HoldUpdates = oldHoldUpdates;
                PluginTree.RebuildObjects();
            }
        }

        private void StripEDIDspublic(Rec r)
        {
            if (r is Record)
            {
                Record r2 = (Record)r;
                if (r2.Name != "GMST" && r2.SubRecords.Count > 0 && r2.SubRecords[0].Name == "EDID") r2.DeleteRecord(r2.SubRecords[0]);
                for (int i = 0; i < r2.SubRecords.Count; i++)
                {
                    if (r2.SubRecords[i].Name == "SCTX") r2.SubRecords.RemoveAt(i--);
                }
            }
            else
            {
                foreach (Rec r2 in ((GroupRecord)r).Records) StripEDIDspublic(r2);
            }
        }
        private void stripEDIDsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelected, Resources.ErrorText);
                return;
            }
            if (MessageBox.Show(Resources.GeneralSpellWarningInquiry, Resources.WarningText, MessageBoxButtons.YesNo) != DialogResult.Yes) 
                return;
            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            foreach (Rec r in p.Records) StripEDIDspublic(r);
            PluginTree.RebuildObjects();
        }

        private bool findDuplicateFormIDs(BaseRecord tn, Dictionary<uint, Record> ids)
        {
            if (tn is Record)
            {
                Record r2 = (Record)tn;
                if (ids.ContainsKey(r2.FormID))
                {
                    PluginTree.SelectedRecord = tn;
                    MessageBox.Show("Record duplicates " + ((Record)ids[r2.FormID]).DescriptiveName);
                    ids.Clear();
                    return true;
                }
                else
                {
                    ids.Add(r2.FormID, r2);
                }
            }
            else
            {
                foreach (BaseRecord tn2 in tn.Records) findDuplicateFormIDs(tn2, ids);
            }
            return false;
        }
        private void findDuplicatedFormIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            var ids = new Dictionary<uint, Record>();
            foreach (BaseRecord tn2 in p.Records)
            {
                if (findDuplicateFormIDs(tn2, ids)) return;
            }
            ids.Clear();
        }

        private void DumpEdidsInternal(Rec r, System.IO.StreamWriter sw)
        {
            if (r is Record)
            {
                Record r2 = (Record)r;
                if (r2.SubRecords.Count > 0 && r2.SubRecords[0].Name == "EDID") sw.WriteLine(r2.SubRecords[0].GetStrData());
            }
            else
            {
                foreach (Rec r2 in ((GroupRecord)r).Records) DumpEdidsInternal(r2, sw);
            }
        }
        private void dumpEDIDListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;
            if (PluginTree.SelectedRecord is Record)
            {
                MessageBox.Show("Spell works only on plugins or record groups", "Error");
                return;
            }
            if (SaveEdidListDialog.ShowDialog() != DialogResult.OK) return;
            System.IO.StreamWriter sw = new System.IO.StreamWriter(SaveEdidListDialog.FileName);
            if (PluginTree.SelectedRecord is Plugin)
            {
                foreach (Rec r in ((Plugin)PluginTree.SelectedRecord).Records)
                {
                    DumpEdidsInternal(r, sw);
                }
            }
            else
            {
                DumpEdidsInternal((GroupRecord)PluginTree.SelectedRecord, sw);
            }
            sw.Close();
        }

        private void cleanRecurse(Rec r, uint match, uint mask, Dictionary<uint, Record> lookup)
        {
            Record r2 = r as Record;
            if (r2 != null)
            {
                if ((r2.FormID & 0xff000000) == match)
                {
                    if (r2.Name != "REFR" && r2.Name != "ACHR" && r2.Name != "NAVM" && r2.Name != "INFO") lookup[(r2.FormID & 0xffffff) | mask] = r2;
                }
            }
            else
            {
                foreach (Rec r3 in ((GroupRecord)r).Records)
                {
                    cleanRecurse(r3, match, mask, lookup);
                }
            }
        }
        private bool cleanRecurse2(Rec r, ref int count, Dictionary<uint, Record> lookup)
        {
            Record r2 = r as Record;
            if (r2 != null)
            {
                if (lookup.ContainsKey(r2.FormID))
                {
                    Record r3 = lookup[r2.FormID];
                    if (r2.Name == r3.Name && r2.Size == r3.Size && r2.SubRecords.Count == r3.SubRecords.Count && r2.Flags1 == r3.Flags1 &&
                        r2.Flags2 == r3.Flags2 && r2.Flags3 == r3.Flags3)
                    {
                        for (int i = 0; i < r2.SubRecords.Count; i++)
                        {
                            if (r2.SubRecords[i].Name != r3.SubRecords[i].Name || r2.SubRecords[i].Size != r3.SubRecords[i].Size) return false;
                            byte[] data1 = r2.SubRecords[i].GetReadonlyData(), data2 = r3.SubRecords[i].GetReadonlyData();
                            for (int j = 0; j < data1.Length; j++) if (data1[j] != data2[j]) return false;
                        }
                        return true;
                    }
                }
            }
            else
            {
                GroupRecord gr = (GroupRecord)r;
                for (int i = 0; i < gr.Records.Count; i++)
                {
                    if (cleanRecurse2(gr.Records[i] as Rec, ref count, lookup))
                    {
                        count++;
                        gr.Records.RemoveAt(i--);
                    }
                }
            }
            return false;
        }
        private void cleanEspToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null)
            {
                MessageBox.Show(Resources.NoPluginSelected, "Error");
                return;
            }
            if (MessageBox.Show("This may delete records from the esp.\nAre you sure you wish to continue?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            FixMasters();
            var plugin = GetPluginFromNode(PluginTree.SelectedRecord);

            Dictionary<uint, Record> lookup = new Dictionary<uint, Record>();
            bool missingMasters = false;
            for (int i = 0; i < plugin.Masters.Length - 1; i++)
            {
                if (plugin.Masters[i] == null)
                {
                    missingMasters = true;
                    continue;
                }
                var tes4 = plugin.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
                if (plugin.Masters[i].Records.Count < 2 || tes4 == null) continue;
                uint match = (uint)plugin.Masters.Count(x => x.Name == "MAST");
                match <<= 24;
                uint mask = (uint)i << 24;
                for (int j = 1; j < plugin.Masters[i].Records.Count; j++) 
                    cleanRecurse(plugin.Masters[i].Records[j] as Rec, match, mask, lookup);
            }

            if (missingMasters)
            {
                MessageBox.Show("One or more dependencies are not loaded, and will be ignored.", "Warning");
            }

            int count = 0;
            for (int j = 1; j < plugin.Masters[plugin.Masters.Length - 1].Records.Count; j++)
                cleanRecurse2(plugin.Masters[plugin.Masters.Length - 1].Records[j] as Rec, ref count, lookup);
            if (count == 0) MessageBox.Show("No records removed");
            else MessageBox.Show("" + count + " records removed");


            PluginTree.Refresh();
        }


        private void compileScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixMasters();
            var r = PluginTree.SelectedRecord as Record;
            var plugin = GetPluginFromNode(r);
            if (plugin == null || plugin.Parent == null) return;

            string errors;
            if (Selection.SelectedSubrecord && Selection.Record.Name != "SCPT")
            {
                var sr = subrecordPanel.SubRecord;
                if (sr == null) return;
                if (sr.Name != "SCTX")
                {
                    MessageBox.Show("You need to select a SCPT record or SCTX subrecord to compile", "Error");
                    return;
                }
                ScriptCompiler.ScriptCompiler.Setup(plugin.Masters);
                Record r2;
                if (!ScriptCompiler.ScriptCompiler.CompileResultScript(sr, out r2, out errors))
                {
                    MessageBox.Show("There were compilation errors:\n" + errors);
                }
                else
                {
                    var srs = r.SubRecords;
                    int i = srs.IndexOf(sr);
                    if (i >= 0)
                    {
                        while (i > 0 && (srs[i - 1].Name == "SCDA" || srs[i - 1].Name == "SCHR"))
                            srs.RemoveAt(--i);
                        while (i < srs.Count && (srs[i].Name == "SCTX" || srs[i].Name == "SLSD" || srs[i].Name == "SCVR" || srs[i].Name == "SCRO" || srs[i].Name == "SCRV"))
                            srs.RemoveAt(i);
                        srs.InsertRange(i, r2.SubRecords);
                        RebuildSelection();
                        PluginTree.RefreshObject(r);
                    }
                }
                return;
            }
            if (r == null || (r.Name != "SCPT"))
            {
                MessageBox.Show("You need to select a SCPT record or SCTX subrecord to compile", "Error");
                return;
            }

            ScriptCompiler.ScriptCompiler.Setup(plugin.Masters);
            if (!ScriptCompiler.ScriptCompiler.Compile(r, out errors))
            {
                MessageBox.Show("There were compilation errors:\n" + errors);
            }
            else
            {
                RebuildSelection();
                PluginTree.RebuildObjects();
            }
        }

        private void compileAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errors;
            string thingy = "";
            int count = 0, failed = 0, failed2 = 0;
            int size;

            FixMasters();
            var plugin = GetPluginFromNode(PluginTree.SelectedRecord);
            if (plugin == null) return;

            ScriptCompiler.ScriptCompiler.Setup(plugin.Masters);
            foreach (Rec rec in plugin.Records)
            {
                GroupRecord gr = rec as GroupRecord;
                if (gr == null) continue;
                if (gr.ContentsType == "SCPT")
                {
                    foreach (Record r in gr.Records)
                    {
                        count++;
                        size = 0;
                        foreach (SubRecord sr in r.SubRecords)
                        {
                            if (sr.Name == "SCDA")
                            {
                                size = (int)sr.Size;
                                break;
                            }
                        }
                        if (!ScriptCompiler.ScriptCompiler.Compile(r, out errors))
                        {
                            failed++;
                            thingy += r.DescriptiveName + Environment.NewLine + errors + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            foreach (SubRecord sr in r.SubRecords)
                            {
                                if (sr.Name == "SCDA")
                                {
                                    if (sr.Size != size)
                                    {
                                        failed2++;
                                        thingy += r.DescriptiveName + Environment.NewLine + "Size changed from " + size + " to " + sr.Size + Environment.NewLine + Environment.NewLine;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            thingy += Environment.NewLine + Environment.NewLine + "Final results: " + count + "/" + failed + "/" + failed2;
            System.IO.File.WriteAllText("script results.txt", thingy);
        }

        private void generateLLXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixMasters();
            var plugin = GetPluginFromNode(PluginTree.SelectedRecord);
            if (plugin == null) return;
            var p = plugin;

            {
                Record r;
                if (p.Records.Count > 0) r = p.Records[0] as Record;
                else r = null;
                bool firstwasfallout = false;
                if (r != null && r.Name == "TES4")
                {
                    foreach (SubRecord sr in r.SubRecords)
                    {
                        if (sr.Name == "MAST")
                        {
                            if (sr.GetStrData().ToLowerInvariant() == "skyrim.esm") firstwasfallout = true;
                            break;
                        }
                    }
                }
                if (!firstwasfallout)
                {
                    MessageBox.Show("Only works on plugin's whose first master is Skyrim.esm", "Error");
                    return;
                }
            }

            uint mask = (uint)(plugin.Masters.Length - 1) << 24;
            Queue<Rec> recs = new Queue<Rec>(p.Records.OfType<Rec>());

            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            System.Text.StringBuilder sb3 = new System.Text.StringBuilder();
            while (recs.Count > 0)
            {
                Rec rec = recs.Dequeue();
                if (rec is GroupRecord)
                {
                    GroupRecord gr = (GroupRecord)rec;
                    if (gr.ContentsType == "LVLI" || gr.ContentsType == "LVLN" || gr.ContentsType == "LVLC")
                    {
                        for (int i = 0; i < gr.Records.Count; i++) recs.Enqueue(gr.Records[i] as Rec);
                    }
                }
                else
                {
                    Record r = (Record)rec;
                    if ((r.FormID & 0xff000000) != 0) continue;
                    switch (r.Name)
                    {
                        case "LVLI":
                            for (int i = 0; i < r.SubRecords.Count; i++)
                            {
                                if (r.SubRecords[i].Name == "LVLO")
                                {
                                    if (r.SubRecords[i].Size != 12) continue;
                                    byte[] data = r.SubRecords[i].GetReadonlyData();
                                    uint formid = TypeConverter.h2i(data[4], data[5], data[6], data[7]);
                                    if ((formid & 0xff000000) != mask) continue;
                                    sb3.Append("      <Element level=\"" + TypeConverter.h2ss(data[0], data[1]) + "\" formid=\"" +
                                        (formid & 0xffffff).ToString("X6") + "\" count=\"" + TypeConverter.h2ss(data[8], data[9]) + "\" ");
                                    if (i < r.SubRecords.Count - 1 && r.SubRecords[i + 1].Name == "COED" && r.SubRecords[i + 1].Size == 12)
                                    {
                                        i++;
                                        data = r.SubRecords[i].GetReadonlyData();
                                        sb3.Append(" coed1=\"" + TypeConverter.h2i(data[0], data[1], data[2], data[3]) + "\" coed2=\"" +
                                            TypeConverter.h2i(data[4], data[5], data[6], data[7]) + "\" coed3=\"" +
                                            TypeConverter.h2i(data[8], data[9], data[10], data[11]) + "\" ");
                                    }
                                    sb3.AppendLine("/>");
                                }
                            }
                            if (sb3.Length > 0)
                            {
                                sb2.AppendLine("    <LVLI formid=\"" + r.FormID.ToString("X6") + "\">");
                                sb2.Append(sb3.ToString());
                                sb2.AppendLine("    </LVLI>");
                            }
                            sb3.Length = 0;
                            break;
                        case "LVLN":
                            for (int i = 0; i < r.SubRecords.Count; i++)
                            {
                                if (r.SubRecords[i].Name == "LVLO")
                                {
                                    if (r.SubRecords[i].Size != 12) continue;
                                    byte[] data = r.SubRecords[i].GetReadonlyData();
                                    uint formid = TypeConverter.h2i(data[4], data[5], data[6], data[7]);
                                    if ((formid & 0xff000000) != mask) continue;
                                    sb3.AppendLine("      <Element level=\"" + TypeConverter.h2ss(data[0], data[1]) + "\" formid=\"" +
                                        (formid & 0xffffff).ToString("X6") + "\" count=\"" + TypeConverter.h2ss(data[8], data[9]) + "\" />");
                                }
                            }
                            if (sb3.Length > 0)
                            {
                                sb2.AppendLine("    <LVLN formid=\"" + r.FormID.ToString("X6") + "\">");
                                sb2.Append(sb3.ToString());
                                sb2.AppendLine("    </LVLN>");
                            }
                            sb3.Length = 0;
                            break;
                        case "LVLC":
                            for (int i = 0; i < r.SubRecords.Count; i++)
                            {
                                if (r.SubRecords[i].Name == "LVLO")
                                {
                                    if (r.SubRecords[i].Size != 12) continue;
                                    byte[] data = r.SubRecords[i].GetReadonlyData();
                                    uint formid = TypeConverter.h2i(data[4], data[5], data[6], data[7]);
                                    if ((formid & 0xff000000) != mask) continue;
                                    sb3.AppendLine("      <Element level=\"" + TypeConverter.h2ss(data[0], data[1]) + "\" formid=\"" +
                                        (formid & 0xffffff).ToString("X6") + "\" count=\"" + TypeConverter.h2ss(data[8], data[9]) + "\" />");
                                }
                            }
                            if (sb3.Length > 0)
                            {
                                sb2.AppendLine("    <LVLC formid=\"" + r.FormID.ToString("X6") + "\">");
                                sb2.Append(sb3.ToString());
                                sb2.AppendLine("    </LVLC>");
                            }
                            sb3.Length = 0;
                            break;
                    }
                }
            }
            if (sb2.Length > 0)
            {
                System.Text.StringBuilder sb1 = new System.Text.StringBuilder();
                sb1.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sb1.AppendLine("<Plugin>");
                sb1.AppendLine("  <MergedLists>");
                sb1.Append(sb2);
                sb1.AppendLine("  </MergedLists>");
                sb1.AppendLine("</Plugin>");
                System.IO.File.WriteAllText(System.IO.Path.ChangeExtension("data\\" + p.Name, ".xml"), sb1.ToString());
            }
            else MessageBox.Show("No compatible leveled lists found");
        }

        private void makeEsmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;

            var p = GetPluginFromNode(PluginTree.SelectedRecord);
            var tes4 = p.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
            if (tes4 != null)
            {
                MessageBox.Show("Plugin has no TES4 record");
                return;
            }
            if ((tes4.Flags1 & 1) == 1)
            {
                MessageBox.Show("Plugin is already a master file");
                return;
            }
            tes4.Flags1 |= 1;

            SaveModDialog.FileName = System.IO.Path.ChangeExtension(p.Name, ".esm");
            if (SaveModDialog.ShowDialog() == DialogResult.OK)
            {
                p.Save(SaveModDialog.FileName);
            }
        }

        private void martigensToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginTree.SelectedRecord == null) return;

            var p = GetPluginFromNode(PluginTree.SelectedRecord);

            Form f = new Form();
            f.Text = "Replace";
            TextBox tb = new TextBox();
            f.Controls.Add(tb);
            tb.Dock = DockStyle.Fill;
            tb.AcceptsReturn = true;
            tb.Multiline = true;
            tb.ScrollBars = ScrollBars.Vertical;
            f.ShowDialog();

            string replace = tb.Text;
            f.Text = "Replace with";
            tb.Text = "";
            f.ShowDialog();
            string with = tb.Text;

            Queue<Rec> recs = new Queue<Rec>(p.Records.OfType<Rec>());
            while (recs.Count > 0)
            {
                if (recs.Peek() is GroupRecord)
                {
                    GroupRecord gr = (GroupRecord)recs.Dequeue();
                    for (int i = 0; i < gr.Records.Count; i++) recs.Enqueue(gr.Records[i] as Rec);
                }
                else
                {
                    Record r = (Record)recs.Dequeue();
                    foreach (SubRecord sr in r.SubRecords)
                    {
                        if (sr.Name != "SCTX") continue;
                        string text = sr.GetStrData();
                        int upto = 0;
                        bool replaced = false;
                        while (true)
                        {
                            int i = text.IndexOf(replace, upto, StringComparison.InvariantCultureIgnoreCase);
                            if (i == -1) break;
                            text = text.Remove(i, replace.Length).Insert(i, with);
                            upto = i + with.Length;
                            replaced = true;
                        }
                        if (replaced)
                        {
                            sr.SetStrData(text, false);
                        }
                    }
                }
            }
        }

        #region Reorder Subrecords
        // try to reorder subrecords to match the structure file.
        private void reorderSubrecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rec = Selection.Record as Record;
            if (rec == null || RecordStructure.Records == null) return;
            if (!RecordStructure.Records.ContainsKey(rec.Name)) return;

            SubrecordStructure[] sss = RecordStructure.Records[rec.Name].subrecords;

            List<SubRecord> subs = new List<SubRecord>(rec.SubRecords);
            foreach (var sub in subs) sub.DetachStructure();

            List<SubRecord> newsubs = new List<SubRecord>();
            for (int ssidx = 0, sslen = 0; ssidx < sss.Length; ssidx += sslen)
            {
                SubrecordStructure ss = sss[ssidx];
                bool repeat = ss.repeat > 0;
                sslen = Math.Max(1, ss.repeat);

                bool found = false;
                do
                {
                    found = false;
                    for (int ssoff = 0; ssoff < sslen; ++ssoff)
                    {
                        ss = sss[ssidx + ssoff];
                        for (int i = 0; i < subs.Count; ++i)
                        {
                            var sr = subs[i];
                            if (sr.Name == ss.name)
                            {
                                newsubs.Add(sr);
                                subs.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }
                } while (found && repeat);
            }
            newsubs.AddRange(subs);
            rec.SubRecords.Clear();
            rec.SubRecords.AddRange(newsubs);
            RebuildSelection();
        }
        #endregion


        #region Create Record Structure XML



        private void createRecordStructureXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = GetPluginFromNode(PluginTree.SelectedRecord);

            TESVSnip.Data.RecordBuilder builder = new TESVSnip.Data.RecordBuilder();
            builder.FormLookup = new dFormIDLookupR(this.GetRecordByID);
            builder.StringLookup = new dLStringLookup(this.LookupFormStrings);
            builder.CancelAction = new Func<bool>(() => { return backgroundWorker1.CancellationPending; });
            builder.UpdateProgressAction = new Action<int>(UpdateBackgroundProgress);

            StartBackgroundWork(() => { builder.Start(p); }
                , () =>
                {
                    if (!IsBackroundProcessCanceled())
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.InitialDirectory = System.IO.Path.GetTempPath();
                            dlg.FileName = "RecordStructure.xml";
                            dlg.OverwritePrompt = false;
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(TESVSnip.Data.Records));
                                using (System.IO.StreamWriter fs = System.IO.File.CreateText(dlg.FileName))
                                {
                                    xs.Serialize(fs, builder.Complete());
                                }
                            }
                        }
                    }
                }
                );

        }
        #endregion

    }
}