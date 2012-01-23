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
}
