using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using TESVSnip.Domain.Data.Structure;
using TESVSnip.Framework;
using TESVSnip.Framework.Persistence;

namespace TESVSnip.Domain.Model
{
    [Persistable(Flags = PersistType.DeclaredOnly)]
    [Serializable]
    public sealed class SubRecord : BaseRecord
    {
        [Persistable] private byte[] Data;

        private Record Owner;

        public SubRecord()
        {
            Name = "NEW_";
            this.Data = new byte[0];
            this.Owner = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SubRecord" /> class.
        ///     Create new subrecord using Structure as template
        /// </summary>
        /// <param name="srs">
        /// </param>
        public SubRecord(SubrecordStructure srs)
            : this()
        {
            if (srs != null)
            {
                Name = srs.name;
                int size = 0;
                if (srs.size > 0)
                {
                    size = srs.size;
                }
                else
                {
                    foreach (var elem in srs.elements)
                    {
                        if (elem.optional == 0 || elem.repeat == 0)
                        {
                            switch (elem.type)
                            {
                                case ElementValueType.FormID:
                                case ElementValueType.LString:
                                case ElementValueType.Int:
                                case ElementValueType.UInt:
                                case ElementValueType.Float:
                                case ElementValueType.Str4:
                                case ElementValueType.IString:
                                    size += 4;
                                    break;
                                case ElementValueType.BString:
                                case ElementValueType.Short:
                                case ElementValueType.UShort:
                                    size += 2;
                                    break;
                                case ElementValueType.String:
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

        public SubRecord(Record rec, string name, BinaryReader br, uint size)
        {
            this.Owner = rec;
            Name = name;
            this.Data = new byte[size];
            br.Read(this.Data, 0, this.Data.Length);
        }

        private SubRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private SubRecord(SubRecord sr)
        {
            this.Owner = null;
            Name = sr.Name;
            this.Data = (byte[]) sr.Data.Clone();
        }

        public string Description
        {
            get { return this.Structure != null ? this.Structure.desc : string.Empty; }
        }

        public override string DescriptiveName
        {
            get
            {
                if (string.IsNullOrEmpty(this.Description) && this.Description != Name)
                {
                    return Name;
                }

                return string.Format("{0}: {1}", Name, this.Description);
            }
        }

        public bool IsValid
        {
            get { return this.Structure != null && (this.Structure.size == 0 || this.Structure.size == this.Size); }
        }

        public override BaseRecord Parent
        {
            get { return this.Owner; }

            internal set { this.Owner = value as Record; }
        }

        public override long Size
        {
            get { return this.Data.Length; }
        }

        public override long Size2
        {
            get { return 6 + this.Data.Length + (this.Data.Length > ushort.MaxValue ? 10 : 0); }
        }

        public SubrecordStructure Structure { get; private set; }

        public override void AddRecord(BaseRecord br)
        {
            throw new TESParserException("Subrecords cannot contain additional data.");
        }

        public override BaseRecord Clone()
        {
            return new SubRecord(this);
        }

        public override bool DeleteRecord(BaseRecord br)
        {
            return false;
        }

        public byte[] GetData()
        {
            return (byte[]) this.Data.Clone();
        }

        public string GetHexData()
        {
            string s = string.Empty;
            foreach (byte b in this.Data)
            {
                s += b.ToString("X").PadLeft(2, '0') + " ";
            }

            return s;
        }

        public string GetLString()
        {
            var data = new ArraySegment<byte>(this.GetReadonlyData());
            bool isString = (data.Count != 4) || TypeConverter.IsLikelyString(data);
            if (isString)
            {
                return TypeConverter.GetString(data);
            }
            else
            {
                uint id = TypeConverter.h2i(data);
                var p = this.GetPlugin();
                return p != null ? p.LookupFormStrings(id) : null;
            }
        }

        public Plugin GetPlugin()
        {
            BaseRecord tn = this.Owner;
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

        public string GetStrData()
        {
            // note that UTF8 is substantially slower on order of 5x or so in my test 
            //  but probably more correct for non-English

            // probably most efficient way to extract a byte encoded null terminated string I think
            //  using array search to find ending zero then limit to that or length
            int len = this.Data.Length;
            while (len > 0 && this.Data[len - 1] == 0) --len;
            return TESVSnip.Framework.Services.Encoding.Instance.GetString(this.Data, 0, len);
        }

        public T GetValue<T>(int offset)
        {
            T value;
            if (!this.TryGetValue(offset, out value))
            {
                value = default(T);
            }

            return value;
        }

        public void SetData(byte[] data)
        {
            this.Data = (byte[]) data.Clone();
        }

        public void SetStrData(string s, bool nullTerminate)
        {
            if (nullTerminate)
            {
                s += '\0';
            }

            this.Data = System.Text.Encoding.Default.GetBytes(s);
        }

        public bool TryGetValue<T>(int offset, out T value)
        {
            try
            {
                value = (T) TypeConverter.GetObject<T>(this.Data, offset);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public bool TrySetValue<T>(int offset, T value)
        {
            try
            {
                var data = new ArraySegment<byte>(this.Data, offset, Marshal.SizeOf(value));
                return TypeConverter.TrySetValue<T>(data, value);
            }
            catch
            {
                return false;
            }
        }

        public void SetValue<T>(int offset, T value)
        {
            var data = new ArraySegment<byte>(this.Data, offset, Marshal.SizeOf(value));
            TypeConverter.TrySetValue<T>(data, value);
        }

        internal void AttachStructure(SubrecordStructure ss)
        {
            this.Structure = ss;
        }

        public void DetachStructure()
        {
            this.Structure = null;
        }

        public IEnumerable<Element> EnumerateElements()
        {
            return this.EnumerateElements(false);
        }


        /// <summary>
        ///     Python helper function to unpack elements with references to data
        /// </summary>
        /// <returns></returns>
        public Element[] UnpackElements()
        {
            return this.EnumerateElements(false).ToArray();
        }

        /// <summary>
        ///     Python helper function to repack elements into data from array of elements
        ///     Note that passed in Elements are not updated
        /// </summary>
        public void PackElements(System.Collections.IEnumerable items)
        {
            var stream = new MemoryStream(this.Data.Length);
            foreach (Element elem in items)
                stream.Write(elem.Data.Array, elem.Data.Offset, elem.Data.Count);
            this.Data = stream.ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="rawData">
        ///     Retain raw data instead of converting to more usuable form
        /// </param>
        /// <returns>
        ///     The System.Collections.Generic.IEnumerable`1[T -&gt; TESVSnip.Element].
        /// </returns>
        public IEnumerable<Element> EnumerateElements(bool rawData)
        {
            if (this.Structure == null)
            {
                yield return new Element(new ElementStructure(), new ArraySegment<byte>(this.GetData()));
            }
            else
            {
                byte[] data = this.GetReadonlyData();
                int offset = 0;
                foreach (var item in EnumerateElements(this.Structure.elementTree, data, offset, rawData))
                    yield return item.Item2;
            }
        }

        /// <summary>
        ///     Enumerate Children.  Note the tuple of Offset, Element.  Kinda weird but need to keep track of location
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private IEnumerable<Tuple<int, Element>> EnumerateElements(IEnumerable<ElementBase> elements, byte[] data,
                                                                   int offset, bool rawData)
        {
            foreach (var es in elements)
            {
                int count = 0;
                while (true)
                {
                    int startoffset = offset;
                    int maxlen = data.Length - offset;
                    if ((es.optional > 0 || es.repeat > 0) && maxlen == 0)
                        break;
                    if (es is ElementGroup)
                    {
                        foreach (var item in EnumerateElements(((ElementGroup) es).elements, data, offset, rawData))
                        {
                            offset = item.Item1;
                            yield return item;
                        }
                    }
                    if (es is ElementStructure)
                    {
                        var elem = Element.CreateElement((ElementStructure) es, data, ref offset, rawData);
                        yield return new Tuple<int, Element>(offset, elem);
                    }
                    if (es.repeat == 0) break;
                    ++count;
                    if ((es.repeat == 1 || count < es.repeat) && startoffset < offset)
                        continue;
                    break;
                }
            }
        }

        internal IEnumerable<Element> EnumerateElements(Dictionary<int, Conditional> conditions)
        {
            foreach (var elem in this.EnumerateElements())
            {
                if (elem != null && elem.Structure != null)
                {
                    var es = elem.Structure;
                    var essCondID = es.CondID;
                    if (essCondID != 0)
                    {
                        conditions[essCondID] = new Conditional(elem.Type, elem.Value);
                    }
                }

                yield return elem;
            }
        }

        internal object GetCompareValue(Element se)
        {
            object value = se.Value;
            switch (se.Structure.type)
            {
                case ElementValueType.LString:
                    if (value is uint)
                    {
                        var p = this.GetPlugin();
                        if (p != null)
                        {
                            value = p.LookupFormStrings((uint) value) ?? value;
                        }
                    }

                    break;
            }

            return value;
        }

        public object GetDisplayValue(Element elem)
        {
            object value = elem.Value;

            var sselem = elem.Structure;
            Record rec = null;
            string strValue = null; // value to display
            bool hasOptions = sselem.options != null && sselem.options.Length > 0;
            bool hasFlags = sselem.flags != null && sselem.flags.Length > 1;
            var p = this.GetPlugin();

            switch (elem.Structure.type)
            {
                case ElementValueType.FormID:
                    {
                        var id = (uint) value;
                        strValue = id.ToString("X8");
                        if (id != 0)
                        {
                            rec = p.GetRecordByID(id);
                            if (rec != null)
                            {
                                strValue = string.Format("{0}: {1}", strValue, rec.DescriptiveName);
                            }
                        }

                        value = strValue;
                    }

                    break;
                case ElementValueType.LString:
                    if (value is uint)
                    {
                        if (p != null)
                        {
                            value = p.LookupFormStrings((uint) value) ?? value;
                        }
                    }

                    break;
                case ElementValueType.Blob:
                    value = TypeConverter.GetHexData(elem.Data);
                    break;
                case ElementValueType.SByte:
                case ElementValueType.Int:
                case ElementValueType.Short:
                    {
                        if (sselem.hexview || hasFlags)
                        {
                            value = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count*2), value);
                        }
                        else
                        {
                            value = value ?? string.Empty;
                        }

                        if (hasOptions)
                        {
                            int intVal;
                            if (sselem.hexview || hasFlags)
                            {
                                intVal = int.Parse(value.ToString(), NumberStyles.HexNumber);
                            }
                            else
                            {
                                intVal = Convert.ToInt32(value);
                            }

                            for (int k = 0; k < sselem.options.Length; k += 2)
                            {
                                if (intVal == int.Parse(sselem.options[k + 1]))
                                {
                                    value = sselem.options[k];
                                }
                            }
                        }
                        else if (hasFlags)
                        {
                            int intVal = int.Parse(value.ToString(), NumberStyles.HexNumber);
                            var tmp2 = new StringBuilder();
                            for (int k = 0; k < sselem.flags.Length; k++)
                            {
                                if ((intVal & (1 << k)) != 0)
                                {
                                    if (tmp2.Length > 0)
                                    {
                                        tmp2.Append(", ");
                                    }

                                    tmp2.Append(sselem.flags[k]);
                                }
                            }

                            tmp2.Insert(0, ": ");
                            tmp2.Insert(0, value.ToString());
                            value = tmp2.ToString();
                        }
                    }

                    break;
                case ElementValueType.UInt:
                case ElementValueType.Byte:
                case ElementValueType.UShort:
                    {
                        if (sselem.hexview || hasFlags)
                        {
                            value = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count*2), value);
                        }
                        else
                        {
                            value = value ?? string.Empty;
                        }

                        if (hasOptions)
                        {
                            uint intVal;
                            if (sselem.hexview || hasFlags)
                            {
                                intVal = uint.Parse(value.ToString(), NumberStyles.HexNumber);
                            }
                            else
                            {
                                intVal = Convert.ToUInt32(value);
                            }

                            for (int k = 0; k < sselem.options.Length; k += 2)
                            {
                                if (intVal == uint.Parse(sselem.options[k + 1]))
                                {
                                    value = sselem.options[k];
                                }
                            }
                        }
                        else if (hasFlags)
                        {
                            uint intVal = uint.Parse(value.ToString(), NumberStyles.HexNumber);
                            var tmp2 = new StringBuilder();
                            for (int k = 0; k < sselem.flags.Length; k++)
                            {
                                if ((intVal & (1 << k)) != 0)
                                {
                                    if (tmp2.Length > 0)
                                    {
                                        tmp2.Append(", ");
                                    }

                                    tmp2.Append(sselem.flags[k]);
                                }
                            }

                            tmp2.Insert(0, ": ");
                            tmp2.Insert(0, value.ToString());
                            value = tmp2.ToString();
                        }
                    }

                    break;
                case ElementValueType.Str4:
                    strValue = TypeConverter.GetString(elem.Data);
                    break;
                case ElementValueType.BString:
                    strValue = TypeConverter.GetBString(elem.Data);
                    break;
                case ElementValueType.IString:
                    strValue = TypeConverter.GetIString(elem.Data);
                    break;
                default:
                    strValue = value == null ? string.Empty : value.ToString();
                    break;
            }
            return value;
        }

        internal override List<string> GetIDs(bool lower)
        {
            var list = new List<string>();
            if (Name == "EDID")
            {
                list.Add(lower ? this.GetStrData().ToLower() : this.GetStrData());
            }
            return list;
        }

        public byte[] GetReadonlyData()
        {
            return this.Data;
        }

        internal override void SaveData(BinaryWriter writer)
        {
            if (this.Data.Length > ushort.MaxValue)
            {
                WriteString(writer, "XXXX");
                writer.Write((ushort) 4);
                writer.Write(this.Data.Length);
                WriteString(writer, Name);
                writer.Write((ushort) 0);
                writer.Write(this.Data, 0, this.Data.Length);
            }
            else
            {
                WriteString(writer, Name);
                writer.Write((ushort) this.Data.Length);
                writer.Write(this.Data, 0, this.Data.Length);
            }
        }

        public override string ToString()
        {
            return string.Format("[SubRecord] {0} [{1}]: {2} ", this.Name, this.Size,
                                 this.Structure != null ? this.Structure.desc : "");
        }
    }
}