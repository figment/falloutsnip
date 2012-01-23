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
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public sealed class Plugin : BaseRecord, IDeserializationCallback, IGroupRecord
    {
        [Persistable]
        readonly List<Rec> records = new List<Rec>();

        public override IList Records { get { return records; } }

        private string FileName { get; set; }
        private string StringsFolder { get; set; }
        private System.IO.FileSystemWatcher fsw;

        public bool StringsDirty { get; set; }
        public LocalizedStringDict Strings = new LocalizedStringDict();
        public LocalizedStringDict ILStrings = new LocalizedStringDict();
        public LocalizedStringDict DLStrings = new LocalizedStringDict();

        // References to masters
        public Plugin[] Masters = new Plugin[0];
        // Number of masters each master holds. (used to calculate internal id)
        public uint[] Fixups = new uint[0];
        // Hash tables for quick FormID lookups
        public readonly Dictionary<uint, Record> FormIDLookup = new Dictionary<uint, Record>();

        // Whether the file was filtered on load
        public bool Filtered = false;

        BaseRecord parent = null;
        public override BaseRecord Parent { get { return parent; } internal set { parent = value; } }

        public override long Size
        {
            get { long size = 0; foreach (Rec rec in Records) size += rec.Size2; return size; }
        }
        public override long Size2 { get { return Size; } }

        public override bool DeleteRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) return false;
            bool result = records.Remove(r);
            if (result) r.Parent = null;
            InvalidateCache();
            FireRecordDeleted(this, r);
            FireRecordListUpdate(this, this);
            return result;
        }

        public override void AddRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Plugins can only hold Groups or Records.");
            r.Parent = this;
            records.Add(r);
            InvalidateCache();
            FireRecordListUpdate(this, this);
        }
        public override void InsertRecord(int idx, BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Plugins can only hold Groups or Records.");
            r.Parent = this;
            if (idx < 0 || idx > this.records.Count)
                idx = this.records.Count;
            records.Insert(idx, r);
            InvalidateCache();
            FireRecordListUpdate(this, this);
        }


        public override void AddRecords(IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to add was not of the correct type.\nPlugins can only hold records or other groups.");
            }
            foreach (var r in br) r.Parent = this;
            records.AddRange(br.OfType<Rec>());
            FireRecordListUpdate(this, this);
            InvalidateCache();
        }
        public override bool DeleteRecords(IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
                throw new TESParserException("Record to delete was not of the correct type.\nPlugins can only hold records or other groups.");
            var ok = false;
            foreach (Rec r in from Rec r in br where records.Remove(r) select r)
            {
                ok = true;
                r.Parent = null;
                FireRecordDeleted(this, r);
            }
            FireRecordListUpdate(this, this);
            InvalidateCache();
            return ok;
        }
        public override void InsertRecords(int index, IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
                throw new TESParserException("Record to add was not of the correct type.\nPlugins can only hold records or other groups.");
            records.InsertRange(index, br.OfType<Rec>());
            FireRecordListUpdate(this, this);
            InvalidateCache();
        }

        public override int IndexOf(BaseRecord br)
        {
            return this.records.IndexOf(br as Rec);
        }

        public void Clear()
        {
            foreach (var r in records)
                r.Parent = null;
            records.Clear();
        }

        public override IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            if (!match(this)) yield break;
            foreach (BaseRecord r in this.Records)
                foreach (var itm in r.Enumerate(match))
                    yield return itm;
        }

        public override bool While(Predicate<BaseRecord> action)
        {
            if (!base.While(action)) return false;
            foreach (BaseRecord r in this.Records)
                if (!r.While(action))
                    return false;
            return true;
        }
        public override void ForEach(Action<BaseRecord> action)
        {
            base.ForEach(action);
            foreach (BaseRecord r in this.Records) r.ForEach(action);
        }

        public bool TryGetRecordByID(uint key, out Record value)
        {
            RebuildCache();
            return this.FormIDLookup.TryGetValue(key, out value);
        }

        private void RebuildCache()
        {
            if (this.FormIDLookup.Count == 0)
            {
                this.ForEach(br => { Record r = br as Record; if (r != null) { this.FormIDLookup[r.FormID] = r; } });
            }
        }

        /// <summary>
        /// Invalidate the FormID Cache.
        /// </summary>
        public void InvalidateCache()
        {
            this.FormIDLookup.Clear();
        }

        private void LoadPluginData(BinaryReader br, bool headerOnly, string[] recFilter)
        {
            bool oldHoldUpdates = HoldUpdates;
            try
            {
                string s;
                uint recsize;
                bool IsOblivion = false;

                this.Filtered = (recFilter != null && recFilter.Length > 0);

                HoldUpdates = true;
                Decompressor.Init();

                s = ReadRecName(br);
                if (s != "TES4") throw new Exception("File is not a valid TES4 plugin (Missing TES4 record)");
                br.BaseStream.Position = 20;
                s = ReadRecName(br);
                if (s == "HEDR")
                {
                    IsOblivion = true;
                }
                else
                {
                    s = ReadRecName(br);
                    if (s != "HEDR") throw new Exception("File is not a valid TES4 plugin (Missing HEDR subrecord in the TES4 record)");
                }
                br.BaseStream.Position = 4;
                recsize = br.ReadUInt32();
                AddRecord(new Record("TES4", recsize, br, IsOblivion));
                if (!headerOnly)
                {
                    while (br.PeekChar() != -1)
                    {
#if DEBUG
                        long szPos = br.BaseStream.Position;
#endif
                        s = ReadRecName(br);
                        recsize = br.ReadUInt32();
#if DEBUG
                        System.Diagnostics.Trace.TraceInformation("{0} {1}", s, recsize);
#endif
                        if (s == "GRUP")
                        {
                            AddRecord(new GroupRecord(recsize, br, IsOblivion, recFilter, false));
                        }
                        else
                        {
                            bool skip = recFilter != null && Array.IndexOf(recFilter, s) >= 0;
                            if (skip)
                            {
                                long size = (recsize + (IsOblivion ? 8 : 12));
                                if ((br.ReadUInt32() & 0x00040000) > 0) size += 4;
                                br.BaseStream.Position += size;// just read past the data
                            }
                            else
                                AddRecord(new Record(s, recsize, br, IsOblivion));
                        }
#if DEBUG
                        System.Diagnostics.Debug.Assert((br.BaseStream.Position - szPos) == recsize);
#endif
                    }
                }
            }
            finally
            {
                HoldUpdates = oldHoldUpdates;
                FireRecordListUpdate(this, this);
                Decompressor.Close();
            }
        }

        public static bool GetIsEsm(string FilePath)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(FilePath));
            try
            {
                string s = ReadRecName(br);
                if (s != "TES4") return false;
                br.ReadInt32();
                return (br.ReadInt32() & 1) != 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                br.Close();
            }
        }

        Plugin(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public Plugin(byte[] data, string name)
        {
            Name = name;
            BinaryReader br = new BinaryReader(new MemoryStream(data));
            try
            {
                LoadPluginData(br, false, null);

                this.FileName = System.IO.Path.GetFileNameWithoutExtension(name);
            }
            finally
            {
                br.Close();
            }
        }
        internal Plugin(string FilePath, bool headerOnly) : this(FilePath, headerOnly, null) { }

        internal Plugin(string FilePath, bool headerOnly, string[] recFilter)
        {
            Name = Path.GetFileName(FilePath);
            try
            {
                FileInfo fi = new FileInfo(FilePath);
                using (BinaryReader br = new BinaryReader(fi.OpenRead()))
                {
                    LoadPluginData(br, headerOnly, recFilter);
                }
                this.FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
                if (!headerOnly)
                {
                    this.StringsFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), "Strings");
                }
                ReloadStrings();
            }
            catch
            {
            }
        }

        public void ReloadStrings()
        {
            if (string.IsNullOrEmpty(this.StringsFolder) || string.IsNullOrEmpty(this.FileName) || !Directory.Exists(this.StringsFolder))
                return;

            string locName = global::TESVSnip.Properties.Settings.Default.LocalizationName;

            if (Directory.GetFiles(this.StringsFolder, this.FileName + "_" + locName + "*").Count() == 0)
            {
                if (locName == "English")
                    return;
                locName = "English";
            }

            string prefix = System.IO.Path.Combine(this.StringsFolder, this.FileName);
            prefix += "_" + global::TESVSnip.Properties.Settings.Default.LocalizationName;

            System.Text.Encoding enc = TESVSnip.Encoding.CP1252;
            TESVSnip.FontLangInfo fontInfo;
            if (TESVSnip.Encoding.TryGetFontInfo(locName, out fontInfo))
            {
                if (fontInfo.CodePage != 1252)
                    enc = System.Text.Encoding.GetEncoding(fontInfo.CodePage);
            }

            Strings = LoadPluginStrings(enc, LocalizedStringFormat.Base, prefix + ".STRINGS");
            ILStrings = LoadPluginStrings(enc, LocalizedStringFormat.IL, prefix + ".ILSTRINGS");
            DLStrings = LoadPluginStrings(enc, LocalizedStringFormat.DL, prefix + ".DLSTRINGS");

            if (global::TESVSnip.Properties.Settings.Default.MonitorStringsFolderForChanges)
            {
                if (fsw == null)
                {
                    fsw = new System.IO.FileSystemWatcher(this.StringsFolder, this.FileName + "*");
                    fsw.EnableRaisingEvents = true;
                    fsw.Changed += delegate(object sender, FileSystemEventArgs e)
                    {
                        ReloadStrings();
                    };
                }
            }
            else
            {
                if (fsw != null)
                    fsw.Dispose();
                fsw = null;
            }
        }

        public Plugin()
        {
            Name = "New plugin";
        }

        public override string GetDesc()
        {
            return "[Skyrim plugin]" + Environment.NewLine +
                "Filename: " + Name + Environment.NewLine +
                "File size: " + Size + Environment.NewLine +
                "Records: " + records.Count;
        }

        public byte[] Save()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            SaveData(bw);
            byte[] b = ms.ToArray();
            bw.Close();
            return b;
        }

        internal void Save(string FilePath)
        {
            bool existed = false;
            DateTime timestamp = DateTime.Now;
            if (File.Exists(FilePath))
            {
                timestamp = new FileInfo(FilePath).LastWriteTime;
                existed = true;
                File.Delete(FilePath);
            }
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(FilePath));
            try
            {
                SaveData(bw);
                Name = Path.GetFileName(FilePath);
            }
            finally
            {
                bw.Close();
            }
            try
            {
                if (existed)
                {
                    new FileInfo(FilePath).LastWriteTime = timestamp;
                }
            }
            catch { }

            //if (StringsDirty)
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
            if (tes4 != null && (tes4.Flags1 & 0x80) != 0)
            {
                if (global::TESVSnip.Properties.Settings.Default.SaveStringsFiles)
                {
                    string prefix = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), "Strings"), System.IO.Path.GetFileNameWithoutExtension(FilePath));
                    prefix += "_" + global::TESVSnip.Properties.Settings.Default.LocalizationName;

                    System.Text.Encoding enc = TESVSnip.Encoding.CP1252;
                    TESVSnip.FontLangInfo fontInfo;
                    if (TESVSnip.Encoding.TryGetFontInfo(global::TESVSnip.Properties.Settings.Default.LocalizationName, out fontInfo))
                    {
                        if (fontInfo.CodePage != 1252)
                            enc = System.Text.Encoding.GetEncoding(fontInfo.CodePage);
                    }
                    SavePluginStrings(enc, LocalizedStringFormat.Base, Strings, prefix + ".STRINGS");
                    SavePluginStrings(enc, LocalizedStringFormat.IL, ILStrings, prefix + ".ILSTRINGS");
                    SavePluginStrings(enc, LocalizedStringFormat.DL, DLStrings, prefix + ".DLSTRINGS");
                }
            }
            StringsDirty = false;
        }

        internal override void SaveData(BinaryWriter bw)
        {
            Compressor.Init();
            foreach (Rec r in Records) r.SaveData(bw);
            Compressor.Close();
        }

        internal override List<string> GetIDs(bool lower)
        {
            List<string> list = new List<string>();
            foreach (Rec r in Records) list.AddRange(r.GetIDs(lower));
            return list;
        }

        public override BaseRecord Clone()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        private LocalizedStringDict LoadPluginStrings(System.Text.Encoding encoding, LocalizedStringFormat format, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                        return LoadPluginStrings(encoding, format, reader);
                }
            }
            catch { }
            return new LocalizedStringDict();
        }

        private LocalizedStringDict LoadPluginStrings(System.Text.Encoding encoding, LocalizedStringFormat format, BinaryReader reader)
        {
            if (encoding == null)
                encoding = TESVSnip.Encoding.CP1252;
            LocalizedStringDict dict = new LocalizedStringDict();
            int length = reader.ReadInt32();
            int size = reader.ReadInt32(); // size of data section
            var list = new List<Pair<uint, uint>>();
            for (uint i = 0; i < length; ++i)
            {
                uint id = reader.ReadUInt32();
                uint off = reader.ReadUInt32();
                list.Add(new Pair<uint, uint>(id, off));
            }
            long offset = reader.BaseStream.Position;
            byte[] data = new byte[size];
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data, 0, size, true, false))
            {
                byte[] buffer = new byte[65536];
                int left = size;
                while (left > 0)
                {
                    int read = Math.Min(left, (int)buffer.Length);
                    int nread = reader.BaseStream.Read(buffer, 0, read);
                    if (nread == 0) break;
                    stream.Write(buffer, 0, nread);
                    left -= nread;
                }
            }
            foreach (var kvp in list)
            {
                int start = (int)kvp.Value;
                int len = 0;
                switch (format)
                {
                    case LocalizedStringFormat.Base:
                        while (data[start + len] != 0) ++len;
                        break;

                    case LocalizedStringFormat.DL:
                    case LocalizedStringFormat.IL:
                        len = BitConverter.ToInt32(data, start) - 1;
                        start = start + sizeof(int);
                        if (start + len > data.Length)
                            len = data.Length - start;
                        if (len < 0) len = 0;
                        break;
                }
                string str = encoding.GetString(data, start, len);
                dict.Add(kvp.Key, str);
            }
            return dict;
        }

        private void SavePluginStrings(System.Text.Encoding enc, LocalizedStringFormat format, LocalizedStringDict strings, string path)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
                    SavePluginStrings(enc, format, strings, writer);
            }
            catch { }
        }

        private void SavePluginStrings(System.Text.Encoding enc, LocalizedStringFormat format, LocalizedStringDict strings, BinaryWriter writer)
        {
            if (enc == null) enc = TESVSnip.Encoding.CP1252;

            var list = new List<Pair<uint, uint>>();

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            using (System.IO.BinaryWriter memWriter = new System.IO.BinaryWriter(stream))
            {
                foreach (KeyValuePair<uint, string> kvp in strings)
                {
                    list.Add(new Pair<uint, uint>(kvp.Key, (uint)stream.Position));
                    byte[] data = enc.GetBytes(kvp.Value);
                    switch (format)
                    {
                        case LocalizedStringFormat.Base:
                            memWriter.Write(data, 0, data.Length);
                            memWriter.Write((byte)0);
                            break;

                        case LocalizedStringFormat.DL:
                        case LocalizedStringFormat.IL:
                            memWriter.Write(data.Length + 1);
                            memWriter.Write(data, 0, data.Length);
                            memWriter.Write((byte)0);
                            break;
                    }
                }
                writer.Write(strings.Count);
                writer.Write((int)stream.Length);
                foreach (var item in list)
                {
                    writer.Write(item.Key);
                    writer.Write(item.Value);
                }

                stream.Position = 0;
                byte[] buffer = new byte[65536];
                int left = (int)stream.Length;
                while (left > 0)
                {
                    int read = Math.Min(left, (int)buffer.Length);
                    int nread = stream.Read(buffer, 0, read);
                    if (nread == 0) break;
                    writer.Write(buffer, 0, nread);
                    left -= nread;
                }
            }
        }

        public bool AddMaster(string masterName)
        {
            Record brcTES4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
            if (brcTES4 == null)
                throw new ApplicationException("Plugin lacks a valid TES4 record. Cannot continue.");
            // find existing if already present
            foreach (var mast in brcTES4.SubRecords.Where(x => x.Name == "MAST"))
            {
                var path = mast.GetStrData();
                if (string.Compare(path, masterName, true) == 0)
                    return false;
            }
            int idx = brcTES4.SubRecords.IndexOf(brcTES4.SubRecords.FirstOrDefault(x => x.Name == "INTV"));
            if (idx < 0) idx = brcTES4.SubRecords.Count;

            SubRecord sbrMaster = new SubRecord();
            sbrMaster = new SubRecord();
            sbrMaster.Name = "DATA";
            sbrMaster.SetData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            brcTES4.InsertRecord(idx, sbrMaster);

            sbrMaster = new SubRecord();
            sbrMaster.Name = "MAST";
            Int32 intCount = Encoding.CP1252.GetByteCount(masterName);
            byte[] bteData = new byte[intCount + 1];
            Array.Copy(Encoding.CP1252.GetBytes(masterName), bteData, intCount);
            sbrMaster.SetData(bteData);
            brcTES4.InsertRecord(idx, sbrMaster);

            // Fix Masters
            //  Update IDs for current record to be +1
            return true;
        }
        public string[] GetMasters()
        {
            Record brcTES4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name == "TES4");
            if (brcTES4 == null)
                return new string[0];
            return brcTES4.SubRecords.Where(x => x.Name == "MAST").Select(x => x.GetStrData()).ToArray();
        }

        #region External references
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugins"></param>
        /// <remarks>
        /// Rules:  order 
        /// </remarks>
        public void UpdateReferences(IList<Plugin> plugins)
        {
            var masters = GetMasters();
            Masters = new Plugin[masters.Length + 1];
            Fixups = new uint[masters.Length + 1];
            for (int i = 0; i < masters.Length; ++i)
            {
                var master = plugins.FirstOrDefault(x => string.Compare(masters[i], x.Name, true) == 0);
                Masters[i] = master;
                Fixups[i] = (uint)((master != null) ? master.GetMasters().Length : 0);
            }
            Masters[masters.Length] = this;
            Fixups[masters.Length] = (uint)masters.Length;
            InvalidateCache();
        }


        /// <summary>
        /// Lookup FormID by index.  Search via defined masters
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal string LookupFormID(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > this.Masters.Length)
                return "FormID was invalid";

            Record r;
            // First search self for exact match
            if (this.TryGetRecordByID(id, out r))
                return r.DescriptiveName;
            id &= 0xffffff;
            if (pluginid < Masters.Length && Masters[pluginid] != null)
            {
                // find the reference master and search it for reference
                //   TODO: in theory another master could override the first master
                var p = Masters[pluginid];
                id |= (Fixups[pluginid] << 24);
                if (p.TryGetRecordByID(id, out r))
                    return r.DescriptiveName;
                return "No match";
            }
            else
            {
                return "Master not loaded";
            }
        }

        internal Record GetRecordByID(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > Masters.Length)
                return null;
            Record r;
            // first check self for exact match
            if (this.TryGetRecordByID(id, out r))
                return r;
            id &= 0xffffff;
            if (pluginid >= Masters.Length || Masters[pluginid] == null)
                return null;
            // find the reference master and search it for reference
            //   TODO: in theory another master could override the first master
            id += Fixups[pluginid] << 24;
            if (Masters[pluginid].TryGetRecordByID(id, out r))
                return r;
            return null;
        }

        internal string LookupFormIDS(string sid)
        {
            uint id;
            if (!uint.TryParse(sid, System.Globalization.NumberStyles.AllowHexSpecifier, null, out id))
                return "FormID was invalid";
            return LookupFormID(id);
        }

        internal string LookupFormStrings(uint id)
        {
            string value = default(string);
            foreach (var plugin in Masters)
            {
                if (plugin == null) continue;

                if (plugin.Strings.TryGetValue(id, out value))
                    break;
                if (plugin.DLStrings.TryGetValue(id, out value))
                    break;
                if (plugin.ILStrings.TryGetValue(id, out value))
                    break;
            }
            return value;
        }

        internal IEnumerable<KeyValuePair<uint, Record>> EnumerateRecords(string type)
        {
            Dictionary<uint, string> list = new Dictionary<uint, string>();
            // search each master reference.  Override any 
            for (int i = 0; i < Masters.Length - 1; i++)
            {
                if (Masters[i] == null) continue; // missing master

                uint match = Fixups[i];
                match <<= 24;
                uint mask = (uint)i << 24;
                // This enumerate misses any records that are children of masters
                foreach (var r in Masters[i].Enumerate(r =>
                {
                    if (r is Record)
                    {
                        if ((type == null || r.Name == type) && (((Record)r).FormID & 0xFF000000) == match)
                            return true;
                    }
                    else if (r is GroupRecord)
                    {
                        var gr = (GroupRecord)r;
                        if (gr.groupType != 0 || gr.ContentsType == type)
                            return true;
                    }
                    else if (r is Plugin)
                    {
                        return true;
                    }
                    return false;
                })
                )
                {
                    if (r is Record)
                    {
                        var r2 = r as Record;
                        yield return new KeyValuePair<uint, Record>((r2.FormID & 0xffffff) | mask, r2);
                    }
                }
            }
            // finally add records of self in to the list
            foreach (var r in this.Enumerate(r =>
            {
                if (r is Record)
                {
                    if (type == null || r.Name == type)
                        return true;
                }
                else if (r is GroupRecord)
                {
                    var gr = (GroupRecord)r;
                    if (gr.groupType != 0 || type == null || gr.ContentsType == type)
                        return true;
                }
                else if (r is Plugin)
                {
                    return true;
                }
                return false;
            })
            )
            {
                if (r is Record)
                {
                    var r2 = r as Record;
                    yield return new KeyValuePair<uint, Record>(r2.FormID, r2);
                }
            }
        }

        #endregion

        #region IDeserializationCallback Members

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            foreach (BaseRecord rec in this.Records)
                rec.Parent = this;
        }

        #endregion
    }
}
