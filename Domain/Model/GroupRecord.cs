using FalloutSnip.Domain.Services;

namespace FalloutSnip.Domain.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    using Data.Structure;
    using FalloutSnip.Framework.Persistence;
    using FalloutSnip.Framework.Services;

    [Persistable(Flags = PersistType.DeclaredOnly)]
    [Serializable]
    public sealed class GroupRecord : Rec, IDeserializationCallback, IGroupRecord
    {
        [Persistable]
        public uint dateStamp;

        [Persistable]
        public uint flags;

        [Persistable]
        public uint groupType;

        [Persistable]
        private readonly byte[] data;

        [Persistable]
        private readonly List<Rec> records = new List<Rec>(1);

        public GroupRecord(string data)
        {
            Name = "GRUP";
            this.data = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                this.data[i] = (byte)data[i];
            }

            this.UpdateShortDescription();
        }

        internal GroupRecord(uint Size, BinaryReader br, FalloutSnip.Domain.Data.DomainDefinition define, Func<string, bool> recFilter, bool filterAll)
        {
            Name = "GRUP";
            this.data = br.ReadBytes(4);
            this.groupType = br.ReadUInt32();
            this.dateStamp = br.ReadUInt32();
            string contentType = this.groupType == 0 ? Encoding.Instance.GetString(this.data) : string.Empty;
            if (define.RecSize >= 16)
            {
                this.flags = br.ReadUInt32();
            }

            uint amountRead = 0;
            while (amountRead < Size - (define.RecSize+8))
            {
                string s = ReadRecName(br);
                uint recsize = br.ReadUInt32();
                if (s == "GRUP")
                {
                    try
                    {
                        bool skip = filterAll || (recFilter != null && !recFilter(contentType));
                        var gr = new GroupRecord(recsize, br, define, recFilter, skip);
                        if (!filterAll)
                        {
                            this.AddRecord(gr);
                        }
                    }
                    catch (Exception e)
                    {
                        Alerts.Show(e.Message);
                    }
                    finally
                    {
                        amountRead += recsize;
                    }
                }
                else
                {
                    bool skip = filterAll || (recFilter != null && !recFilter(contentType));
                    if (skip)
                    {
                        long size = recsize + define.RecSize;

                        // if ((br.ReadUInt32() & 0x00040000) > 0) size += 4;
                        br.BaseStream.Position += size; // just read past the data
                        amountRead += (uint)(recsize + (define.RecSize+8));
                    }
                    else
                    {
                        try
                        {
                            var r = new Record(s, recsize, br, define);
                            this.AddRecord(r);
                        }
                        catch (Exception e)
                        {
                            Alerts.Show(e.Message);
                        }
                        finally
                        {
                            amountRead += (uint)(recsize + (define.RecSize+8));
                        }
                    }
                }
            }

            this.UpdateShortDescription();
            if (amountRead != (Size - (define.RecSize+8)))
            {
                throw new TESParserException(
                    string.Format("Record block did not match the size specified in the group header! Header Size={0:D} Group Size={1:D}", Size - (define.RecSize+8), amountRead));
            }
        }

        private GroupRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private GroupRecord(GroupRecord gr, bool recursive)
        {
            Name = "GRUP";
            this.data = (byte[])gr.data.Clone();
            this.groupType = gr.groupType;
            this.dateStamp = gr.dateStamp;
            this.flags = gr.flags;
            if (recursive)
            {
                this.records = new List<Rec>(gr.records.Count);
                for (int i = 0; i < gr.records.Count; i++)
                {
                    this.AddRecord(gr.records[i].Clone());
                }
            }

            Name = gr.Name;
            this.UpdateShortDescription();
        }

        public string ContentsType
        {
            get
            {
                return this.groupType == 0 ? Encoding.Instance.GetString(this.data, 0, 4) : string.Empty;
            }
        }

        public uint GroupType
        {
            get
            {
                return this.groupType;
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
                long size = 24;
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

        public override void AddRecord(BaseRecord br)
        {
            try
          {
            //var r = br as Rec;
            var r = br as Rec;
            if (r == null)
            {
              throw new TESParserException("Record to add was not of the correct type." + Environment.NewLine + "Groups can only hold records or other groups.");
            }

            r.Parent = this;
            this.records.Add(r);
            FireRecordListUpdate(this, this);
          }
          catch (Exception ex)
          {
              throw new TESParserException("GroupRecord.AddRecord: " + ex.Message);
          }
        }

        public override void AddRecords(IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to add was not of the correct type.\nGroups can only hold records or other groups.");
            }

            foreach (var r in br)
            {
                r.Parent = this;
            }

            this.records.AddRange(br.OfType<Rec>());
            FireRecordListUpdate(this, this);
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
            return new GroupRecord(this, recursive: true);
        }

        public override BaseRecord Clone(bool recursive)
        {
            return new GroupRecord(this, recursive);
        }

        public override bool DeleteRecord(BaseRecord br)
        {
            var r = br as Rec;
            if (r == null)
            {
                return false;
            }

            bool ok = this.records.Remove(r);
            if (ok)
            {
                r.Parent = null;
            }

            FireRecordListUpdate(this, this);
            FireRecordDeleted(this, r);
            return ok;
        }

        public override bool DeleteRecords(IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to delete was not of the correct type.\nGroups can only hold records or other groups.");
            }

            var ok = false;
            foreach (Rec r in from Rec r in br where this.records.Remove(r) select r)
            {
                ok = true;
                r.Parent = null;
                FireRecordDeleted(this, r);
            }

            FireRecordListUpdate(this, this);
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

        public byte[] GetData()
        {
            return (byte[])this.data.Clone();
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
                throw new TESParserException("Record to add was not of the correct type." + Environment.NewLine + "Groups can only hold records or other groups.");
            }

            r.Parent = this;
            if (idx < 0 || idx > this.records.Count)
            {
                idx = this.records.Count;
            }

            this.records.Insert(idx, r);
            FireRecordListUpdate(this, this);
        }

        public override void InsertRecords(int index, IEnumerable<BaseRecord> br)
        {
            if (br.Count(r => !(r is Record || r is GroupRecord)) > 0)
            {
                throw new TESParserException("Record to add was not of the correct type.\nGroups can only hold records or other groups.");
            }

            this.records.InsertRange(index, br.OfType<Rec>());
            FireRecordListUpdate(this, this);
        }

        public bool IsEquivalent(GroupRecord other)
        {
            return this.GroupType == other.GroupType && ByteArrayCompare(this.GetReadonlyData(), other.GetReadonlyData());
        }

        public void SetData(byte[] data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException("data length must be 4");
            }

            for (int i = 0; i < 4; i++)
            {
                this.data[i] = data[i];
            }
        }

        public override void UpdateShortDescription()
        {
            if (this.groupType == 0)
            {
                string data = Encoding.Instance.GetString(this.data);
                string desc = string.Format(" ({0})", data);
                if (this.groupType == 0)
                {
                    var rec = GetStructure();
                    if (rec != null && rec.description != data)
                        desc += " - " + rec.description;
                }
                descriptiveName = desc;
            }
            else
            {
                descriptiveName = string.Empty;
            }
        }

        public override bool While(Predicate<BaseRecord> action)
        {
            if (!base.While(action))
            {
                return false;
            }

            return this.Records.Cast<BaseRecord>().All(r => r.While(action));
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            foreach (BaseRecord rec in this.Records)
            {
                rec.Parent = this;
            }
        }

        internal override List<string> GetIDs(bool lower)
        {
            var list = new List<string>();
            foreach (Record r in this.Records)
            {
                list.AddRange(r.GetIDs(lower));
            }

            return list;
        }

        public byte[] GetReadonlyData()
        {
            return this.data;
        }

        internal override void SaveData(BinaryWriter writer)
        {
            long startpos = writer.BaseStream.Position;
            var svSize = (uint)this.Size;
            var svSize2 = (uint)this.Size2;
            WriteString(writer, "GRUP");
            writer.Write(svSize); // Write uncompressed size for now
            writer.Write(this.data);
            writer.Write(this.groupType);
            writer.Write(this.dateStamp);
            writer.Write(this.flags); // should this check for oblivion?
            foreach (Rec r in this.Records)
            {
                r.SaveData(writer);
            }

            writer.Flush();
            long curpos = writer.BaseStream.Position;
            var wrSize = (uint)(curpos - startpos);
            if (wrSize != svSize2)
            {
                // fix size due to compression
                writer.BaseStream.Position = startpos + 4;
                writer.Write(wrSize); // Write the actuall compressed group size
                writer.BaseStream.Position = curpos;
            }
        }

        private static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        [DllImport("msvcrt.dll")]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public Plugin GetPlugin()
        {
            BaseRecord tn = Parent;
            while (!(tn is Plugin) && tn != null)
                tn = tn.Parent;
            if (tn != null)
                return tn as Plugin;
            return null;
        }

        public Dictionary<string, RecordStructure> GetStructures()
        {
            var p = GetPlugin();
            if (p == null) return null;
            return p.GetRecordStructures();
        }

        public RecordStructure GetStructure()
        {
            var p = GetPlugin();
            if (p == null) return null;
            var structs = p.GetRecordStructures();
            RecordStructure recStruct;
            if (structs.TryGetValue(this.ContentsType, out recStruct))
                return recStruct;
            return null;
        }

        public override string ToString()
        {
            return string.Format("[Group] '{0}' : {1}", this.Name, this.DescriptiveName);
        }
    }
}
