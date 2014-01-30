using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TESVSnip.Domain.Data.RecordStructure;
using TESVSnip.Domain.Model;
using TESVSnip.Domain.Scripts;
using TESVSnip.Framework;
using TESVSnip.Framework.Services;
using TESVSnip.Properties;
using TESVSnip.UI.Rendering.Extensions;

namespace TESVSnip.UI.Rendering.Extensions
{
    static class StringRenderer
    {
        public static void GetFormattedData(this BaseRecord rec, StringBuilder sb)
        {
            if (rec is Record)
                ((Record)rec).GetFormattedData(sb);
            else if (rec is SubRecord)
                ((SubRecord) rec).GetFormattedData(sb);
            else
                sb.Append(rec.GetDesc());
        }
        public static string GetDesc(this BaseRecord rec)
        {
            if (rec is PluginList)
                return ((PluginList)rec).GetDesc();
            if (rec is Plugin)
                return ((Plugin)rec).GetDesc();
            if (rec is GroupRecord)
                return ((GroupRecord)rec).GetDesc();
            if (rec is Record)
                return ((Record)rec).GetDesc();
            if (rec is SubRecord)
                return ((SubRecord)rec).GetDesc();
            return "";
        }



        internal static string GetDesc(this Record rec, ISelectionContext context)
        {
            string start = "[Record]" + Environment.NewLine + rec.GetBaseDesc();
            string end;
            try
            {
                end = rec.GetExtendedDesc(context);
            }
            catch
            {
                end = "Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml";
            }

            if (end == null)
            {
                return start;
            }
            else
            {
                return start + Environment.NewLine + Environment.NewLine + "[Formatted information]" + Environment.NewLine + end;
            }
        }


        internal static string GetBaseDesc(this Record rec)
        {
            return "Type: " + rec.Name + Environment.NewLine + "FormID: " + rec.FormID.ToString("x8") + Environment.NewLine + "Flags 1: " + rec.Flags1.ToString("x8")
                   + (rec.Flags1 == 0 ? string.Empty : " (" + FlagDefs.GetRecFlags1Desc(rec.Flags1) + ")") + Environment.NewLine + "Flags 2: " + rec.Flags2.ToString("x8") + Environment.NewLine + "Flags 3: "
                   + rec.Flags3.ToString("x8") + Environment.NewLine + "Subrecords: " + rec.SubRecords.Count.ToString() + Environment.NewLine + "Size: " + rec.Size.ToString()
                   + " bytes (excluding header)";
        }

        internal static string GetExtendedDesc(this Record rec, ISelectionContext selectContext)
        {
            var context = selectContext.Clone();
            try
            {
                context.Record = rec;
                RecordStructure structure;
                if (!RecordStructure.Records.TryGetValue(rec.Name, out structure))
                {
                    return string.Empty;
                }

                var s = new StringBuilder();
                s.AppendLine(structure.description);
                foreach (var subrec in rec.SubRecords)
                {
                    if (subrec.Structure == null)
                    {
                        continue;
                    }

                    if (subrec.Structure.elements == null)
                    {
                        return s.ToString();
                    }

                    if (subrec.Structure.notininfo)
                    {
                        continue;
                    }

                    s.AppendLine();
                    s.Append(subrec.GetFormattedData());
                }

                return s.ToString();
            }
            finally
            {
                context.Reset();
            }
        }


