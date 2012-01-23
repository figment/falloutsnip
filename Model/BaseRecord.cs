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

    class RecordChangeEventArgs : EventArgs
    {
        BaseRecord record;
        public RecordChangeEventArgs(BaseRecord rec)
        {
            this.record = rec;
        }
        public BaseRecord Record
        {
            get { return this.record; }
        }
    }
    
    [Persistable(Flags = PersistType.DeclaredOnly), Serializable]
    public abstract class BaseRecord : PersistObject, ICloneable, ISerializable, IRecord
    {
        static BaseRecord[] emptyList = new BaseRecord[0];

        public abstract BaseRecord Parent { get; internal set; }
        public virtual IList Records { get { return emptyList; } }

        [Persistable]
        public virtual string Name { get; set; }

        /// <summary>
        /// Size of Data portion
        /// </summary>
        public abstract long Size { get; }
        /// <summary>
        /// Size of Data + Headers
        /// </summary>
        public abstract long Size2 { get; }

        public abstract string GetDesc();
        public virtual void GetFormattedHeader(RTFBuilder rb, SelectionContext context) { }
        public virtual void GetFormattedData(RTFBuilder rb, SelectionContext context) { rb.Append(GetDesc()); }
        public virtual void GetFormattedData(StringBuilder sb, SelectionContext context) { sb.Append(GetDesc()); }

        public abstract bool DeleteRecord(BaseRecord br);
        public abstract void AddRecord(BaseRecord br);
        public virtual void InsertRecord(int index, BaseRecord br) { AddRecord(br); }
        public virtual int IndexOf(BaseRecord br) { return -1; }

        public virtual void AddRecords(IEnumerable<BaseRecord> br) { foreach ( var r in br) AddRecord(r); }

        public virtual bool DeleteRecords(IEnumerable<BaseRecord> br)
        {
            return br.Aggregate(false, (current, r) => current | DeleteRecord(r));
        }

        public virtual void InsertRecords(int index, IEnumerable<BaseRecord> br) { foreach (var r in br.Reverse()) InsertRecord(index, r); }

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

        public override string ToString()
        {
            return this.DescriptiveName;
        }

        public virtual string DescriptiveName { get { return this.Name; } }
        public virtual void SetDescription(string value) { }
        public virtual void UpdateShortDescription() { }


        public static bool HoldUpdates { get; set; }

        protected static void FireRecordChangeUpdate(object sender, BaseRecord rec)
        {
            if (!HoldUpdates && RecordDescChanged != null)
            {
                RecordDescChanged(sender, new RecordChangeEventArgs(rec));
            }
        }
        protected static void FireRecordListUpdate(object sender, BaseRecord rec)
        {
            if (!HoldUpdates && ChildListChanged != null)
            {
                ChildListChanged(sender, new RecordChangeEventArgs(rec));
            }
        }
        protected static void FireRecordDeleted(object sender, BaseRecord rec)
        {
            if (!HoldUpdates && RecordDeleted != null)
            {
                RecordDeleted(sender, new RecordChangeEventArgs(rec));
            }            
        }

        /// <summary>
        /// Shared Record Change Notifications
        /// </summary>
        internal static event EventHandler<RecordChangeEventArgs> RecordDescChanged;

        /// <summary>
        /// Shared Record Change Notifications
        /// </summary>
        internal static event EventHandler<RecordChangeEventArgs> ChildListChanged;

        /// <summary>
        /// Shared Record Change Notifications
        /// </summary>
        internal static event EventHandler<RecordChangeEventArgs> RecordDeleted;
    }
}
