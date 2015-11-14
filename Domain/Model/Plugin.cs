using FalloutSnip.Domain.Data;
using FalloutSnip.Domain.Data.Structure;

namespace FalloutSnip.Domain.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    using FalloutSnip.Domain.Data.Strings;
    using FalloutSnip.Framework;
    using FalloutSnip.Framework.Persistence;
    using FalloutSnip.Framework.Services;

    using FalloutSnip.Domain.Services;

    [Persistable(Flags = PersistType.DeclaredOnly)]
    [Serializable]
    public sealed class Plugin : BaseRecord, IDeserializationCallback, IGroupRecord
    {
        // Hash tables for quick FormID lookups
        public readonly Dictionary<uint, Record> FormIDLookup = new Dictionary<uint, Record>();

        public LocalizedStringDict DLStrings = new LocalizedStringDict();

        // Whether the file was filtered on load
        public bool Filtered;

        public uint[] Fixups = new uint[0];

        public LocalizedStringDict ILStrings = new LocalizedStringDict();

        public Plugin[] Masters = new Plugin[0];

        public LocalizedStringDict Strings = new LocalizedStringDict();

        [Persistable]
        private readonly List<Rec> records = new List<Rec>();

        private FileSystemWatcher fsw;

        private BaseRecord parent;

        private DomainDefinition define;

        public DomainDefinition Domain { get { return define; } }

        public Plugin(byte[] data, string name)
        {
            Name = name;
            var br = new BinaryReader(new MemoryStream(data));
            try
            {
                this.LoadPluginData(br, false, null);
                this.FileName = Path.GetFileNameWithoutExtension(name);
                define = this.DetectVersion();
            }
            finally
            {
                br.Close();
            }
        }

        public Plugin()
            : this(DomainDefinition.LoadedDomains().FirstOrDefault())
        {            
        }

        public Plugin(DomainDefinition define)
        {
            this.Name = "New plugin.esp";
            this.define = define;
        }

        public Plugin(string filePath) : this(filePath, false){}

        public Plugin(string filePath, bool headerOnly) 
            : this(filePath, headerOnly, (Func<string, bool>) null) { }

        public Plugin(string filePath, string[] recExclusions)
            : this(filePath, false, CreateFilter(recExclusions)) { }

        public Plugin(string filePath, bool headerOnly, string[] recExclusions) 
            : this(filePath, headerOnly, CreateFilter(recExclusions)){}

        public Plugin(string filePath, Func<string, bool> includeFilter)
            : this(filePath, false, includeFilter) {}

        private static Func<string, bool> CreateFilter(string[] recExclusions)
        {
            if (recExclusions == null || recExclusions.Length == 0)
                return null;
            return (key) => Array.IndexOf(recExclusions, key) < 0;
        }

        public Plugin(string filePath, bool headerOnly, Func<string, bool> includeFilter )
        {
            Name = Path.GetFileName(filePath);
            PluginPath = Path.GetDirectoryName(filePath);
            var fi = new FileInfo(filePath);
            using (var br = new BinaryReader(fi.OpenRead()))
            {
                define = this.DetectVersion(br, filePath);
                br.BaseStream.Position = 0;
                this.LoadPluginData(br, headerOnly, includeFilter);
            }

            this.FileName = Path.GetFileNameWithoutExtension(filePath);
            if (!headerOnly)
            {
                this.StringsFolder = Path.Combine(Path.GetDirectoryName(filePath), "Strings");
            }

            this.ReloadStrings();
        }

        public static Plugin Load(string filePath, Func<string, bool> includeFilter)
        {
            return new Plugin(filePath, false, includeFilter);
        }

        private Plugin(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // detect version
            define = this.DetectVersion();
        }

        public override BaseRecord Parent
        {
            get
            {
                return this.parent;
            }

            internal set
            {
                this.parent = value;
            }
        }

        public override IList Records
        {
            get
            {
                return this.records;
            }
        }

        public override long Size
        {
            get
            {
                long size = 0;
                foreach (Rec rec in this.Records)
                {
                    size += rec.Size2;
                }

                return size;
            }
        }

        public override long Size2
        {
            get
            {
                return this.Size;
            }
        }

        public bool StringsDirty { get; set; }

        private string FileName { get; set; }

        private string StringsFolder { get; set; }

        public static bool GetIsEsm(string FilePath)
        {
            var br = new BinaryReader(File.OpenRead(FilePath));
            try
            {
                string s = ReadRecName(br);
                if (s != "TES4")
                {
                    return false;
                }

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

        public bool AddMaster(string masterName)
        {
            Record brcTES4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name.StartsWith("TES"));
            if (brcTES4 == null)
            {
                throw new ApplicationException("Plugin lacks a valid TES4 record. Cannot continue.");
            }

            // find existing if already present
            foreach (var mast in brcTES4.SubRecords.Where(x => x.Name == "MAST"))
            {
                var path = mast.GetStrData();
                if (string.Compare(path, masterName, true) == 0)
                {
                    return false;
                }
            }

            int idx = brcTES4.SubRecords.IndexOf(brcTES4.SubRecords.FirstOrDefault(x => x.Name == "INTV"));
            if (idx < 0)
            {
                idx = brcTES4.SubRecords.Count;
            }

            var sbrMaster = new SubRecord();
            sbrMaster.Name = "DATA";
            sbrMaster.SetData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            brcTES4.InsertRecord(idx, sbrMaster);

            sbrMaster = new SubRecord();
            sbrMaster.Name = "MAST";
            int intCount = Encoding.Instance.GetByteCount(masterName);
            var bteData = new byte[intCount + 1];
            Array.Copy(Encoding.Instance.GetBytes(masterName), bteData, intCount);
            sbrMaster.SetData(bteData);
            brcTES4.InsertRecord(idx, sbrMaster);

            int masterIdx = brcTES4.SubRecords.Count(x => x.Name == "MAST") - 1;
            // Update IDs for current record to be +1
            // Fix FormID and references now
            foreach (var rec in EnumerateRecords(null).Select(x => x.Value)
                .SkipWhile(x=>RecordLayout.NoNewCopyTypes.Contains(x.Name)))
            {
                if ((rec.FormID >> 24) >= masterIdx)
                    rec.FormID = (rec.FormID & 0x00FFFFFF) | (((rec.FormID >> 24) + 1) << 24);

                // now handle child references
                rec.MatchRecordStructureToRecord();
                foreach (var elem in rec.SubRecords
                    .SelectMany(sr => sr.EnumerateElements())
                    .Where(elem => elem.Structure != null && elem.Structure.type == ElementValueType.FormID) )
                {
                    var value = elem.GetValue<uint>();
                    if ((value >> 24) >= masterIdx)
                        elem.AssignValue<uint>((value & 0x00FFFFFF) | (((value >> 24) + 1) << 24));
                }
            }

            return true;
        }

        public override void AddRecord(BaseRecord br)
        {
            try
            {
                var r = br as Rec;
                if (r == null)
                {
                    throw new TESParserException("Record to add was not of the correct type." + Environment.NewLine + "PluginList can only hold Groups or Records.");
                }

                r.Parent = this;
                this.records.Add(r);
                this.InvalidateCache();
                FireRecordListUpdate(this, this);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public override void AddRecords(IEnumerable<BaseRecord> br)
        {
            try
            {
                if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
                {
                    throw new TESParserException("Record to add was not of the correct type.\nPlugins can only hold records or other groups.");
                }

                foreach (var r in br)
                {
                    r.Parent = this;
                }

                this.records.AddRange(br.OfType<Rec>());
                FireRecordListUpdate(this, this);
                this.InvalidateCache();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void Clear()
        {
            foreach (var r in this.records)
            {
                r.Parent = null;
            }

            this.records.Clear();
        }

        public override BaseRecord Clone()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override bool DeleteRecord(BaseRecord br)
        {
            var r = br as Rec;
            if (r == null)
            {
                return false;
            }

            bool result = this.records.Remove(r);
            if (result)
            {
                r.Parent = null;
            }

            this.InvalidateCache();
            FireRecordDeleted(this, r);
            FireRecordListUpdate(this, this);
            return result;
        }

        public override bool DeleteRecords(IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to delete was not of the correct type.\nPlugins can only hold records or other groups.");
            }

            var ok = false;
            foreach (Rec r in from Rec r in br where this.records.Remove(r) select r)
            {
                ok = true;
                r.Parent = null;
                FireRecordDeleted(this, r);
            }

            FireRecordListUpdate(this, this);
            this.InvalidateCache();
            return ok;
        }

        public override IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            if (!match(this))
            {
                yield break;
            }

            foreach (BaseRecord r in this.Records)
            {
                if (match(r))
                    yield return r;
                foreach (var itm in r.Enumerate(match))
                {
                    yield return itm;
                }
            }
        }

        public override void ForEach(Action<BaseRecord> action)
        {
            base.ForEach(action);
            foreach (BaseRecord r in this.Records)
            {
                r.ForEach(action);
            }
        }

        public string[] GetMasters()
        {
            Record brcTES4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (brcTES4 == null)
            {
                return new string[0];
            }

            return brcTES4.SubRecords.Where(x => x.Name == "MAST").Select(x => x.GetStrData()).ToArray();
        }

        public override int IndexOf(BaseRecord br)
        {
            return this.records.IndexOf(br as Rec);
        }

        public override void InsertRecord(int idx, BaseRecord br)
        {
            var r = br as Rec;
            if (r == null)
            {
                throw new TESParserException("Record to add was not of the correct type." + Environment.NewLine + "PluginList can only hold Groups or Records.");
            }

            r.Parent = this;
            if (idx < 0 || idx > this.records.Count)
            {
                idx = this.records.Count;
            }

            this.records.Insert(idx, r);
            this.InvalidateCache();
            FireRecordListUpdate(this, this);
        }

        public override void InsertRecords(int index, IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to add was not of the correct type.\nPlugins can only hold records or other groups.");
            }

            this.records.InsertRange(index, br.OfType<Rec>());
            FireRecordListUpdate(this, this);
            this.InvalidateCache();
        }

        /// <summary>
        ///   Invalidate the FormID Cache.
        /// </summary>
        public void InvalidateCache()
        {
            this.FormIDLookup.Clear();
        }

        public void ReloadStrings()
        {
            if (string.IsNullOrEmpty(this.StringsFolder) || string.IsNullOrEmpty(this.FileName) || !Directory.Exists(this.StringsFolder))
            {
                return;
            }

            string locName = FalloutSnip.Domain.Properties.Settings.Default.LocalizationName;

            if (!Directory.GetFiles(this.StringsFolder, this.FileName + "_" + locName + "*").Any())
            {
                if (locName == "English")
                {
                    return;
                }

                locName = "English";
            }

            string prefix = Path.Combine(this.StringsFolder, this.FileName);
            prefix += "_" + FalloutSnip.Domain.Properties.Settings.Default.LocalizationName;

            System.Text.Encoding enc = Encoding.Instance;
            FontLangInfo fontInfo;
            if (Encoding.TryGetFontInfo(locName, out fontInfo))
            {
                if (fontInfo.CodePage != 1252)
                {
                    enc = System.Text.Encoding.GetEncoding(fontInfo.CodePage);
                }
            }

            this.Strings = this.LoadPluginStrings(enc, LocalizedStringFormat.Base, prefix + ".STRINGS");
            this.ILStrings = this.LoadPluginStrings(enc, LocalizedStringFormat.IL, prefix + ".ILSTRINGS");
            this.DLStrings = this.LoadPluginStrings(enc, LocalizedStringFormat.DL, prefix + ".DLSTRINGS");

            if (Properties.Settings.Default.MonitorStringsFolderForChanges)
            {
                if (this.fsw == null)
                {
                    this.fsw = new FileSystemWatcher(this.StringsFolder, this.FileName + "*");
                    this.fsw.EnableRaisingEvents = true;
                    this.fsw.Changed += delegate { this.ReloadStrings(); };
                }
            }
            else
            {
                if (this.fsw != null)
                {
                    this.fsw.Dispose();
                }

                this.fsw = null;
            }
        }

        public byte[] Save()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            this.SaveData(bw);
            byte[] b = ms.ToArray();
            bw.Close();
            return b;
        }

        public bool TryGetRecordByID(uint key, out Record value)
        {
            this.RebuildCache();
            return this.FormIDLookup.TryGetValue(key, out value);
        }

        /// <summary>
        /// </summary>
        /// <param name="plugins">
        /// </param>
        /// <remarks>
        /// Rules:  order
        /// </remarks>
        public void UpdateReferences(IList<Plugin> plugins)
        {
            var masters = this.GetMasters();
            this.Masters = new Plugin[masters.Length + 1];
            this.Fixups = new uint[masters.Length + 1];
            for (int i = 0; i < masters.Length; ++i)
            {
                var master = plugins.FirstOrDefault(x => string.Compare(masters[i], x.Name, true) == 0);
                this.Masters[i] = master;
                this.Fixups[i] = (uint)((master != null) ? master.GetMasters().Length : 0);
            }

            this.Masters[masters.Length] = this;
            this.Fixups[masters.Length] = (uint)masters.Length;
            this.InvalidateCache();
        }

        public override bool While(Predicate<BaseRecord> action)
        {
            if (!base.While(action))
            {
                return false;
            }

            foreach (BaseRecord r in this.Records)
            {
                if (!r.While(action))
                {
                    return false;
                }
            }

            return true;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            foreach (BaseRecord rec in this.Records)
            {
                rec.Parent = this;
            }
        }

        public IEnumerable<KeyValuePair<uint, Record>> EnumerateRecords(string type)
        {
            var list = new Dictionary<uint, string>();

            // search each master reference.  Override any 
            for (int i = 0; i < this.Masters.Length - 1; i++)
            {
                if (this.Masters[i] == null)
                {
                    continue; // missing master
                }

                uint match = this.Fixups[i];
                match <<= 24;
                uint mask = (uint)i << 24;

                // This enumerate misses any records that are children of masters
                foreach (var r in this.Masters[i].Enumerate(
                    r =>
                    {
                        if (r is Record)
                        {
                            if ((type == null || r.Name == type) && (((Record)r).FormID & 0xFF000000) == match)
                            {
                                return true;
                            }
                        }
                        else if (r is GroupRecord)
                        {
                            var gr = (GroupRecord)r;
                            if (gr.groupType != 0 || gr.ContentsType == type)
                            {
                                return true;
                            }
                        }
                        else if (r is Plugin)
                        {
                            return true;
                        }

                        return false;
                    }))
                {
                    if (r is Record)
                    {
                        var r2 = r as Record;
                        yield return new KeyValuePair<uint, Record>((r2.FormID & 0xffffff) | mask, r2);
                    }
                }
            }

            // finally add records of self in to the list
            foreach (var r in this.Enumerate(
                r =>
                {
                    if (r is Record)
                    {
                        if (type == null || r.Name == type)
                        {
                            return true;
                        }
                    }
                    else if (r is GroupRecord)
                    {
                        var gr = (GroupRecord)r;
                        if (gr.groupType != 0 || type == null || gr.ContentsType == type)
                        {
                            return true;
                        }
                    }
                    else if (r is Plugin)
                    {
                        return true;
                    }

                    return false;
                }))
            {
                if (r is Record)
                {
                    var r2 = r as Record;
                    yield return new KeyValuePair<uint, Record>(r2.FormID, r2);
                }
            }
        }

        /// <summary>
        /// Python helper function to get list of records. 
        /// </summary>
        /// <param name="type">Type to </param>
        /// <returns>Array of Records</returns>
        public Record[] GetRecordList(string type)
        {
            return string.IsNullOrEmpty(type)
                ? EnumerateRecords(null).Select(x => x.Value).ToArray()
                : GetRecordList(new[] { type });
        }

        /// <summary>
        /// Python helper function to get list of records. 
        /// </summary>
        /// <param name="types">Collection of types</param>
        /// <returns>Array of Records</returns>
        public Record[] GetRecordList(IEnumerable types)
        {
            if (types == null)
                return EnumerateRecords(null).Select(x => x.Value).ToArray();

            var vtypes = new HashSet<string>(types.OfType<string>());
            Predicate<BaseRecord> itr = (rec) => (rec is Plugin)
                || (rec is GroupRecord && (((GroupRecord)rec).groupType != 0 || vtypes.Contains(((GroupRecord)rec).ContentsType)))
                || (rec is Record && vtypes.Contains(rec.Name));
            return Enumerate(itr).OfType<Record>().ToArray();
        }

        internal override List<string> GetIDs(bool lower)
        {
            var list = new List<string>();
            foreach (Rec r in this.Records)
            {
                list.AddRange(r.GetIDs(lower));
            }

            return list;
        }

        public uint GetNewFormID(bool increment = false)
        {
            uint formID = 0;
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (tes4 != null && tes4.SubRecords.Count > 0)
            {
                var masterCount = tes4.SubRecords.Count(x => x.Name == "MAST");
                var hedr = tes4.SubRecords[0];
                if (hedr.Name == "HEDR" && hedr.Size >= 12)
                {
                    if (hedr.TryGetValue(8, out formID) && increment)
                    {
                        hedr.TrySetValue(8, (formID & 0x00FFFFFF) + 1);
                    }
                }
                return (formID & 0x00FFFFFF) | ((uint)(masterCount << 24));
            }
            return formID;
        }

        public void UpdateNextFormID(uint newid)
        {
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (tes4 != null && tes4.SubRecords.Count > 0)
            {
                var masterCount = tes4.SubRecords.Count(x => x.Name == "MAST");
                var hedr = tes4.SubRecords[0];
                if (hedr.Name == "HEDR" && hedr.Size >= 12)
                {
                    hedr.TrySetValue(8, newid);
                }
            }
        }


        public Record GetRecordByID(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > this.Masters.Length)
            {
                return null;
            }

            Record r;

            // first check self for exact match
            if (this.TryGetRecordByID(id, out r))
            {
                return r;
            }

            id &= 0xffffff;
            if (pluginid >= this.Masters.Length || this.Masters[pluginid] == null)
            {
                return null;
            }

            // find the reference master and search it for reference
            // TODO: in theory another master could override the first master
            id += this.Fixups[pluginid] << 24;
            if (this.Masters[pluginid].TryGetRecordByID(id, out r))
            {
                return r;
            }

            return null;
        }

        public string GetRecordMaster(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid == this.Masters.Length)
                return this.FileName;
            if (pluginid > this.Masters.Length)
                return null;
            var master = this.Masters[pluginid];
            if (master != null)
                return master.FileName;
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (tes4 == null) return null;
            var sr = tes4.SubRecords.Where(x => x.Name == "MAST").ElementAtOrDefault((int)pluginid);
            if (sr == null) return null;
            return sr.GetStrData();
        }


        /// <summary>
        /// Lookup FormID by index.  Search via defined masters
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public string LookupFormID(uint id)
        {
            uint pluginid = (id & 0xff000000) >> 24;
            if (pluginid > this.Masters.Length)
            {
                return "FormID was invalid";
            }

            Record r;

            // First search self for exact match
            if (this.TryGetRecordByID(id, out r))
            {
                return r.DescriptiveName;
            }

            id &= 0xffffff;
            if (pluginid < this.Masters.Length && this.Masters[pluginid] != null)
            {
                // find the reference master and search it for reference
                // TODO: in theory another master could override the first master
                var p = this.Masters[pluginid];
                id |= this.Fixups[pluginid] << 24;
                if (p.TryGetRecordByID(id, out r))
                {
                    return r.DescriptiveName;
                }

                return "No match";
            }
            else
            {
                return "Master not loaded";
            }
        }

        public string LookupFormIDS(string sid)
        {
            uint id;
            if (!uint.TryParse(sid, NumberStyles.AllowHexSpecifier, null, out id))
            {
                return "FormID was invalid";
            }

            return this.LookupFormID(id);
        }

        public string LookupFormStrings(uint id)
        {
            string value = default(string);
            foreach (var plugin in this.Masters.Reverse())
            {
                if (plugin == null)
                {
                    continue;
                }

                if (plugin.Strings.TryGetValue(id, out value))
                {
                    break;
                }

                if (plugin.DLStrings.TryGetValue(id, out value))
                {
                    break;
                }

                if (plugin.ILStrings.TryGetValue(id, out value))
                {
                    break;
                }
            }

            return value;
        }

        public void Save(string filePath)
        {
            this.UpdateRecordCount();
            string tmpFileName = filePath + ".new";
            if (File.Exists(tmpFileName))
                File.Delete(tmpFileName);
            using (var bw = new BinaryWriter(File.OpenWrite(tmpFileName)))
            {
                this.SaveData(bw);
                bw.Flush();
            }

            if (File.Exists(filePath))
            {
                try
                {
                    string bakFile = null;
                    string fileName = Path.GetFileName(filePath);
                    bool backupExists = true;
                    int backupVersion = 0;
                    string backupFolder = CreateBackupFolder(filePath);
                    while (backupExists)
                    {
                        bakFile = Path.Combine(backupFolder, fileName + string.Format(".{0,3:D3}.bak", backupVersion));
                        backupExists = File.Exists(bakFile);
                        if (backupExists)
                            backupVersion++;
                    }
                    File.Replace(tmpFileName, filePath, bakFile);
                }
                catch
                {
                    Alerts.Show(Properties.Resources.Plugin_Save_UnableToBackup);
                    return;
                }
            }
            else
            {
                File.Move(tmpFileName, filePath);
            }

            // if (StringsDirty)
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (tes4 != null && (tes4.Flags1 & 0x80) != 0)
            {
                if (Properties.Settings.Default.SaveStringsFiles)
                {
                    string prefix = Path.Combine(Path.Combine(Path.GetDirectoryName(filePath), "Strings"), Path.GetFileNameWithoutExtension(filePath));
                    prefix += "_" + FalloutSnip.Domain.Properties.Settings.Default.LocalizationName;
                    this.SaveStrings(prefix);
                }
            }

            this.StringsDirty = false;
        }

        private string CreateBackupFolder(string filePath)
        {
            var dir = Path.Combine(Path.GetDirectoryName(filePath), "Backup");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        internal override void SaveData(BinaryWriter writer)
        {
            Compressor.Init();
            try
            {
                foreach (Rec r in this.Records)
                {
                    r.SaveData(writer);
                }
            }
            finally
            {
                Compressor.Close();
            }
        }

        public void SaveStrings(string FilePath)
        {
            System.Text.Encoding enc = Encoding.Instance;
            FontLangInfo fontInfo;
            if (Encoding.TryGetFontInfo(FalloutSnip.Domain.Properties.Settings.Default.LocalizationName, out fontInfo))
            {
                if (fontInfo.CodePage != 1252)
                {
                    enc = System.Text.Encoding.GetEncoding(fontInfo.CodePage);
                }
            }

            this.SavePluginStrings(enc, LocalizedStringFormat.Base, this.Strings, FilePath + ".STRINGS");
            this.SavePluginStrings(enc, LocalizedStringFormat.IL, this.ILStrings, FilePath + ".ILSTRINGS");
            this.SavePluginStrings(enc, LocalizedStringFormat.DL, this.DLStrings, FilePath + ".DLSTRINGS");
        }

        public void UpdateRecordCount()
        {
            int reccount = -1 + this.Records.Cast<Rec>().Sum(r => r.CountRecords());
            var tes4 = this.Records.OfType<Record>().FirstOrDefault(x => x.Name .StartsWith("TES"));
            if (tes4 != null)
            {
                var hedr = tes4.SubRecords[0];
                if (hedr.Name == "HEDR" && hedr.Size >= 12)
                {
                    hedr.TrySetValue(4, reccount);

                    if (Properties.Settings.Default.AutoUpdateNextFormID)
                    {
                        var minId = new uint[] {0x0801};
                        var masterCount = tes4.SubRecords.Count(x => x.Name == "MAST");
                        var maxID = this.Records.OfType<BaseRecord>().SelectMany(x => x.Enumerate()).OfType<Record>()
                            .Where(x => (x.FormID >> 24) == masterCount).Select(x => x.FormID & 0x00FFFFFF)
                            .Union(minId).Max();

                        uint curid;
                        if (hedr.TryGetValue(8, out curid) && maxID >= curid)
                            hedr.TrySetValue(8, maxID + 1);
                    }
                }
            }
        }
        
        private DomainDefinition DetectVersion(BinaryReader br, string fileName)
        {
            // Quick check for master esm.  Skyrim.esm uses same as fallout. so harder to detect
            if (!string.IsNullOrEmpty(fileName))
            {
                var filename = Path.GetFileName(fileName);
                var foldername = Path.GetDirectoryName(fileName);
                foreach (var domain in DomainDefinition.AllDomains().Where(domain => 
                    string.Compare(domain.Master, filename,StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    if (!domain.Loaded)
                        DomainDefinition.Load(domain.Name);
                    return domain;
                }

                // Now quick check for folder location
                if (!string.IsNullOrWhiteSpace(foldername))
                {
                    foreach (var domain in DomainDefinition.AllDomains().Where(domain =>
                        string.Compare(domain.GameDataDirectory, foldername, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        if (!domain.Loaded)
                            DomainDefinition.Load(domain.Name);
                        return domain;
                    }
                }
            }

            var tes = ReadRecName(br);
            if (tes == "TES3")
                return DomainDefinition.Load("Morrowind"); // hardcoded?
            if (tes != "TES4")
                throw new Exception("File is not a valid TES4 plugin (Missing TES4 record)");
            // Check for file version by checking the position of the HEDR field in the file. (ie. how big are the record header.)
            br.BaseStream.Position = 20;
            var s = ReadRecName(br);
            if (s == "HEDR")
                return DomainDefinition.Load("Oblivion"); // hardcoded?
            s = ReadRecName(br);
            if (s != "HEDR")
                throw new Exception("File is not a valid TES4 plugin (Missing HEDR subrecord in the TES4 record)");
            var recsize = br.ReadUInt16();
            var version = br.ReadSingle();
            //var domain = DomainDefinition.DetectDefinitionFromLocation(tes, version);
            return DomainDefinition.DetectDefinitionFromVersion(tes, version);
        }

        private DomainDefinition DetectVersion()
        {
            Record brcTES = this.Records.OfType<Record>().FirstOrDefault(x => x.Name.StartsWith("TES"));
            if (brcTES == null)
                throw new ApplicationException("Plugin lacks a valid TES4 record. Cannot continue.");
            var hdr = brcTES.SubRecords.FirstOrDefault(x => x.Name == "HEDR");
            if (hdr == null)
                throw new ApplicationException("Plugin lacks a valid HEDR subrecord. Cannot continue.");
            var version = hdr.GetValue<float>(0);
            return DomainDefinition.DetectDefinitionFromVersion(brcTES.Name, version);
        }

        private void LoadPluginData(BinaryReader br, bool headerOnly, Func<string, bool> includeFilter)
        {
            bool oldHoldUpdates = HoldUpdates;
            try
            {
                string s;
                uint recsize;

                this.Filtered = includeFilter != null;

                HoldUpdates = true;
                Decompressor.Init();

                s = ReadRecName(br);
                if (s != this.define.HEDRType)
                {
                    throw new Exception("File is not a valid TES4 plugin (Missing TES4 record)");
                }

                // Check for file version by checking the position of the HEDR field in the file. (ie. how big are the record header.)
                br.BaseStream.Position = define.HEDROffset;
                s = ReadRecName(br);
                if (s != "HEDR")
                    throw new Exception(
                        $"File is not a valid {define.Name} plugin (Missing HEDR subrecord in the {define.HEDRType} record)");
                br.BaseStream.Position = 4;
                recsize = br.ReadUInt32();
                try
                {
                    this.AddRecord(new Record(this.define.HEDRType, recsize, br, this.define));
                }
                catch (Exception e)
                {
                    Alerts.Show(e.Message);
                }
                //bool hasExtraFlags = Math.Abs(version - 1.0f) > float.Epsilon * 10.0f;

                if (!headerOnly)
                {
                    while (br.PeekChar() != -1)
                    {
                        s = ReadRecName(br);
                        recsize = br.ReadUInt32();
                        if (s == "GRUP")
                        {
                            try
                            {
                                this.AddRecord(new GroupRecord(recsize, br, this.define, includeFilter, false));
                            }
                            catch (Exception e)
                            {
                                Alerts.Show(e.Message);
                            }
                        }
                        else
                        {
                            bool skip = includeFilter != null && !includeFilter(s);
                            if (skip)
                            {
                                long size = recsize + define.RecSize;
                                if ((br.ReadUInt32() & 0x00040000) > 0)
                                {
                                    size += 4; // Add 4 bytes for compressed record since the decompressed size is not included in the record size.
                                }

                                br.BaseStream.Position += size; // just position past the data
                            }
                            else
                            {
                                try
                                {
                                    this.AddRecord(new Record(s, recsize, br, define));
                                }
                                catch (Exception e)
                                {
                                    Alerts.Show(e.Message);
                                }
                            }
                        }
                    }
                }
                foreach (var rec in Enumerate(x => x is IGroupRecord || x is Record))
                    rec.UpdateShortDescription();
                this.UpdateRecordCount();
            }
            finally
            {
                HoldUpdates = oldHoldUpdates;
                FireRecordListUpdate(this, this);
                Decompressor.Close();
            }
        }

        private LocalizedStringDict LoadPluginStrings(System.Text.Encoding encoding, LocalizedStringFormat format, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (var reader = new BinaryReader(File.OpenRead(path))) return this.LoadPluginStrings(encoding, format, reader);
                }
            }
            catch
            {
            }

            return new LocalizedStringDict();
        }

        private LocalizedStringDict LoadPluginStrings(System.Text.Encoding encoding, LocalizedStringFormat format, BinaryReader reader)
        {
            if (encoding == null)
            {
                encoding = Encoding.Instance;
            }

            var dict = new LocalizedStringDict();
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
            var data = new byte[size];
            using (var stream = new MemoryStream(data, 0, size, true, false))
            {
                var buffer = new byte[65536];
                int left = size;
                while (left > 0)
                {
                    int read = Math.Min(left, buffer.Length);
                    int nread = reader.BaseStream.Read(buffer, 0, read);
                    if (nread == 0)
                    {
                        break;
                    }

                    stream.Write(buffer, 0, nread);
                    left -= nread;
                }
            }

            foreach (var kvp in list)
            {
                var start = (int)kvp.Value;
                int len = 0;
                switch (format)
                {
                    case LocalizedStringFormat.Base:
                        while (data[start + len] != 0)
                        {
                            ++len;
                        }

                        break;

                    case LocalizedStringFormat.DL:
                    case LocalizedStringFormat.IL:
                        len = BitConverter.ToInt32(data, start) - 1;
                        start = start + sizeof(int);
                        if (start + len > data.Length)
                        {
                            len = data.Length - start;
                        }

                        if (len < 0)
                        {
                            len = 0;
                        }

                        break;
                }

                string str = encoding.GetString(data, start, len);
                dict.Add(kvp.Key, str);
            }

            return dict;
        }

        private void RebuildCache()
        {
            if (this.FormIDLookup.Count == 0)
            {
                this.ForEach(
                    br =>
                    {
                        var r = br as Record;
                        if (r != null)
                        {
                            this.FormIDLookup[r.FormID] = r;
                        }
                    });
            }
        }

        private void SavePluginStrings(System.Text.Encoding enc, LocalizedStringFormat format, LocalizedStringDict strings, string path)
        {
            try
            {
                using (var writer = new BinaryWriter(File.Create(path))) this.SavePluginStrings(enc, format, strings, writer);
            }
            catch
            {
            }
        }

        private void SavePluginStrings(System.Text.Encoding enc, LocalizedStringFormat format, LocalizedStringDict strings, BinaryWriter writer)
        {
            if (enc == null)
            {
                enc = Encoding.Instance;
            }

            var list = new List<Pair<uint, uint>>();

            using (var stream = new MemoryStream())
            using (var memWriter = new BinaryWriter(stream))
            {
                foreach (var kvp in strings)
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
                var buffer = new byte[65536];
                var left = (int)stream.Length;
                while (left > 0)
                {
                    int read = Math.Min(left, buffer.Length);
                    int nread = stream.Read(buffer, 0, read);
                    if (nread == 0)
                    {
                        break;
                    }

                    writer.Write(buffer, 0, nread);
                    left -= nread;
                }
            }
        }

        internal Dictionary<string, RecordStructure> GetRecordStructures()
        {
            return define.Records;
        }

        public override string DescriptiveName
        {
            get
            {
                return base.DescriptiveName + " (" + define.Name + ")";
            }
        }

        public override string ToString()
        {
            return string.Format("[Plugin] '{0}'", this.Name);
        }
    }
}