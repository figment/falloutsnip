using System;
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
    public class TESParserException : Exception { public TESParserException(string msg) : base(msg) { } }

    #region class SelectionContext
    /// <summary>
    /// External state for holding single selection for use with evaluating descriptions and intelligent editors
    /// </summary>
    public class SelectionContext
    {
        private Plugin plugin;
        private Record record;
        private SubRecord subRecord;

        public Plugin Plugin
        {
            get { return plugin; }
            set
            {
                if (this.plugin != value)
                {
                    this.plugin = value;
                    this.Groups.Clear();
                    this.Record = null;
                    if (this.PluginChanged != null)
                        this.PluginChanged(this, EventArgs.Empty);
                }
            }
        }
        public Stack<GroupRecord> Groups = new Stack<GroupRecord>();
        public Record Record
        {
            get { return this.record; }
            set
            {
                if (this.record != value)
                {
                    this.record = value;
                    this.SubRecord = null;
                    this.Conditions.Clear();
                    if (this.RecordChanged != null)
                        this.RecordChanged(this, EventArgs.Empty);
                }
            }
        }
        public SubRecord SubRecord
        {
            get { return this.subRecord; }
            set
            {
                if (this.subRecord != value)
                {
                    this.subRecord = value;
                    if (this.SubRecordChanged != null)
                        this.SubRecordChanged(this, EventArgs.Empty);
                }
            }
        }
        internal Dictionary<int, Conditional> Conditions = new Dictionary<int, Conditional>();
        internal dFormIDLookupI formIDLookup = null;
        internal dLStringLookup strLookup = null;
        internal dFormIDLookupR formIDLookupR = null;

        public bool SelectedSubrecord
        {
            get { return this.SubRecord != null; }
        }

        public void Reset()
        {
            this.Plugin = null;
        }

        public event EventHandler PluginChanged;
        public event EventHandler RecordChanged;
        public event EventHandler SubRecordChanged;

        public SelectionContext Clone()
        {
            var result = (SelectionContext)this.MemberwiseClone();
            result.PluginChanged = null;
            result.RecordChanged = null;
            result.SubRecordChanged = null;
            return result;
        }
    }
    #endregion

    #region class Compressor / Decompressor
    static class Compressor
    {
        private static byte[] buffer;
        private static MemoryStream ms;
        private static ICSharpCode.SharpZipLib.Zip.Compression.Deflater def;
        private static ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream defstr;
        private static string[] autoCompRecList = new string[0];


        public static Stream GetSharedStream()
        {
            ms.SetLength(0);
            ms.Position = 0;
            return ms; 
        }

        public static BinaryWriter AllocWriter(Stream s)
        {
            int compressLevel = 9;
            def = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(compressLevel, false);
            defstr = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(ms, def);
            defstr.IsStreamOwner = false;
            return new BinaryWriter(defstr);
        }

        public static void CopyTo(BinaryWriter output, Stream input)
        {
            long left = input.Length;
            while (left > 0)
            {
                int nread = input.Read(buffer,0, buffer.Length);
                if (nread == 0) break;
                output.Write(buffer, 0, nread);
            }
        }

        public static void Init()
        {
            ms = new MemoryStream();
            buffer = new byte[0x4000];

            // bit of a hack to avoid rebuilding this look up index
            autoCompRecList = TESVSnip.Properties.Settings.Default.AutoCompressRecords != null
                ? TESVSnip.Properties.Settings.Default.AutoCompressRecords.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];
            Array.Sort(autoCompRecList);
        }

        public static bool CompressRecord(string name)
        {
            return Array.BinarySearch<string>(autoCompRecList, name) >= 0;
        }

        public static void Close()
        {
            def = null;
            buffer = null;
            if (ms != null) ms.Dispose();
            ms = null;
        }
    }

    static class Decompressor
    {
        private static byte[] input;
        private static byte[] output;
        private static MemoryStream ms;
        private static BinaryReader compReader;
        private static ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf;

        public static BinaryReader Decompress(BinaryReader br, int size, int outsize)
        {
            if (input.Length < size)
            {
                input = new byte[size];
            }
            if (output.Length < outsize)
            {
                output = new byte[outsize];
            }
            br.Read(input, 0, size);
            inf.SetInput(input, 0, size);
            inf.Inflate(output);
            inf.Reset();

            ms.Position = 0;
            ms.Write(output, 0, outsize);
            ms.Position = 0;

            return compReader;
        }
        
        public static void Init()
        {
            inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater(false);
            ms = new MemoryStream();
            compReader = new BinaryReader(ms);
            input = new byte[0x1000];
            output = new byte[0x4000];
        }
        public static void Close()
        {
            compReader.Close();
            compReader = null;
            inf = null;
            input = null;
            output = null;
            ms = null;
        }
    }
    #endregion 

    #region class BaseRecord
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public abstract class BaseRecord : PersistObject, ICloneable, ISerializable
    {
        [Persistable]
        public virtual string Name { get; set; }

        public abstract long Size { get; }
        public abstract long Size2 { get; }

        public abstract string GetDesc();
        public virtual void GetFormattedHeader(RTFBuilder rb, SelectionContext context) { }
        public virtual void GetFormattedData(RTFBuilder rb, SelectionContext context) { rb.Append(GetDesc()); }
        public virtual void GetFormattedData(StringBuilder sb, SelectionContext context) { sb.Append(GetDesc()); }

        public abstract bool DeleteRecord(BaseRecord br);
        public abstract void AddRecord(BaseRecord br);
        public virtual void InsertRecord(int index, BaseRecord br) { AddRecord(br); }

        // internal iterators
        public virtual bool While(Predicate<BaseRecord> action) { return action(this); }
        public virtual void ForEach(Action<BaseRecord> action) { action(this); }
        public virtual IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            if (match(this)) yield return this;
        }

        internal abstract List<string> GetIDs(bool lower);
        internal abstract void SaveData(BinaryWriter bw);

        private static readonly byte[] RecByte = new byte[4];
        protected static string ReadRecName(BinaryReader br)
        {
            br.Read(RecByte, 0, 4);
            return "" + ((char)RecByte[0]) + ((char)RecByte[1]) + ((char)RecByte[2]) + ((char)RecByte[3]);
        }
        protected static void WriteString(BinaryWriter bw, string s)
        {
            byte[] b = new byte[s.Length];
            for (int i = 0; i < s.Length; i++) b[i] = (byte)s[i];
            bw.Write(b, 0, s.Length);
        }

        protected BaseRecord() { }
        protected BaseRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public abstract BaseRecord Clone();

        object ICloneable.Clone() { return this.Clone(); }
    }
    #endregion

    #region class Plugin
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public sealed class Plugin : BaseRecord
    {
        [Persistable]
        public readonly List<Rec> Records = new List<Rec>();

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

        public override long Size
        {
            get { long size = 0; foreach (Rec rec in Records) size += rec.Size2; return size; }
        }
        public override long Size2 { get { return Size; } }

        public override bool DeleteRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) return false;
            bool result = Records.Remove(r);
            InvalidateCache();
            return result;
        }

        public override void AddRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Plugins can only hold Groups or Records.");
            Records.Add(r);
            InvalidateCache();
        }
        public override void InsertRecord(int idx, BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Plugins can only hold Groups or Records.");
            Records.Insert(idx, r);
            InvalidateCache();
        }

        public override IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            if (!match(this)) yield break;
            foreach (var r in this.Records)
                foreach (var itm in r.Enumerate(match))
                    yield return itm;
        }

        public override bool While(Predicate<BaseRecord> action)
        {
            if (!base.While(action)) return false;
            foreach (var r in this.Records)
                if (!r.While(action))
                    return false;
            return true;
        }
        public override void ForEach(Action<BaseRecord> action)
        {
            base.ForEach(action);
            foreach (var r in this.Records) r.ForEach(action);
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
            string s;
            uint recsize;
            bool IsOblivion = false;

            this.Filtered = (recFilter != null && recFilter.Length > 0);

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
            Records.Add(new Record("TES4", recsize, br, IsOblivion));
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
                        Records.Add(new GroupRecord(recsize, br, IsOblivion, recFilter, false));
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
                            Records.Add(new Record(s, recsize, br, IsOblivion));
                    }
#if DEBUG
                    System.Diagnostics.Debug.Assert((br.BaseStream.Position - szPos) == recsize);