        public static void GetFormattedData(this SubRecord rec, StringBuilder s)
        {
            SubrecordStructure ss = rec.Structure;
            if (ss == null)
            {
                return;
            }

            var p = rec.GetPlugin();
            dFormIDLookupI formIDLookup = p.LookupFormID;
            dLStringLookup strLookup = p.LookupFormStrings;
            dFormIDLookupR formIDLookupR = p.GetRecordByID;

            var recdata = rec.GetReadonlyData();
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
                        for (int eoff = 0; eoff < elen && offset < recdata.Length; ++eoff)
                        {
                            sselem = ss.elements[eidx + eoff];

                            if (offset == recdata.Length && eidx == ss.elements.Length - 1 && sselem.optional > 0)
                            {
                                break;
                            }

                            if (!sselem.notininfo)
                            {
                                s.Append(sselem.name).Append(": ");
                            }

                            switch (sselem.type)
                            {
                                case ElementValueType.Int:
                                    {
                                        string tmps = TypeConverter.h2si(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]).ToString("X8"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]);
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset += 4;
                                    }

                                    break;
                                case ElementValueType.UInt:
                                    {
                                        string tmps = TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]).ToString("X8"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]);
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset += 4;
                                    }

                                    break;
                                case ElementValueType.Short:
                                    {
                                        string tmps = TypeConverter.h2ss(recdata[offset], recdata[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(TypeConverter.h2ss(recdata[offset], recdata[offset + 1]).ToString("X4"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(recdata[offset], recdata[offset + 1]);
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset += 2;
                                    }

                                    break;
                                case ElementValueType.UShort:
                                    {
                                        string tmps = TypeConverter.h2s(recdata[offset], recdata[offset + 1]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(TypeConverter.h2s(recdata[offset], recdata[offset + 1]).ToString("X4"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.Append(" (").Append(sselem.options[k]).Append(")");
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                uint val = TypeConverter.h2s(recdata[offset], recdata[offset + 1]);
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset += 2;
                                    }

                                    break;
                                case ElementValueType.Byte:
                                    {
                                        string tmps = recdata[offset].ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(recdata[offset].ToString("X2"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = recdata[offset];
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset++;
                                    }

                                    break;
                                case ElementValueType.SByte:
                                    {
                                        string tmps = ((sbyte)recdata[offset]).ToString();
                                        if (!sselem.notininfo)
                                        {
                                            if (sselem.hexview)
                                            {
                                                s.Append(recdata[offset].ToString("X2"));
                                            }
                                            else
                                            {
                                                s.Append(tmps);
                                            }

                                            if (sselem.options != null && sselem.options.Length > 0)
                                            {
                                                for (int k = 0; k < sselem.options.Length; k += 2)
                                                {
                                                    if (tmps == sselem.options[k + 1])
                                                    {
                                                        s.AppendFormat(" ({0})", sselem.options[k]);
                                                    }
                                                }
                                            }
                                            else if (sselem.flags != null && sselem.flags.Length > 0)
                                            {
                                                int val = recdata[offset];
                                                var tmp2 = new StringBuilder();
                                                for (int k = 0; k < sselem.flags.Length; k++)
                                                {
                                                    if ((val & (1 << k)) != 0)
                                                    {
                                                        if (tmp2.Length > 0)
                                                        {
                                                            tmp2.Append(", ");
                                                        }

                                                        tmp2.Append(sselem.flags[k]);
                                                    }
                                                }

                                                if (tmp2.Length > 0)
                                                {
                                                    s.AppendFormat(" ({0})", tmp2);
                                                }
                                            }
                                        }

                                        offset++;
                                    }

                                    break;
                                case ElementValueType.FormID:
                                    {
                                        uint id = TypeConverter.h2i(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]);
                                        if (!sselem.notininfo)
                                        {
                                            s.Append(id.ToString("X8"));
                                        }

                                        if (id != 0 && formIDLookup != null)
                                        {
                                            s.Append(": ").Append(formIDLookup(id));
                                        }

                                        offset += 4;
                                    }

                                    break;
                                case ElementValueType.Float:
                                    if (!sselem.notininfo)
                                    {
                                        s.Append(TypeConverter.h2f(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]));
                                    }

                                    offset += 4;
                                    break;
                                case ElementValueType.String:
                                    if (!sselem.notininfo)
                                    {
                                        while (recdata[offset] != 0)
                                        {
                                            s.Append((char)recdata[offset++]);
                                        }
                                    }
                                    else
                                    {
                                        while (recdata[offset] != 0)
                                        {
                                            offset++;
                                        }
                                    }

                                    offset++;
                                    break;
                                case ElementValueType.Blob:
                                    if (!sselem.notininfo)
                                    {
                                        s.Append(TypeConverter.GetHexData(recdata, offset, recdata.Length - offset));
                                    }

                                    offset += recdata.Length - offset;
                                    break;
                                case ElementValueType.BString:
                                    {
                                        int len = TypeConverter.h2s(recdata[offset], recdata[offset + 1]);
                                        if (!sselem.notininfo)
                                        {
                                            s.Append(TESVSnip.Framework.Services.Encoding.Instance.GetString(recdata, offset + 2, len));
                                        }

                                        offset += 2 + len;
                                    }

                                    break;
                                case ElementValueType.IString:
                                    {
                                        int len = TypeConverter.h2si(recdata[offset], recdata[offset + 1], recdata[offset + 2], recdata[offset + 3]);
                                        if (!sselem.notininfo)
                                        {
                                            s.Append(TESVSnip.Framework.Services.Encoding.Instance.GetString(recdata, offset + 4, len));
                                        }

                                        offset += 4 + len;
                                    }

                                    break;
                                case ElementValueType.LString:
                                    {
                                        // Try to guess if string or string index.  Do not know if the external string checkbox is set or not in this code
                                        int left = recdata.Length - offset;
                                        var data = new ArraySegment<byte>(recdata, offset, left);
                                        bool isString = TypeConverter.IsLikelyString(data);
                                        uint id = TypeConverter.h2i(data);
                                        string lvalue = strLookup(id);
                                        if (!string.IsNullOrEmpty(lvalue) || !isString)
                                        {
                                            if (!sselem.notininfo)
                                            {
                                                s.Append(id.ToString("X8"));
                                            }

                                            if (strLookup != null)
                                            {
                                                s.Append(": ").Append(lvalue);
                                            }

                                            offset += 4;
                                        }
                                        else
                                        {
                                            if (!sselem.notininfo)
                                            {
                                                while (recdata[offset] != 0)
                                                {
                                                    s.Append((char)recdata[offset++]);
                                                }
                                            }
                                            else
                                            {
                                                while (recdata[offset] != 0)
                                                {
                                                    offset++;
                                                }
                                            }

                                            offset++;
                                        }
                                    }

                                    break;
                                case ElementValueType.Str4:
                                    {
                                        if (!sselem.notininfo)
                                        {
                                            s.Append(TESVSnip.Framework.Services.Encoding.Instance.GetString(recdata, offset, 4));
                                        }

                                        offset += 4;
                                    }

                                    break;
                                default:
                                    throw new ApplicationException();
                            }

                            if (!sselem.notininfo)
                            {
                                s.AppendLine();
                            }
                        }
                    }
                    while (repeat && offset < recdata.Length);
                }

                if (offset < recdata.Length)
                {
                    s.AppendLine();
                    s.AppendLine("Remaining Data: ");
                    s.Append(TypeConverter.GetHexData(recdata, offset, recdata.Length - offset));
                }
            }
            catch
            {
                s.AppendLine("Warning: Subrecord doesn't seem to match the expected structure");
            }
        }

        private static string GetSubDesc(this GroupRecord rec)
        {
            var recdata = rec.GetReadonlyData();
            switch (rec.groupType)
            {
                case 0:
                    return "(Contains: " + (char)recdata[0] + (char)recdata[1] + (char)recdata[2] + (char)recdata[3] + ")";
                case 2:
                case 3:
                    return "(Block number: " + (recdata[0] + recdata[1] * 256 + recdata[2] * 256 * 256 + recdata[3] * 256 * 256 * 256).ToString() + ")";
                case 4:
                case 5:
                    return "(Coordinates: [" + (recdata[0] + recdata[1] * 256) + ", " + recdata[2] + recdata[3] * 256 + "])";
                case 1:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    return "(Parent FormID: 0x" + recdata[3].ToString("x2") + recdata[2].ToString("x2") + recdata[1].ToString("x2") + recdata[0].ToString("x2") + ")";
            }

            return null;
        }
        public static string GetDesc(this GroupRecord rec)
        {
            string desc = "[Record group]" + Environment.NewLine + "Record type: ";
            switch (rec.groupType)
            {
                case 0:
                    desc += "Top " + rec.GetSubDesc();
                    break;
                case 1:
                    desc += "World children " + rec.GetSubDesc();
                    break;
                case 2:
                    desc += "Interior Cell Block " + rec.GetSubDesc();
                    break;
                case 3:
                    desc += "Interior Cell Sub-Block " + rec.GetSubDesc();
                    break;
                case 4:
                    desc += "Exterior Cell Block " + rec.GetSubDesc();
                    break;
                case 5:
                    desc += "Exterior Cell Sub-Block " + rec.GetSubDesc();
                    break;
                case 6:
                    desc += "Cell Children " + rec.GetSubDesc();
                    break;
                case 7:
                    desc += "Topic Children " + rec.GetSubDesc();
                    break;
                case 8:
                    desc += "Cell Persistent Children " + rec.GetSubDesc();
                    break;
                case 9:
                    desc += "Cell Temporary Children " + rec.GetSubDesc();
                    break;
                case 10:
                    desc += "Cell Visible Distant Children " + rec.GetSubDesc();
                    break;
                default:
                    desc += "Unknown";
                    break;
            }

            return desc + Environment.NewLine + "Records: " + rec.Records.Count.ToString() + Environment.NewLine + "Size: " + rec.Size.ToString() + " bytes (including header)";
        }
        public static string GetDesc(this Plugin rec)
        {
            return "[Skyrim plugin]" + Environment.NewLine + "Filename: " + rec.Name + Environment.NewLine + "File size: " + rec.Size + Environment.NewLine + "Records: " + rec.Records.Count;
        }

        public static string GetDesc(this PluginList rec)
        {
            return "Master List";
        }

        public static string GetDesc(this Record rec)
        {
            return "[Record]" + Environment.NewLine + rec.GetBaseDesc();
        }

        public static string GetDesc(this SubRecord rec)
        {
            return "[Subrecord]" + Environment.NewLine + "Name: " + rec.Name + Environment.NewLine + "Size: " + rec.Size.ToString() + " bytes (Excluding header)";
        }

        public static string GetFormattedData(this SubRecord rec)
        {
            var sb = new StringBuilder();
            rec.GetFormattedData(sb);
            return sb.ToString();
        }

    }
}