#endif
                }
            }

            Decompressor.Close();
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
                "Records: " + Records.Count;
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
    }
    #endregion

    #region class Rec  (base class of Record and Group for TreeNode)
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public abstract class Rec : BaseRecord
    {
        protected Rec() { }

        protected Rec(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [Persistable]
        protected string descriptiveName;
        public virtual string DescriptiveName
        {
            get { return descriptiveName == null ? Name : (Name + descriptiveName); }
            //set { descriptiveName = value; }
        }
        public virtual void SetDescription(string value)
        {
            this.descriptiveName = value;
        }
        public virtual void UpdateShortDescription() { this.descriptiveName = ""; }
    }
    #endregion

    #region class Group Record
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public sealed class GroupRecord : Rec
    {
        [Persistable]
        public readonly List<Rec> Records = new List<Rec>();
        [Persistable]
        private readonly byte[] data;
        [Persistable]
        public uint groupType;
        [Persistable]
        public uint dateStamp;
        [Persistable]
        public uint flags;

        public string ContentsType
        {
            get { return groupType == 0 ? "" + (char)data[0] + (char)data[1] + (char)data[2] + (char)data[3] : ""; }
        }

        public override long Size
        {
            get { long size = 24; foreach (Rec rec in Records) size += rec.Size2; return size; }
        }
        public override long Size2 { get { return Size; } }

        public override bool DeleteRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) return false;
            return Records.Remove(r);
        }
        public override void AddRecord(BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Groups can only hold records or other groups.");
            Records.Add(r);
        }
        public override void InsertRecord(int idx, BaseRecord br)
        {
            Rec r = br as Rec;
            if (r == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Groups can only hold records or other groups.");
            Records.Insert(idx, r);
        }

        public override IEnumerable<BaseRecord> Enumerate(Predicate<BaseRecord> match)
        {
            if (!match(this)) yield break;
            foreach (var r in this.Records)
                foreach (var itm in r.Enumerate(match))
                    yield return itm;
        }

        public override bool While(Predicate<BaseRecord> action)
        {
            if (!base.While(action))
                return false;
            foreach (var r in this.Records)
                if (!r.While(action))
                    return false;
            return true;
        }

        public override void ForEach(Action<BaseRecord> action)
        {
            base.ForEach(action);
            foreach (var r in this.Records) r.ForEach(action);
        }

        GroupRecord() { }

        GroupRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        internal GroupRecord(uint Size, BinaryReader br, bool Oblivion, string[] recFilter, bool filterAll)
        {
            Name = "GRUP";
            data = br.ReadBytes(4);
            groupType = br.ReadUInt32();
            dateStamp = br.ReadUInt32();
            string contentType = groupType == 0 ? TESVSnip.Encoding.CP1252.GetString(data) : "";
            if (!Oblivion) flags = br.ReadUInt32();
            uint AmountRead = 0;
            while (AmountRead < Size - (Oblivion ? 20 : 24))
            {
#if DEBUG
                long szPos = br.BaseStream.Position;
#endif
                string s = Plugin.ReadRecName(br);
                uint recsize = br.ReadUInt32();
#if DEBUG
                System.Diagnostics.Trace.TraceInformation("{0} {1}", s, recsize);
#endif
                if (s == "GRUP")
                {
                    bool skip = filterAll || (recFilter != null && Array.IndexOf(recFilter, contentType) >= 0);
                    GroupRecord gr = new GroupRecord(recsize, br, Oblivion, recFilter, skip);
                    AmountRead += recsize;

                    if (!filterAll) Records.Add(gr);
#if DEBUG
                    System.Diagnostics.Debug.Assert((br.BaseStream.Position - szPos) == recsize);
#endif
                }
                else
                {
                    bool skip = filterAll || (recFilter != null && Array.IndexOf(recFilter, s) >= 0);
                    if (skip)
                    {
                        long size = (recsize + (Oblivion ? 12 : 16));
                        //if ((br.ReadUInt32() & 0x00040000) > 0) size += 4;
                        br.BaseStream.Position += size;// just read past the data
                        AmountRead += (uint)(recsize + (Oblivion ? 20 : 24));
                    }
                    else
                    {
                        Record r = new Record(s, recsize, br, Oblivion);
                        AmountRead += (uint)(recsize + (Oblivion ? 20 : 24));
                        Records.Add(r);
                    }
#if DEBUG
                    System.Diagnostics.Debug.Assert((br.BaseStream.Position - szPos) - (Oblivion ? 20 : 24) == recsize);
#endif
                }
            }
            if (AmountRead > (Size - (Oblivion ? 20 : 24)))
            {
                throw new TESParserException("Record block did not match the size specified in the group header");
            }
            this.UpdateShortDescription();
        }

        public GroupRecord(string data)
        {
            Name = "GRUP";
            this.data = new byte[4];
            for (int i = 0; i < 4; i++) this.data[i] = (byte)data[i];
            UpdateShortDescription();
        }

        private GroupRecord(GroupRecord gr)
        {
            Name = "GRUP";
            data = (byte[])gr.data.Clone();
            groupType = gr.groupType;
            dateStamp = gr.dateStamp;
            flags = gr.flags;
            Records = new List<Rec>(gr.Records.Count);
            for (int i = 0; i < gr.Records.Count; i++) Records.Add((Rec)gr.Records[i].Clone());
            Name = gr.Name;
            UpdateShortDescription();
        }

        private string GetSubDesc()
        {
            switch (groupType)
            {
                case 0:
                    return "(Contains: " + (char)data[0] + (char)data[1] + (char)data[2] + (char)data[3] + ")";
                case 2:
                case 3:
                    return "(Block number: " + (data[0] + data[1] * 256 + data[2] * 256 * 256 + data[3] * 256 * 256 * 256).ToString() + ")";
                case 4:
                case 5:
                    return "(Coordinates: [" + (data[0] + data[1] * 256) + ", " + data[2] + data[3] * 256 + "])";
                case 1:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    return "(Parent FormID: 0x" + data[3].ToString("x2") + data[2].ToString("x2") + data[1].ToString("x2") + data[0].ToString("x2") + ")";
            }
            return null;
        }

        public override string GetDesc()
        {
            string desc = "[Record group]" + Environment.NewLine + "Record type: ";
            switch (groupType)
            {
                case 0:
                    desc += "Top " + GetSubDesc();
                    break;
                case 1:
                    desc += "World children " + GetSubDesc();
                    break;
                case 2:
                    desc += "Interior Cell Block " + GetSubDesc();
                    break;
                case 3:
                    desc += "Interior Cell Sub-Block " + GetSubDesc();
                    break;
                case 4:
                    desc += "Exterior Cell Block " + GetSubDesc();
                    break;
                case 5:
                    desc += "Exterior Cell Sub-Block " + GetSubDesc();
                    break;
                case 6:
                    desc += "Cell Children " + GetSubDesc();
                    break;
                case 7:
                    desc += "Topic Children " + GetSubDesc();
                    break;
                case 8:
                    desc += "Cell Persistent Children " + GetSubDesc();
                    break;
                case 9:
                    desc += "Cell Temporary Children " + GetSubDesc();
                    break;
                case 10:
                    desc += "Cell Visible Distant Children " + GetSubDesc();
                    break;
                default:
                    desc += "Unknown";
                    break;
            }
            return desc + Environment.NewLine +
                "Records: " + Records.Count.ToString() + Environment.NewLine +
                "Size: " + Size.ToString() + " bytes (including header)";
        }

        internal override void SaveData(BinaryWriter bw)
        {
            long startpos = bw.BaseStream.Position;
            uint svSize = (uint)Size;
            uint svSize2 = (uint)Size2;
            WriteString(bw, "GRUP");
            bw.Write(svSize);
            bw.Write(data);
            bw.Write(groupType);
            bw.Write(dateStamp);
            bw.Write(flags); // should this check for oblivion?
            foreach (Rec r in Records) r.SaveData(bw);
            bw.Flush();
            long curpos = bw.BaseStream.Position;
            uint wrSize = (uint)(curpos - startpos);
            if (wrSize != svSize2) // fix size probably due to compression
            {
                bw.BaseStream.Position = startpos + 4;
                bw.Write(wrSize);
                bw.BaseStream.Position = curpos;
            }
        }

        internal override List<string> GetIDs(bool lower)
        {
            List<string> list = new List<string>();
            foreach (Record r in Records) list.AddRange(r.GetIDs(lower));
            return list;
        }

        public override BaseRecord Clone()
        {
            return new GroupRecord(this);
        }

        public byte[] GetData() { return (byte[])data.Clone(); }
        internal byte[] GetReadonlyData() { return data; }
        public void SetData(byte[] data)
        {
            if (data.Length != 4) throw new ArgumentException("data length must be 4");
            for (int i = 0; i < 4; i++) this.data[i] = data[i];
        }

        public override void UpdateShortDescription()
        {
            if (groupType == 0)
            {
                string data = TESVSnip.Encoding.CP1252.GetString(this.data);
                string desc = string.Format(" ({0})", data);
                if (groupType == 0)
                {
                    RecordStructure rec;
                    if (RecordStructure.Records.TryGetValue(data, out rec))
                    {
                        if (rec.description != data)
                            desc += " - " + rec.description;
                    }
                }
                this.descriptiveName = desc;
            }
            else
            {
                this.descriptiveName = "";
            }
        }
    }
    #endregion

    #region class Record
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public sealed class Record : Rec, ISerializable, IDeserializationCallback
    {
        public readonly TESVSnip.Collections.Generic.AdvancedList<SubRecord> SubRecords;
        [Persistable]
        public uint Flags1;
        [Persistable]
        public uint Flags2;
        [Persistable]
        public uint Flags3;
        [Persistable]
        public uint FormID;

        static Dictionary<string, Func<string>> overrideFunctionsByType = new Dictionary<string, Func<string>>();
        Func<string> descNameOverride;

        public override long Size
        {
            get
            {
                long size = 0;
                foreach (SubRecord rec in SubRecords) size += rec.Size2;
                return size;
            }
        }
        public override long Size2
        {
            get
            {
                long size = 24;
                foreach (SubRecord rec in SubRecords) size += rec.Size2;
                return size;
            }
        }

        public override bool DeleteRecord(BaseRecord br)
        {
            SubRecord sr = br as SubRecord;
            if (sr == null) return false;
            return SubRecords.Remove(sr);
        }

        public override void AddRecord(BaseRecord br)
        {
            SubRecord sr = br as SubRecord;
            if (sr == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Records can only hold Subrecords.");
            SubRecords.Add(sr);
        }
        public override void InsertRecord(int idx, BaseRecord br)
        {
            SubRecord sr = br as SubRecord;
            if (sr == null) throw new TESParserException("Record to add was not of the correct type." +
                   Environment.NewLine + "Records can only hold Subrecords.");
            SubRecords.Insert(idx, sr);
        }

        // due to weird 'bug' in serialization of arrays we do not have access to children yet.
        SubRecord[] serializationItems = null;
        Record(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            serializationItems = info.GetValue("SubRecords", typeof(SubRecord[])) as SubRecord[];
            SubRecords = new Collections.Generic.AdvancedList<SubRecord>(1);
            descNameOverride = new Func<string>(DefaultDescriptiveName);
            UpdateShortDescription();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SubRecords", SubRecords.ToArray());
            TESVSnip.Data.PersistAssist.Serialize(this, info, context);
        }
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (serializationItems != null)
                this.SubRecords.AddRange(serializationItems.OfType<SubRecord>().ToList());
            serializationItems = null;
        }

        internal Record(string name, uint Size, BinaryReader br, bool Oblivion)
        {
            SubRecords = new TESVSnip.Collections.Generic.AdvancedList<SubRecord>(1);
            SubRecords.AllowSorting = false;
            Name = name;
            Flags1 = br.ReadUInt32();
            FormID = br.ReadUInt32();
            Flags2 = br.ReadUInt32();
            if (!Oblivion) Flags3 = br.ReadUInt32();
            if ((Flags1 & 0x00040000) > 0)
            {
                //Flags1 ^= 0x00040000;
                uint newSize = br.ReadUInt32();
                br = Decompressor.Decompress(br, (int)(Size - 4), (int)newSize);
                Size = newSize;
            }
            uint AmountRead = 0;
            while (AmountRead < Size)
            {
                string s = ReadRecName(br);
                uint i = 0;
                if (s == "XXXX")
                {
                    br.ReadUInt16();
                    i = br.ReadUInt32();
                    s = ReadRecName(br);
                }
                SubRecord r = new SubRecord(this, s, br, i);
                AmountRead += (uint)(r.Size2);
                SubRecords.Add(r);
            }
            if (AmountRead > Size)
            {
                throw new TESParserException("Subrecord block did not match the size specified in the record header");
            }
            descNameOverride = new Func<string>(DefaultDescriptiveName);
            UpdateShortDescription();
            //br.BaseStream.Position+=Size;
        }

        private Record(Record r)
        {
            SubRecords = new TESVSnip.Collections.Generic.AdvancedList<SubRecord>(r.SubRecords.Count);
            SubRecords.AllowSorting = false;
            foreach (var sr in r.SubRecords.OfType<SubRecord>())
                SubRecords.Add((SubRecord)sr.Clone());
            Flags1 = r.Flags1;
            Flags2 = r.Flags2;
            Flags3 = r.Flags3;
            FormID = r.FormID;
            Name = r.Name;
            descNameOverride = new Func<string>(DefaultDescriptiveName);
            UpdateShortDescription();
        }

        public Record()
        {
            Name = "NEW_";
            SubRecords = new TESVSnip.Collections.Generic.AdvancedList<SubRecord>();
            descNameOverride = new Func<string>(DefaultDescriptiveName);
            UpdateShortDescription();
        }

        public override BaseRecord Clone()
        {
            return new Record(this);
        }

        private string DefaultDescriptiveName() { return base.DescriptiveName; }

        public override string DescriptiveName
        {
            get { return descNameOverride(); }
            //set { base.DescriptiveName = value; }
        }

        public override void UpdateShortDescription()
        {
            if (this.Name == "REFR") // temporary hack for references
            {
                var edid = SubRecords.FirstOrDefault(x => x.Name == "EDID");
                string desc = (edid != null) ? string.Format(" ({0})", edid.GetStrData()) : "";
                //var name = SubRecords.FirstOrDefault( x => x.Name == "NAME" );
                var data = SubRecords.FirstOrDefault(x => x.Name == "DATA");
                if (data != null)
                {
                    desc = string.Format(" [{1},{2}]\t{0}",
                        desc, (int)(data.GetValue<float>(0) / 4096.0f), (int)(data.GetValue<float>(4) / 4096.0f)
                        );
                }
                this.descriptiveName = desc;
            }
            else if (this.Name == "ACHR") // temporary hack for references
            {
                var edid = SubRecords.FirstOrDefault(x => x.Name == "EDID");
                string desc = (edid != null) ? string.Format(" ({0})", edid.GetStrData()) : "";
                var data = SubRecords.FirstOrDefault(x => x.Name == "DATA");
                if (data != null)
                {
                    desc = string.Format(" [{1},{2}]\t{0}",
                        desc, (int)(data.GetValue<float>(0) / 4096.0f), (int)(data.GetValue<float>(4) / 4096.0f)
                        );
                }
                this.descriptiveName = desc;
            }
            else if (this.Name == "CELL")
            {
                var edid = SubRecords.FirstOrDefault(x => x.Name == "EDID");
                string desc = (edid != null) ? desc = " (" + edid.GetStrData() + ")" : "";

                var xclc = SubRecords.FirstOrDefault(x => x.Name == "XCLC");
                if (xclc != null)
                {
                    desc = string.Format(" [{1:F0},{2:F0}]\t{0}",
                        desc, xclc.GetValue<int>(0), xclc.GetValue<int>(4)
                        );
                }
                else
                {
                    desc = string.Format(" [Intr]\t{0}", desc);
                }
                this.descriptiveName = desc;
            }
            else
            {
                var edid = SubRecords.FirstOrDefault(x => x.Name == "EDID");
                if (edid != null) this.descriptiveName = " (" + edid.GetStrData() + ")";
                else this.descriptiveName = "";
            }
        }

        private string GetBaseDesc()
        {
            return "Type: " + Name + Environment.NewLine +
                "FormID: " + FormID.ToString("x8") + Environment.NewLine +
                "Flags 1: " + Flags1.ToString("x8") +
                (Flags1 == 0 ? "" : " (" + FlagDefs.GetRecFlags1Desc(Flags1) + ")") +
                Environment.NewLine +
                "Flags 2: " + Flags2.ToString("x8") + Environment.NewLine +
                "Flags 3: " + Flags3.ToString("x8") + Environment.NewLine +
                "Subrecords: " + SubRecords.Count.ToString() + Environment.NewLine +
                "Size: " + Size.ToString() + " bytes (excluding header)";
        }

        private string GetLocalizedString(dLStringLookup strLookup)
        {
            return default(string);
        }

        public override string GetDesc()
        {
            return "[Record]" + Environment.NewLine + GetBaseDesc();
        }

        public override void GetFormattedHeader(RTFBuilder rb, SelectionContext context)
        {
            rb.FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 4).ForeColor(KnownColor.DarkGray).AppendLine("[Record]");


            rb.Append("Type: \t").FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 2).AppendFormat("{0}", Name).AppendLine();
            rb.Append("FormID: \t").FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 2).ForeColor(KnownColor.DarkRed).AppendFormat("{0:X8}", FormID).AppendLine();
            rb.AppendLineFormat("Flags 1: \t{0:X8}", Flags1);
            if (Flags1 != 0) rb.AppendLineFormat(" ({0})", FlagDefs.GetRecFlags1Desc(Flags1));
            rb.AppendLineFormat("Flags 2: \t{0:X8}", Flags2);
            rb.AppendLineFormat("Flags 3: \t{0:X8}", Flags3);
            rb.AppendLineFormat("Size: \t{0:N0}", Size);
            rb.AppendLineFormat("Subrecords:\t{0}", SubRecords.Count);
            rb.AppendPara();
        }

        public override void GetFormattedData(RTFBuilder rb, SelectionContext context)
        {
            try
            {
                rb.FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize).ForeColor(KnownColor.DarkGray).AppendLine("[Formatted information]");
                rb.Reset();

                context = context.Clone();
                context.Record = this;
                RecordStructure rec;
                if (!RecordStructure.Records.TryGetValue(Name, out rec))
                    return;
                rb.FontStyle(FontStyle.Bold).ForeColor(KnownColor.DarkBlue).FontSize(rb.DefaultFontSize + 4).AppendLine(rec.description);
                foreach (var subrec in SubRecords)
                {
                    if (subrec.Structure == null || subrec.Structure.elements == null || subrec.Structure.notininfo)
                        continue;
                    context.SubRecord = subrec;
                    rb.AppendLine();
                    subrec.GetFormattedData(rb, context);
                }
            }
            catch
            {
                rb.ForeColor(KnownColor.Red).Append("Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml");
            }
        }

        internal string GetDesc(SelectionContext context)
        {
            string start = "[Record]" + Environment.NewLine + GetBaseDesc();
            string end;
            try
            {
                end = GetExtendedDesc(context);
            }
            catch
            {
                end = "Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml";
            }
            if (end == null) return start;
            else return start + Environment.NewLine + Environment.NewLine + "[Formatted information]" + Environment.NewLine + end;
        }

        #region Extended Description
        private string GetExtendedDesc(SelectionContext selectContext)
        {
            var context = selectContext.Clone();
            try
            {
                context.Record = this;
                RecordStructure rec;
                if (!RecordStructure.Records.TryGetValue(Name, out rec))
                    return "";
                var s = new System.Text.StringBuilder();
                s.AppendLine(rec.description);
                foreach (var subrec in SubRecords)
                {
                    if (subrec.Structure == null)
                        continue;
                    if (subrec.Structure.elements == null)
                        return s.ToString();
                    if (subrec.Structure.notininfo)
                        continue;

                    context.SubRecord = subrec;
                    s.AppendLine();
                    s.Append(subrec.GetFormattedData(context));
                }
                return s.ToString();
            }
            finally
            {
                context.Record = null;
                context.SubRecord = null;
                context.Conditions.Clear();
            }
        }

        #endregion


        internal override void SaveData(BinaryWriter bw)
        {
            WriteString(bw, Name);
            uint srSize = (uint)Size;

            bool bCompress = false;
            if (global::TESVSnip.Properties.Settings.Default.UseDefaultRecordCompression)
            {
                bCompress = ((Flags1 & 0x00040000) != 0)
                    || (global::TESVSnip.Properties.Settings.Default.EnableAutoCompress && Compressor.CompressRecord(Name))
                    || (global::TESVSnip.Properties.Settings.Default.EnableCompressionLimit &&
                      (srSize >= global::TESVSnip.Properties.Settings.Default.CompressionLimit));
            }
            if (bCompress) // compressed
            {
                var stream = Compressor.GetSharedStream();
                using (var writer = Compressor.AllocWriter(stream))
                    foreach (SubRecord sr in SubRecords) sr.SaveData(writer);

                bw.Write((uint)stream.Length + 4); // Size of compressed section + length
                bw.Write((uint)(Flags1 | 0x00040000));
                bw.Write(FormID);
                bw.Write(Flags2);
                bw.Write(Flags3);

                stream.Position = 0;
                bw.Write(srSize); //ideally use writer bytes written but should be same
                Compressor.CopyTo(bw, stream);
            }
            else
            {
                bw.Write(srSize);
                bw.Write((uint)(Flags1 & ~0x00040000));
                bw.Write(FormID);
                bw.Write(Flags2);
                bw.Write(Flags3);
                foreach (SubRecord sr in SubRecords) sr.SaveData(bw);
            }
        }

        internal override List<string> GetIDs(bool lower)
        {
            List<string> list = new List<string>();
            foreach (SubRecord sr in SubRecords) list.AddRange(sr.GetIDs(lower));
            return list;
        }

        #region Match subrecords
        class LoopContext
        {
            public enum LoopEvalResult
            {
                Failed, // Failed to properly match
                NoMatches, // no matches
                Success, // all matched
                Partial, // some matched
            }

            public int idx;
            public int matches;

            public int ssidx;
            public SubrecordBase[] sss;

            public LoopContext(int start, SubrecordBase[] sss)
            {
                this.idx = start;
                this.ssidx = 0;
                this.sss = sss;
                this.matches = 0;
            }
        }


        private LoopContext.LoopEvalResult InnerLoop(SubRecord[] subs, Dictionary<int, Conditional> conditions, LoopContext context)
        {
            while (true)
            {
                if (context.idx >= subs.Length || context.ssidx >= context.sss.Length)
                    return LoopContext.LoopEvalResult.Success;

                var ssb = context.sss[context.ssidx];
                var sb = subs[context.idx];
                if (ssb is SubrecordGroup)
                {
                    var sg = ssb as SubrecordGroup;
                    var newcontext = new LoopContext(context.idx, sg.elements);
                    LoopContext.LoopEvalResult result = InnerLoop(subs, conditions, newcontext);
                    if (context.idx == newcontext.idx)
                    {
                        if (ssb.optional > 0 || (ssb.repeat > 0 && context.matches > 0))
                        {
                            ++context.ssidx;
                            context.matches = 0;
                            continue;
                        }
                    }
                    else if (result == LoopContext.LoopEvalResult.Success)
                    {
                        if (ssb.repeat == 0)
                            ++context.ssidx;
                        else
                            ++context.matches;
                        context.idx = newcontext.idx;
                        continue;
                    }
                    break;
                }
                else if (ssb is SubrecordStructure)
                {
                    var ss = (SubrecordStructure)ssb;
                    if (ss.Condition != CondType.None && !MatchRecordCheckCondition(conditions, ss))
                    {
                        ++context.ssidx;
                        continue;
                    }

                    if (sb.Name == ss.name && (ss.size == 0 || ss.size == sb.Size))
                    {
                        sb.AttachStructure(ss);
                        if (ss.ContainsConditionals)
                        {
                            foreach (var elem in EnumerateElements(sb))
                            {
                                if (elem != null && elem.Structure != null)
                                {
                                    var es = elem.Structure;
                                    if (es.CondID != 0)
                                        conditions[es.CondID] = new Conditional(elem.Type, elem.Value);
                                }
                            }
                        }
                        ++context.idx;
                        if (ss.repeat == 0)
                        {
                            ++context.ssidx;
                            context.matches = 0;
                        }
                        else
                        {
                            // keep ss context and try again
                            ++context.matches;
                        }
                        continue;
                    }
                    else
                    {
                        if (ss.optional > 0 || (ss.repeat > 0 && context.matches > 0))
                        {
                            ++context.ssidx;
                            context.matches = 0;
                            continue;
                        }
                        else
                        {
                            // true failure
                            break;
                        }
                    }
                }
            }
            return LoopContext.LoopEvalResult.Failed;
        }

        private static bool MatchRecordCheckCondition(Dictionary<int, Conditional> conditions, SubrecordStructure ss)
        {
            if (ss.Condition == CondType.Exists)
            {
                if (conditions.ContainsKey(ss.CondID)) return true;
                else return false;
            }
            else if (ss.Condition == CondType.Missing)
            {
                if (conditions.ContainsKey(ss.CondID)) return false;
                else return true;
            }
            Conditional cond;
            if (!conditions.TryGetValue(ss.CondID, out cond))
                return false;
            switch (cond.type)
            {
                case ElementValueType.SByte:
                case ElementValueType.Byte:
                case ElementValueType.UShort:
                case ElementValueType.Short:
                case ElementValueType.Int:
                case ElementValueType.UInt:
                case ElementValueType.FormID:
                    {
                        int i = Convert.ToInt32(cond.value), i2;
                        if (!int.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }
                case ElementValueType.Float:
                    {
                        float i = (float)cond.value, i2;
                        if (!float.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }
                case ElementValueType.Str4:
                case ElementValueType.fstring:
                case ElementValueType.BString:
                case ElementValueType.String:
                    {
                        string s = (string)cond.value;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return s == ss.CondOperand;
                            case CondType.Not: return s != ss.CondOperand;
                            case CondType.StartsWith: return s.StartsWith(ss.CondOperand);
                            case CondType.EndsWith: return s.EndsWith(ss.CondOperand);
                            case CondType.Contains: return s.Contains(ss.CondOperand);
                            default: return false;
                        }
                    }
                case ElementValueType.LString:
                    {
                        int i = (int)cond.value, i2;
                        if (!int.TryParse(ss.CondOperand, out i2)) return false;
                        switch (ss.Condition)
                        {
                            case CondType.Equal: return i == i2;
                            case CondType.Not: return i != i2;
                            case CondType.Less: return i < i2;
                            case CondType.Greater: return i > i2;
                            case CondType.GreaterEqual: return i >= i2;
                            case CondType.LessEqual: return i <= i2;
                            default: return false;
                        }
                    }

                default: return false;
            }
        }

        /// <summary>
        /// Routine to match subrecord definitions to subrecord instances
        /// </summary>
        /// <returns></returns>
        public bool MatchRecordStructureToRecord()
        {
            try
            {
                if (RecordStructure.Records == null) return false;
                RecordStructure rs;
                if (!RecordStructure.Records.TryGetValue(this.Name, out rs))
                    return false;

                var subrecords = new List<SubrecordStructure>();
                var sss = rs.subrecordTree;
                var subs = this.SubRecords.ToArray();
                foreach (var sub in subs) sub.DetachStructure();
                Dictionary<int, Conditional> conditions = new Dictionary<int, Conditional>();
                var context = new LoopContext(0, sss);
                var result = InnerLoop(subs, conditions, context);
                if (result == LoopContext.LoopEvalResult.Success && context.idx == subs.Length)
                    return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="rawData">Retain raw data instead of converting to more usuable form</param>
        /// <returns></returns>
        internal IEnumerable<Element> EnumerateElements(SubRecord sr, bool rawData)
        {
            if (sr == null) return new Element[0];
            return sr.EnumerateElements(rawData);
        }

        internal IEnumerable<Element> EnumerateElements(SubRecord sr)
        {
            return EnumerateElements(sr, false);
        }

        internal IEnumerable<Element> EnumerateElements(SubRecord sr, Dictionary<int, Conditional> conditions)
        {
            if (sr == null) return new Element[0];
            return sr.EnumerateElements(conditions);
        }
        #endregion
    }
    #endregion

    #region class SubRecord
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public sealed class SubRecord : BaseRecord
    {
        private Record Owner;

        [Persistable]
        private byte[] Data;

        public override long Size { get { return Data.Length; } }
        public override long Size2 { get { return 6 + Data.Length + (Data.Length > ushort.MaxValue ? 10 : 0); } }

        public byte[] GetData()
        {
            return (byte[])Data.Clone();
        }
        internal byte[] GetReadonlyData() { return Data; }
        public void SetData(byte[] data)
        {
            Data = (byte[])data.Clone();
        }
        public void SetStrData(string s, bool nullTerminate)
        {
            if (nullTerminate) s += '\0';
            Data = System.Text.Encoding.Default.GetBytes(s);
        }

        SubRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Create new subrecord using Structure as template
        /// </summary>
        /// <param name="srs"></param>
        internal SubRecord(SubrecordStructure srs)
            : this()
        {
            if (srs != null)
            {
                Name = srs.name;
                int size = 0;
                if (srs.size > 0)
                    size = srs.size;
                else
                {
                    foreach (var elem in srs.elements)
                    {
                        if (!elem.optional || elem.repeat == 0)
                        {
                            switch (elem.type)
                            {
                                case ElementValueType.FormID:
                                case ElementValueType.LString:
                                case ElementValueType.Int:
                                case ElementValueType.UInt:
                                case ElementValueType.Float:
                                case ElementValueType.Str4:
                                    size += 4;
                                    break;
                                case ElementValueType.BString:
                                case ElementValueType.Short:
                                case ElementValueType.UShort:
                                    size += 2;
                                    break;
                                case ElementValueType.String:
                                case ElementValueType.fstring:
                                case ElementValueType.Byte:
                                case ElementValueType.SByte:
                                    size += 1;
                                    break;
                            }
                        }
                    }
                }
                this.Data = new byte[size];
                // TODO: populate with defaults if provided...
            }
        }

        internal SubRecord(Record rec, string name, BinaryReader br, uint size)
        {
            Owner = rec;
            Name = name;
            if (size == 0) size = br.ReadUInt16(); else br.BaseStream.Position += 2;
            Data = new byte[size];
            br.Read(Data, 0, Data.Length);
        }

        private SubRecord(SubRecord sr)
        {
            Owner = null;
            Name = sr.Name;
            Data = (byte[])sr.Data.Clone();
        }

        public override BaseRecord Clone()
        {
            return new SubRecord(this);
        }

        public SubRecord()
        {
            Name = "NEW_";
            Data = new byte[0];
            Owner = null;
        }

        internal override void SaveData(BinaryWriter bw)
        {
            if (Data.Length > ushort.MaxValue)
            {
                WriteString(bw, "XXXX");
                bw.Write((ushort)4);
                bw.Write(Data.Length);
                WriteString(bw, Name);
                bw.Write((ushort)0);
                bw.Write(Data, 0, Data.Length);
            }
            else
            {
                WriteString(bw, Name);
                bw.Write((ushort)Data.Length);
                bw.Write(Data, 0, Data.Length);
            }
        }

        public override string GetDesc()
        {
            return "[Subrecord]" + Environment.NewLine +
                "Name: " + Name + Environment.NewLine +
                "Size: " + Size.ToString() + " bytes (Excluding header)";
        }
        public override bool DeleteRecord(BaseRecord br) { return false; }
        public override void AddRecord(BaseRecord br)
        {
            throw new TESParserException("Subrecords cannot contain additional data.");
        }
        public string GetStrData()
        {
            string s = "";
            foreach (byte b in Data)
            {
                if (b == 0) break;
                s += (char)b;
            }
            return s;
        }
        public string GetStrData(int id)
        {
            string s = "";
            foreach (byte b in Data)
            {
                if (b == 0) break;
                s += (char)b;
            }
            return s;
        }
        public string GetHexData()
        {
            string s = "";
            foreach (byte b in Data) s += b.ToString("X").PadLeft(2, '0') + " ";
            return s;
        }

        public string Description
        {
            get { return this.Structure != null ? this.Structure.desc : ""; }
        }

        public bool IsValid
        {
            get { return this.Structure != null && (this.Structure.size == 0 || this.Structure.size == this.Size); }
        }

        internal SubrecordStructure Structure { get; private set; }

        internal void AttachStructure(SubrecordStructure ss)
        {
            this.Structure = ss;
        }
        internal void DetachStructure()
        {
            this.Structure = null;
        }

        internal string GetFormattedData(SelectionContext context)
        {
            var sb = new System.Text.StringBuilder();
            GetFormattedData(sb, context);
            return sb.ToString();
        }

        #region Get Formatted Data
        public override void GetFormattedData(System.Text.StringBuilder s, SelectionContext context)
        {
            SubrecordStructure ss = this.Structure;
            if (ss == null)
                return;

            dFormIDLookupI formIDLookup = context.formIDLookup;
            dLStringLookup strLookup = context.strLookup;
            dFormIDLookupR formIDLookupR = context.formIDLookupR;

            int offset = 0;
            s.AppendFormat("{0} ({1})", ss.name, ss.desc);
            s.AppendLine();
            try
            {
                for (int eidx = 0, elen = 1; eidx < ss.elements.Length; eidx += elen)
                {
                    var sselem = ss.elements[eidx];
                    bool repeat = sselem.repeat > 0;
                    elen = sselem.repeat > 1 ? sselem.repeat : 1;
                    do
                    {
                        for (int eoff = 0; eoff < elen && offset < Data.Length; ++eoff)
                        {
                            sselem = ss.elements[eidx + eoff];

                            if (offset == Data.Length && eidx == ss.elements.Length - 1 && sselem.optional) break;
                            if (!sselem.notininfo) s.Append(sselem.name).Append(": ");

                            switch (sselem.type)
                            {
                                case ElementValueType.Int:
                                    {

                                        string tmps = TypeConverter.h2si(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString("X8"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 4;
                                    } break;
                                case ElementValueType.UInt:
                                    {
                                        string tmps = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString("X8"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 4;
                                    }
                                    break;
                                case ElementValueType.Short:
                                    {
                                        string tmps = TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString("X4"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1]) s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 2;
                                    }
                                    break;
                                case ElementValueType.UShort:
                                    {
                                        string tmps = TypeConverter.h2s(Data[offset], Data[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2s(Data[offset], Data[offset + 1]).ToString("X4"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1]) s.Append(" (").Append(sselem.options[k]).Append(")");
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 2;
                                    }
                                    break;
                                case ElementValueType.Byte:
                                    {
                                        string tmps = Data[offset].ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(Data[offset].ToString("X2"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = Data[offset];
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0) s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset++;
                                    }
                                    break;
                                case ElementValueType.SByte:
                                    {
                                        string tmps = ((sbyte)Data[offset]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(Data[offset].ToString("X2"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = Data[offset];
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0) s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset++;
                                    }
                                    break;
                                case ElementValueType.FormID:
                                    {
                                        uint id = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                        if (!sselem.notininfo) s.Append(id.ToString("X8"));
                                        if (id != 0 && formIDLookup != null) s.Append(": ").Append(formIDLookup(id));
                                        offset += 4;
                                    } break;
                                case ElementValueType.Float:
                                    if (!sselem.notininfo) s.Append(TypeConverter.h2f(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]));
                                    offset += 4;
                                    break;
                                case ElementValueType.String:
                                    if (!sselem.notininfo)
                                    {
                                        while (Data[offset] != 0) s.Append((char)Data[offset++]);
                                    }
                                    else
                                    {
                                        while (Data[offset] != 0) offset++;
                                    }
                                    offset++;
                                    break;
                                case ElementValueType.fstring:
                                    if (!sselem.notininfo) s.Append(GetStrData());
                                    offset += Data.Length - offset;
                                    break;
                                case ElementValueType.Blob:
                                    if (!sselem.notininfo) s.Append(TypeConverter.GetHexData(Data, offset, Data.Length - offset));
                                    offset += Data.Length - offset;
                                    break;
                                case ElementValueType.BString:
                                    {
                                        int len = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                        if (!sselem.notininfo)
                                            s.Append(TESVSnip.Encoding.CP1252.GetString(Data, offset + 2, len));
                                        offset += (2 + len);
                                    }
                                    break;
                                case ElementValueType.LString:
                                    {
                                        // Try to guess if string or string index.  Do not know if the external string checkbox is set or not in this code
                                        int left = Data.Length - offset;
                                        var data = new ArraySegment<byte>(Data, offset, left);
                                        bool isString = TypeConverter.IsLikelyString(data);
                                        uint id = TypeConverter.h2i(data);
                                        string lvalue = strLookup(id);
                                        if (!string.IsNullOrEmpty(lvalue) || !isString)
                                        {
                                            if (!sselem.notininfo) s.Append(id.ToString("X8"));
                                            if (strLookup != null) s.Append(": ").Append(lvalue);
                                            offset += 4;
                                        }
                                        else
                                        {
                                            if (!sselem.notininfo)
                                                while (Data[offset] != 0) s.Append((char)Data[offset++]);
                                            else
                                                while (Data[offset] != 0) offset++;
                                            offset++;
                                        }
                                    } break;
                                case ElementValueType.Str4:
                                    {
                                        if (!sselem.notininfo)
                                            s.Append(TESVSnip.Encoding.CP1252.GetString(Data, offset, 4));
                                        offset += 4;
                                    }
                                    break;
                                default:
                                    throw new ApplicationException();
                            }
                            if (!sselem.notininfo) s.AppendLine();
                        }
                    } while (repeat && offset < Data.Length);
                }

                if (offset < Data.Length)
                {
                    s.AppendLine();
                    s.AppendLine("Remaining Data: ");
                    s.Append(TypeConverter.GetHexData(Data, offset, Data.Length - offset));
                }
            }
            catch
            {
                s.AppendLine("Warning: Subrecord doesn't seem to match the expected structure");
            }
        }

        public static RTFBuilderbase AppendLink(RTFBuilderbase s, string text, string hyperlink)
        {
            if (global::TESVSnip.Properties.Settings.Default.DisableHyperlinks)
                s.Append(text);
            else
                s.AppendLink(text, hyperlink);
            return s;
        }

        public override void GetFormattedHeader(RTF.RTFBuilder s, SelectionContext context)
        {
            s.FontStyle(FontStyle.Bold).FontSize(s.DefaultFontSize + 4).ForeColor(KnownColor.DarkGray).AppendLine("[Subrecord data]");
        }

        public override void GetFormattedData(RTF.RTFBuilder s, SelectionContext context)
        {
            SubrecordStructure ss = this.Structure;
            if (ss == null || ss.elements == null)
            {
                s.Append("String:\t").AppendLine(this.GetStrData()).AppendLine();
                s.Append("Hex: \t").AppendLine(this.GetHexData());
                s.AppendPara();
                return;
            }

            bool addTerminatingParagraph = false;
            try
            {
                var formIDLookup = context.formIDLookup;
                var strLookup = context.strLookup;
                var formIDLookupR = context.formIDLookupR;

                // Table of items
                var table = new List<List<RTFCellDefinition>>();

                // set up elements
                float maxWidth = 0;
                int maxFirstCellWidth = 0;

                var elems = EnumerateElements(true).Where( x => x.Structure != null && !x.Structure.notininfo).ToList();
                if (elems.Count == 0)
                    return;

                foreach (var element in elems)
                {
                    Size sz = s.MeasureText(element.Structure.name);
                    int width = Math.Max(sz.Width / 11, 10); // approximate convert pixels to twips as the rtflib has crap documentation
                    if (width > maxFirstCellWidth)
                        maxFirstCellWidth = width;
                }

                foreach (var element in elems)
                {
                    var row = new List<RTFCellDefinition>();
                    table.Add(row);
                    var sselem = element.Structure;
                    bool hasOptions = (sselem.options != null && sselem.options.Length > 0);
                    bool hasFlags = (sselem.flags != null && sselem.flags.Length > 1);

                    // setup borders for header
                    var value = element.Value;
                    var nameCell = new RTFCellDefinition(maxFirstCellWidth, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty);
                    row.Add(nameCell);
                    switch (sselem.type)
                    {
                        case ElementValueType.FormID:
                            row.Add(new RTFCellDefinition(12, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            // Optional Add cell for 
                            break;
                        case ElementValueType.LString:
                            row.Add(new RTFCellDefinition(12, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;

                        case ElementValueType.BString:
                        case ElementValueType.String:
                        case ElementValueType.fstring:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;
                        case ElementValueType.Int:
                        case ElementValueType.UInt:
                        case ElementValueType.Byte:
                        case ElementValueType.SByte:
                        case ElementValueType.Short:
                        case ElementValueType.UShort:
                        case ElementValueType.Float:
                            row.Add(new RTFCellDefinition(12, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, hasOptions || hasFlags ? RTFBorderSide.Default : RTFBorderSide.Default
                                , 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;
                        case ElementValueType.Blob:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;
                        case ElementValueType.Str4:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;
                        default:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty));
                            break;
                    }
                    maxWidth = Math.Max(maxWidth, row.Sum(x => x.CellWidthRaw));
                }

                int rowWidth = (int)(maxWidth * 100.0f);
                var p = new System.Windows.Forms.Padding { All = 50 };

                var hdrd = new RTFRowDefinition(rowWidth, RTFAlignment.TopLeft, RTFBorderSide.Default, 15, SystemColors.WindowText, p);
                var hdrcds = new RTFCellDefinition[] {
                    new RTFCellDefinition(rowWidth, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, System.Windows.Forms.Padding.Empty)
                };

                addTerminatingParagraph = true;
                s.Reset();
                using (IRTFRow ie = s.CreateRow(hdrd, hdrcds))
                {
                    foreach (var item in ie)
                    {
                        var rb = item.Content;
                        item.Content
                            .FontSize(s.DefaultFontSize + 1)
                            .FontStyle(FontStyle.Bold)
                            .ForeColor(KnownColor.DarkCyan)
                            .AppendFormat("{0} ({1})", ss.name, ss.desc);
                    }
                }
                for (int rowIdx = 0; rowIdx < elems.Count; ++rowIdx)
                {
                    var rd = new RTFRowDefinition(rowWidth, RTFAlignment.TopLeft, RTFBorderSide.Default, 15, SystemColors.WindowText, p);
                    var cds = table[rowIdx];
                    var elem = elems[rowIdx];
                    var sselem = elem.Structure;
                    var value = elem.Value;
                    Record rec = null;
                    string strValue = null; // value to display
                    string strDesc = null; // first description
                    string strDesc2 = null; // second description
                    bool hasOptions = (sselem.options != null && sselem.options.Length > 0);
                    bool hasFlags = (sselem.flags != null && sselem.flags.Length > 1);

                    // Pre row write caching to avoid expensive duplicate calls between cells
                    switch (sselem.type)
                    {
                        case ElementValueType.FormID:
                            {
                                uint id = (uint)value;
                                strValue = id.ToString("X8");
                                if (id != 0)
                                rec = formIDLookupR != null ? formIDLookupR(id) : null;
                                if (rec != null)
                                {
                                    strDesc = rec.DescriptiveName;
                                    var full = rec.SubRecords.FirstOrDefault(x => x.Name == "FULL");
                                    if (full != null) //  split the cell 2 in 2 if full name found
                                    {
                                        var data = new ArraySegment<byte>(full.Data, 0, full.Data.Length);
                                        bool isString = TypeConverter.IsLikelyString(data);
                                        string lvalue = (isString)
                                            ? full.GetStrData()
                                            : strLookup != null
                                            ? strLookup(TypeConverter.h2i(data))
                                            : null;
                                        if (!string.IsNullOrEmpty(lvalue))
                                        {
                                            var first = cds[cds.Count - 1];
                                            Size sz = s.MeasureText(lvalue);
                                            int width = Math.Min(40, Math.Max(sz.Width / 12, 10)); // approximate convert pixels to twips as the rtflib has crap documentation
                                            var second = new RTFCellDefinition(width, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 0, Color.DarkGray, System.Windows.Forms.Padding.Empty);
                                            cds.Add(second);
                                            strDesc2 = lvalue;
                                        }
                                    }
                                }
                            } break;
                        case ElementValueType.LString:
                            {
                                if (elem.Type == ElementValueType.String)
                                {
                                    strValue = "";
                                    strDesc = value.ToString();
                                }
                                else if (TypeConverter.IsLikelyString(elem.Data))
                                {
                                    strValue = "";
                                    strDesc = TypeConverter.GetString(elem.Data);
                                }
                                else
                                {
                                    uint id = TypeConverter.h2i(elem.Data);
                                    strValue = id.ToString("X8");
                                    strDesc = strLookup != null ? strLookup(id) : null;
                                }
                            } break;
                        case ElementValueType.Blob:
                            strValue = TypeConverter.GetHexData(elem.Data);
                            break;
                        case ElementValueType.Int:
                        case ElementValueType.UInt:
                        case ElementValueType.Byte:
                        case ElementValueType.SByte:
                        case ElementValueType.Short:
                        case ElementValueType.UShort:
                            {
                                if (sselem.hexview || hasFlags)
                                    strValue = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count * 2), value);
                                else
                                    strValue = value == null ? "" : value.ToString();
                                if (hasOptions)
                                {
                                    int intVal = Convert.ToInt32(value);
                                    for (int k = 0; k < sselem.options.Length; k += 2)
                                    {
                                        if (intVal == int.Parse(sselem.options[k + 1]))
                                            strDesc = sselem.options[k];
                                    }
                                }
                                else if (hasFlags)
                                {
                                    uint intVal = Convert.ToUInt32(value);
                                    var tmp2 = new System.Text.StringBuilder();
                                    for (int k = 0; k < sselem.flags.Length; k++)
                                    {
                                        if ((intVal & (1 << k)) != 0)
                                        {
                                            if (tmp2.Length > 0) tmp2.Append(", ");
                                            tmp2.Append(sselem.flags[k]);
                                        }
                                    }
                                    strDesc = tmp2.ToString();
                                }
                            } break;
                        case ElementValueType.Str4:
                            strValue = TypeConverter.GetString(elem.Data);
                            break;
                        case ElementValueType.BString:
                            strValue = TypeConverter.GetBString(elem.Data);
                            break;
                        default:
                            strValue = value == null ? "" : value.ToString();
                            break;
                    }

                    // Now create row and fill in cells
                    using (IRTFRow ie = s.CreateRow(rd, cds))
                    {
                        int colIdx = 0;
                        IEnumerator<IBuilderContent> ie2 = ie.GetEnumerator();
                        for (bool ok = ie2.MoveNext(); ok; ok = ie2.MoveNext(), ++colIdx)
                        {
                            using (var item = ie2.Current)
                            {
                                var rb = item.Content;
                                if (colIdx == 0) // name
                                {
                                    rb.FontStyle(FontStyle.Bold).Append(sselem.name);
                                }
                                else if (colIdx == 1) // value
                                {
                                    switch (sselem.type)
                                    {
                                        case ElementValueType.FormID:
                                            if (((uint)value) == 0)
                                            {
                                                rb.Append(strValue);
                                            }
                                            else if (rec != null)
                                            {
                                                AppendLink(rb, strValue, string.Format("{0}:{1}", rec.Name, strValue));
                                            }
                                            else if (!string.IsNullOrEmpty(sselem.FormIDType))
                                            {
                                                AppendLink(rb, strValue, string.Format("{0}:{1}", sselem.FormIDType, strValue));
                                            }
                                            else
                                            {
                                                AppendLink(rb, strValue, string.Format("XXXX:{0}", strValue));
                                            }
                                            break;
                                        default:
                                            rb.Append(strValue);
                                            break;
                                    }
                                }
                                else if (colIdx == 2) // desc
                                {
                                    if (!string.IsNullOrEmpty(strDesc))
                                        rb.Append(strDesc);
                                }
                                else if (colIdx == 3) // desc2
                                {
                                    if (!string.IsNullOrEmpty(strDesc2))
                                        rb.Append(strDesc2);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                s.AppendLine("Warning: Subrecord doesn't seem to match the expected structure");
            }
            finally
            {
                if (addTerminatingParagraph)
                {
                    s.Reset();
                    s.AppendPara();
                }
            }
        }
#if false
        public void GetFormattedDataOriginal(RTF.RTFBuilder s, SelectionContext context)
        {
            SubrecordStructure ss = this.Structure;
            if (ss == null || ss.elements == null)
            {
                s.Append("String:\t").AppendLine(this.GetStrData()).AppendLine();
                s.Append("Hex: \t").AppendLine(this.GetHexData());
                return;
            }

            dFormIDLookupI formIDLookup = context.formIDLookup;
            dLStringLookup strLookup = context.strLookup;
            dFormIDLookupR formIDLookupR = context.formIDLookupR;

            int offset = 0;

            s.FontSize(s.DefaultFontSize + 1).FontStyle(FontStyle.Bold).AppendFormat("{0} ({1})", ss.name, ss.desc);
            s.AppendLine();
            try
            {
                for (int eidx = 0, elen = 1; eidx < ss.elements.Length; eidx += elen)
                {
                    var sselem = ss.elements[eidx];
                    bool repeat = sselem.repeat > 0;
                    elen = sselem.repeat > 1 ? sselem.repeat : 1;
                    do
                    {
                        for (int eoff = 0; eoff < elen && offset < Data.Length; ++eoff)
                        {
                            sselem = ss.elements[eidx + eoff];

                            if (offset == Data.Length && eidx == ss.elements.Length - 1 && sselem.optional) break;

                            if (!sselem.notininfo)
                                s.FontStyle(FontStyle.Bold).Append(sselem.name).Append(":\t");

                            switch (sselem.type)
                            {
                                case ElementValueType.Int:
                                    {

                                        string tmps = TypeConverter.h2si(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString("X8"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 1)
                                            {
                                                uint val = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 4;
                                    } break;
                                case ElementValueType.UInt:
                                    {
                                        string tmps = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]).ToString("X8"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 4;
                                    }
                                    break;
                                case ElementValueType.Short:
                                    {
                                        string tmps = TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2ss(Data[offset], Data[offset + 1]).ToString("X4"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1]) s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 2;
                                    }
                                    break;
                                case ElementValueType.UShort:
                                    {
                                        string tmps = TypeConverter.h2s(Data[offset], Data[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(TypeConverter.h2s(Data[offset], Data[offset + 1]).ToString("X4"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1]) s.Append(" (").Append(sselem.options[k]).Append(")");
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0)
                                                    s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset += 2;
                                    }
                                    break;
                                case ElementValueType.Byte:
                                    {
                                        string tmps = Data[offset].ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(Data[offset].ToString("X2"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = Data[offset];
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0) s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset++;
                                    }
                                    break;
                                case ElementValueType.SByte:
                                    {
                                        string tmps = ((sbyte)Data[offset]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview) s.Append(Data[offset].ToString("X2"));
                                            else s.Append(tmps);
                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = Data[offset];
                                                var tmp2 = new System.Text.StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0) tmp2.Append(", ");
                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }
                                                if (tmp2.Length > 0) s.AppendFormat(" ({0})", tmp2);
                                            }
                                        }
                                        offset++;
                                    }
                                    break;
                                case ElementValueType.FormID:
                                    {
                                        uint id = TypeConverter.h2i(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]);
                                        if (!sselem.notininfo)
                                        {
                                            var strid = id.ToString("X8");
                                            if (id != 0 && formIDLookupR != null)
                                            {
                                                var rec = formIDLookupR(id);
                                                if (rec != null)
                                                {
                                                    AppendLink(s, strid, string.Format("{0}:{1}", rec.Name, strid));
                                                    var strval = rec.DescriptiveName;
                                                    if (!string.IsNullOrEmpty(strval))
                                                        s.Append(":\t").Append(strval);
                                                    else
                                                        s.Append(":\t").Append(formIDLookup(id));
                                                    var full = rec.SubRecords.FirstOrDefault(x => x.Name == "FULL");
                                                    if (full != null)
                                                    {
                                                        var data = new ArraySegment<byte>(full.Data, 0, full.Data.Length);
                                                        bool isString = TypeConverter.IsLikelyString(data);
                                                        string lvalue = (isString)
                                                            ? full.GetStrData()
                                                            : strLookup != null
                                                            ? strLookup(TypeConverter.h2i(data))
                                                            : null;
                                                        if (!string.IsNullOrEmpty(lvalue))
                                                            s.Append("\t").Append(lvalue);
                                                    }
                                                }
                                                else if (formIDLookup != null)
                                                {
                                                    var strval = formIDLookup(id);
                                                    AppendLink(s, strid, string.Format("XXXX:{0}", strid));
                                                    if (!string.IsNullOrEmpty(strval))
                                                        s.Append(":\t").Append(strval);
                                                    else
                                                        s.Append(":\t").Append(formIDLookup(id));
                                                }
                                                else
                                                {
                                                    AppendLink(s, strid, string.Format("XXXX:{0}", strid));
                                                }
                                            }
                                            else
                                            {
                                                AppendLink(s, strid, string.Format("XXXX:{0}", strid));
                                            }
                                        }
                                        offset += 4;
                                    } break;
                                case ElementValueType.Float:
                                    if (!sselem.notininfo) s.Append(TypeConverter.h2f(Data[offset], Data[offset + 1], Data[offset + 2], Data[offset + 3]));
                                    offset += 4;
                                    break;
                                case ElementValueType.String:
                                    if (!sselem.notininfo)
                                    {
                                        while (Data[offset] != 0) s.Append((char)Data[offset++]);
                                    }
                                    else
                                    {
                                        while (Data[offset] != 0) offset++;
                                    }
                                    offset++;
                                    break;
                                case ElementValueType.fstring:
                                    if (!sselem.notininfo) s.Append(GetStrData());
                                    offset += Data.Length - offset;
                                    break;
                                case ElementValueType.Blob:
                                    if (!sselem.notininfo) s.Append(TypeConverter.GetHexData(Data, offset, Data.Length - offset));
                                    offset += Data.Length - offset;
                                    break;
                                case ElementValueType.BString:
                                    {
                                        int len = TypeConverter.h2s(Data[offset], Data[offset + 1]);
                                        if (!sselem.notininfo)
                                            s.Append(TESVSnip.Encoding.CP1252.GetString(Data, offset + 2, len));
                                        offset += (2 + len);
                                    }
                                    break;
                                case ElementValueType.LString:
                                    {
                                        // Try to guess if string or string index.  Do not know if the external string checkbox is set or not in this code
                                        int left = Data.Length - offset;
                                        var data = new ArraySegment<byte>(Data, offset, left);
                                        bool isString = TypeConverter.IsLikelyString(data);
                                        uint id = TypeConverter.h2i(data);
                                        string lvalue = strLookup(id);
                                        if (!string.IsNullOrEmpty(lvalue) || !isString)
                                        {
                                            if (!sselem.notininfo) s.Append(id.ToString("X8"));
                                            if (strLookup != null) s.Append(":\t").Append(lvalue);
                                            offset += 4;
                                        }
                                        else
                                        {
                                            if (!sselem.notininfo)
                                                while (Data[offset] != 0) s.Append((char)Data[offset++]);
                                            else
                                                while (Data[offset] != 0) offset++;
                                            offset++;
                                        }
                                    } break;
                                case ElementValueType.Str4:
                                    {
                                        if (!sselem.notininfo)
                                            s.Append(TESVSnip.Encoding.CP1252.GetString(Data, offset, 4));
                                        offset += 4;
                                    }
                                    break;
                                default:
                                    throw new ApplicationException();
                            }
                            if (!sselem.notininfo) s.AppendLine();
                        }
                    } while (repeat && offset < Data.Length);
                }

                if (offset < Data.Length)
                {
                    s.AppendLine();
                    s.AppendLine("Remaining Data: ");
                    s.Append(TypeConverter.GetHexData(Data, offset, Data.Length - offset));
                }
            }
            catch
            {
                s.AppendLine("Warning: Subrecord doesn't seem to match the expected structure");
            }
        }
#endif
        #endregion

        public bool TryGetValue<T>(int offset, out T value)
        {
            value = (T)TypeConverter.GetObject<T>(this.Data, offset);
            return true;
        }

        public T GetValue<T>(int offset)
        {
            T value;
            if (!TryGetValue<T>(offset, out value))
                value = default(T);
            return value;
        }


        internal override List<string> GetIDs(bool lower)
        {
            List<string> list = new List<string>();
            if (Name == "EDID")
            {
                if (lower)
                {
                    list.Add(this.GetStrData().ToLower());
                }
                else
                {
                    list.Add(this.GetStrData());
                }
            }
            return list;
        }

        #region Enumerate Elements
        internal IEnumerable<Element> EnumerateElements()
        {
            return EnumerateElements(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="rawData">Retain raw data instead of converting to more usuable form</param>
        /// <returns></returns>
        internal IEnumerable<Element> EnumerateElements(bool rawData)
        {
            if (this.Structure != null)
            {
                byte[] data = this.GetReadonlyData();
                var ss = this.Structure;
                int offset = 0;
                foreach (var es in ss.elements)
                {
                    while (true)
                    {
                        int startoffset = offset;
                        int maxlen = data.Length - offset;
                        if ((es.optional || es.repeat > 0) && maxlen == 0) break;
                        var elem = Element.CreateElement(es, data, ref offset, rawData);
                        yield return elem;
                        if (es.repeat > 0 && startoffset < offset)
                            continue;
                        break;
                    }
                }
            }
        }
        internal IEnumerable<Element> EnumerateElements(Dictionary<int, Conditional> conditions)
        {
            foreach (var elem in EnumerateElements())
            {
                if (elem != null && elem.Structure != null)
                {
                    var es = elem.Structure;
                    var essCondID = es.CondID;
                    if (essCondID != 0)
                        conditions[essCondID] = new Conditional(elem.Type, elem.Value);
                }
                yield return elem;
            }
        }
        #endregion

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description) && this.Description != this.Name)
                return this.Name;
            return string.Format("{0}: {1}", this.Name, this.Description);
        }
    }
    #endregion


    /// <summary>
    /// Helper for reference to Element structure including data
    /// </summary>
    internal sealed class Element
    {
        TESVSnip.ElementValueType type = ElementValueType.Blob;

        public Element() { }

        public static Element CreateElement(TESVSnip.ElementStructure es, byte[] data, ref int offset, bool rawData)
        {
            int maxlen = data.Length - offset;
            int len;
            Element elem = null;
            try
            {
                switch (es.type)
                {
                    case ElementValueType.Int:
                        len = maxlen >= sizeof(int) ? sizeof(int) : maxlen;
                        elem = new Element(es, ElementValueType.UInt, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.UInt:
                    case ElementValueType.FormID:
                        len = maxlen >= sizeof(uint) ? sizeof(uint) : maxlen;
                        elem = new Element(es, ElementValueType.UInt, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.Float:
                        len = maxlen >= sizeof(float) ? sizeof(float) : maxlen;
                        elem = new Element(es, ElementValueType.Float, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.Short:
                        len = maxlen >= sizeof(short) ? sizeof(short) : maxlen;
                        elem = new Element(es, ElementValueType.Short, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.UShort:
                        len = maxlen >= sizeof(ushort) ? sizeof(ushort) : maxlen;
                        elem = new Element(es, ElementValueType.UShort, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.SByte:
                        len = maxlen >= sizeof(sbyte) ? sizeof(sbyte) : maxlen;
                        elem = new Element(es, ElementValueType.SByte, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.Byte:
                        len = maxlen >= sizeof(byte) ? sizeof(byte) : maxlen;
                        elem = new Element(es, ElementValueType.SByte, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;
                    case ElementValueType.String:
                        len = 0;
                        for (int i = offset; i < data.Length && data[i] != 0; ++i, ++len) ;
                        if (rawData) // raw form includes the zero termination byte
                        {
                            len = (len == 0 ? 0 : len + 1);
                            elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, len));
                            offset += len;
                        }
                        else
                        {
                            elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, len));
                            offset += (len == 0 ? 0 : len + 1);
                        }
                        break;
                    case ElementValueType.fstring:
                        if (rawData) // raw form includes the zero termination byte
                        {
                            elem = new Element(es, ElementValueType.fstring, new ArraySegment<byte>(data, offset, maxlen));
                            offset += maxlen;
                        }
                        else
                        {
                            elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, maxlen));
                            offset += maxlen;
                        }
                        break;
                    case ElementValueType.BString:
                        if (maxlen >= sizeof(ushort))
                        {
                            len = TypeConverter.h2s(data[offset], data[offset + 1]);
                            len = (len < maxlen - 2) ? len : maxlen - 2;
                            if (rawData) // raw data includes short prefix
                            {
                                elem = new Element(es, ElementValueType.BString, new ArraySegment<byte>(data, offset, len + 2));
                                offset += (len + 2);
                            }
                            else
                            {
                                elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset + 2, len));
                                offset += (len + 2);
                            }
                        }
                        else
                        {
                            if (rawData)
                                elem = new Element(es, ElementValueType.BString, new ArraySegment<byte>(new byte[2] { 0, 0 }));
                            else
                                elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(new byte[0]));
                            offset += maxlen;
                        }
                        break;
                    case ElementValueType.Str4:
                        len = maxlen >= 4 ? 4 : maxlen;
                        if (rawData)
                            elem = new Element(es, ElementValueType.Str4, new ArraySegment<byte>(data, offset, len));
                        else
                            elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, len));
                        offset += len;
                        break;

                    case ElementValueType.LString:
                        if (maxlen < sizeof(int))
                        {
                            elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, maxlen));
                            offset += maxlen;
                        }
                        else
                        {
                            len = maxlen;
                            var blob = new ArraySegment<byte>(data, offset, len);
                            bool isString = TypeConverter.IsLikelyString(blob);
                            if (!isString)
                            {
                                elem = new Element(es, ElementValueType.UInt, new ArraySegment<byte>(data, offset, len));
                                offset += 4;
                            }
                            else
                            {
                                len = 0;
                                for (int i = offset; i < data.Length && data[i] != 0; ++i, ++len) ;
                                if (rawData) // lstring as raw string includes the terminating null
                                {
                                    len = (len == 0 ? 0 : len + 1);
                                    elem = new Element(es, ElementValueType.LString, new ArraySegment<byte>(data, offset, len));
                                    offset += len;
                                }
                                else
                                {
                                    elem = new Element(es, ElementValueType.String, new ArraySegment<byte>(data, offset, len));
                                    offset += (len == 0 ? 0 : len + 1);
                                }
                            }
                        }
                        break;

                    default:
                        elem = new Element(es, ElementValueType.Blob, new ArraySegment<byte>(data, offset, maxlen));
                        offset += maxlen;
                        break;
                }
            }
            catch
            {

            }
            finally
            {
                if (offset > data.Length) offset = data.Length;
            }
            return elem;
        }

        public Element(TESVSnip.ElementStructure es, byte[] data, int offset, int count)
            : this(es, new ArraySegment<byte>(data, offset, count))
        {
        }

        public Element(TESVSnip.ElementStructure es, ArraySegment<byte> data)
        {
            this.Structure = es;
            this.Data = data;
        }

        public Element(TESVSnip.ElementStructure es, ElementValueType vt, ArraySegment<byte> data)
        {
            this.Structure = es;
            this.Data = data;
            this.type = vt;
        }

        public TESVSnip.ElementValueType Type
        {
            get { return Structure == null && type == ElementValueType.Blob ? Structure.type : type; }
        }

        public ArraySegment<byte> Data { get; private set; }

        public TESVSnip.ElementStructure Structure { get; private set; }

        public object Value
        {
            get
            {
                switch (this.Type)
                {
                    case ElementValueType.Int:
                        return TypeConverter.h2si(this.Data);
                    case ElementValueType.UInt:
                    case ElementValueType.FormID:
                        return TypeConverter.h2i(this.Data);
                    case ElementValueType.Float:
                        return TypeConverter.h2f(this.Data);
                    case ElementValueType.Short:
                        return TypeConverter.h2ss(this.Data);
                    case ElementValueType.UShort:
                        return TypeConverter.h2s(this.Data);
                    case ElementValueType.SByte:
                        return TypeConverter.h2sb(this.Data);
                    case ElementValueType.Byte:
                        return TypeConverter.h2b(this.Data);
                    case ElementValueType.String:
                        return TypeConverter.GetString(this.Data);
                    case ElementValueType.fstring:
                        return TypeConverter.GetString(this.Data);
                    default:
                        if (this.Data.Offset == 0 && this.Data.Count == this.Data.Array.Length)
                            return this.Data.Array;
                        var b = new byte[this.Data.Count];
                        Array.Copy(this.Data.Array, this.Data.Offset, b, 0, this.Data.Count);
                        return b;
                }
            }
        }
    }


    #region Misc Flag Defs
    internal static class FlagDefs
    {
        public static readonly string[] RecFlags1 = {
            "ESM file",
            null,
            null,
            null,
            null,
            "Deleted",
            null,
            null,
            null,
            "Casts shadows",
            "Quest item / Persistent reference",
            "Initially disabled",
            "Ignored",
            null,
            null,
            "Visible when distant",
            null,
            "Dangerous / Off limits (Interior cell)",
            "Data is compressed",
            "Can't wait",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
        };

        public static string GetRecFlags1Desc(uint flags)
        {
            string desc = "";
            bool b = false;
            for (int i = 0; i < 32; i++)
            {
                if ((flags & (uint)(1 << i)) > 0)
                {
                    if (b) desc += ", ";
                    b = true;
                    desc += (RecFlags1[i] == null ? "Unknown (" + ((uint)(1 << i)).ToString("x") + ")" : RecFlags1[i]);
                }
            }
            return desc;
        }
    }
    #endregion
}
