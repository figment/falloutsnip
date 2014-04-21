using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RTF;
using TESVSnip.Domain.Data.Structure;
using TESVSnip.Domain.Model;
using TESVSnip.Domain.Scripts;
using TESVSnip.Framework;
using TESVSnip.Properties;

namespace TESVSnip.UI.Rendering.Extensions
{
    static class RTFRenderer
    {

        public static void GetFormattedData(this BaseRecord rec, RTFBuilder rb)
        {
            if (rec is Record )
                ((Record)rec).GetFormattedData(rb);
            else if (rec is SubRecord)
                ((SubRecord)rec).GetFormattedData(rb);
            else
                rb.Append(rec.GetDesc());
        }
        
        public static void GetFormattedHeader(this BaseRecord rec, RTFBuilder rb)
        {
            if (rec is Record)
                ((Record)rec).GetFormattedHeader(rb);
            else if (rec is SubRecord)
                ((SubRecord)rec).GetFormattedHeader(rb);
        }


        public static void GetFormattedData(this Record rec, RTFBuilder rb)
        {
            try
            {
                rb.FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize).ForeColor(KnownColor.DarkGray).AppendLine("[Formatted information]");
                rb.Reset();

                RecordStructure recordStructure;
                if (!RecordStructure.Records.TryGetValue(rec.Name, out recordStructure))
                {
                    return;
                }

                rb.FontStyle(FontStyle.Bold).ForeColor(KnownColor.DarkBlue).FontSize(rb.DefaultFontSize + 4).AppendLine(recordStructure.description);
                foreach (var subrec in rec.SubRecords)
                {
                    if (subrec.Structure == null || subrec.Structure.elements == null || subrec.Structure.notininfo)
                    {
                        continue;
                    }

                    rb.AppendLine();
                    subrec.GetFormattedData(rb);
                }
            }
            catch
            {
                rb.ForeColor(KnownColor.Red).Append("Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml");
            }
        }

        public static void GetFormattedHeader(this Record rec, RTFBuilder rb)
        {
            rb.FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 4).ForeColor(KnownColor.DarkGray).AppendLine("[Record]");

            rb.Append("Type: ").FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 2).AppendFormat("{0}", rec.Name).AppendLine();
            rb.Append("FormID: ").FontStyle(FontStyle.Bold).FontSize(rb.DefaultFontSize + 2).ForeColor(KnownColor.DarkRed).AppendFormat("{0:X8}", rec.FormID).AppendLine();

            if (rec.Flags1 != 0)
            {
                rb.AppendLineFormat("Flags 1: {0:X8} : ({1} : Level = {2})", rec.Flags1, FlagDefs.GetRecFlags1Desc(rec.Flags1), rec.CompressionLevel.ToString());
            }
            else
                rb.AppendLineFormat("Flags 1: {0:X8}", rec.Flags1);

            //rb.AppendLineFormat("OLD --> Flags 3: \t{0:X8}", rec.Flags3);
            rb.AppendLineFormat("Version Control Info: {0:X8}", rec.Flags2);

            //rb.AppendLineFormat("OLD --> Flags 3: \t{0:X8}", rec.Flags3);
            rb.AppendLineFormat("Flags 2: {0:X4}", (rec.Flags3 >> 16));
            rb.AppendLineFormat("Form Version: {0:X4} : {1}", ((rec.Flags3 << 16) >> 16), ((rec.Flags3 << 16) >> 16));

            rb.AppendLineFormat("Size: {0:N0}", rec.Size);
            rb.AppendLineFormat("Subrecords: {0}", rec.SubRecords.Count);
            rb.AppendPara();
        }

        public static void GetFormattedHeader(this SubRecord rec, RTFBuilder s)
        {
            s.FontStyle(FontStyle.Bold).FontSize(s.DefaultFontSize + 4).ForeColor(KnownColor.DarkGray).AppendLine("[Subrecord data]");
        }

        public static RTFBuilderbase AppendLink(RTFBuilderbase s, string text, string hyperlink)
        {
            if (Settings.Default.DisableHyperlinks)
            {
                s.Append(text);
            }
            else
            {
                s.AppendLink(text, hyperlink);
            }

            return s;
        }

        public static void GetFormattedData(this SubRecord rec, RTFBuilder s)
        {
            SubrecordStructure ss = rec.Structure;
            if (ss == null || ss.elements == null)
            {
                s.Append("String:\t").AppendLine(rec.GetStrData()).AppendLine();
                s.Append("Hex: \t").AppendLine(rec.GetHexData());
                s.AppendPara();
                return;
            }

            bool addTerminatingParagraph = false;
            try
            {
                var p = rec.GetPlugin();

                dFormIDLookupI formIDLookup = p.LookupFormID;
                dLStringLookup strLookup = p.LookupFormStrings;
                dFormIDLookupR formIDLookupR = p.GetRecordByID;

                // Table of items
                var table = new List<List<RTFCellDefinition>>();

                // set up elements
                float maxWidth = 0;
                int maxFirstCellWidth = 0;

                var elems = rec.EnumerateElements(true).Where(x => x.Structure != null && !x.Structure.notininfo).ToList();
                if (elems.Count == 0)
                {
                    return;
                }

                foreach (var element in elems)
                {
                    Size sz = s.MeasureText(element.Structure.name);
                    int width = Math.Max(sz.Width / 11, 10);

                    // approximate convert pixels to twips as the rtflib has crap documentation
                    if (width > maxFirstCellWidth)
                    {
                        maxFirstCellWidth = width;
                    }
                }

                foreach (var element in elems)
                {
                    var row = new List<RTFCellDefinition>();
                    table.Add(row);
                    var sselem = element.Structure;
                    bool hasOptions = sselem.options != null && sselem.options.Length > 0;
                    bool hasFlags = sselem.flags != null && sselem.flags.Length > 1;

                    // setup borders for header
                    var value = element.Value;
                    var nameCell = new RTFCellDefinition(maxFirstCellWidth, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty);
                    row.Add(nameCell);
                    switch (sselem.type)
                    {
                        case ElementValueType.FormID:
                            row.Add(new RTFCellDefinition(12, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));

                            // Optional Add cell for 
                            break;
                        case ElementValueType.LString:
                            row.Add(new RTFCellDefinition(12, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;

                        case ElementValueType.BString:
                        case ElementValueType.IString:
                        case ElementValueType.String:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;
                        case ElementValueType.Int:
                        case ElementValueType.UInt:
                        case ElementValueType.Byte:
                        case ElementValueType.SByte:
                        case ElementValueType.Short:
                        case ElementValueType.UShort:
                        case ElementValueType.Float:
                            row.Add(new RTFCellDefinition(20, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            row.Add(new RTFCellDefinition(30, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;
                        case ElementValueType.Blob:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;
                        case ElementValueType.Str4:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;
                        default:
                            row.Add(new RTFCellDefinition(42, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty));
                            break;
                    }

                    maxWidth = Math.Max(maxWidth, row.Sum(x => x.CellWidthRaw));
                }

                var rowWidth = (int)(maxWidth * 100.0f);
                var pd = new Padding { All = 50 };

                var hdrd = new RTFRowDefinition(rowWidth, RTFAlignment.TopLeft, RTFBorderSide.Default, 15, SystemColors.WindowText, pd);
                var hdrcds = new[] { new RTFCellDefinition(rowWidth, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 15, Color.DarkGray, Padding.Empty) };

                addTerminatingParagraph = true;
                s.Reset();
                using (IRTFRow ie = s.CreateRow(hdrd, hdrcds))
                {
                    foreach (var item in ie)
                    {
                        var rb = item.Content;
                        item.Content.FontSize(s.DefaultFontSize + 1).FontStyle(FontStyle.Bold).ForeColor(KnownColor.DarkCyan).AppendFormat("{0} ({1})", ss.name, ss.desc);
                    }
                }

                for (int rowIdx = 0; rowIdx < elems.Count; ++rowIdx)
                {
                    var rd = new RTFRowDefinition(rowWidth, RTFAlignment.TopLeft, RTFBorderSide.Default, 15, SystemColors.WindowText, pd);
                    var cds = table[rowIdx];
                    var elem = elems[rowIdx];
                    var sselem = elem.Structure;
                    var value = elem.Value;

                    string recprefix = null;
                    Record record = null;
                    string strValue = null; // value to display
                    string strDesc = null; // first description
                    string strDesc2 = null; // second description
                    bool hasOptions = sselem.options != null && sselem.options.Length > 0;
                    bool hasFlags = sselem.flags != null && sselem.flags.Length > 1;

                    if (!string.IsNullOrWhiteSpace(elem.Structure.funcr))
                    {
                        if (elem.Type == ElementValueType.Float) value = PyInterpreter.ExecuteFunction<float>(elem, FunctionOperation.ForReading);
                        else if (elem.Type == ElementValueType.Int) value = PyInterpreter.ExecuteFunction<int>(elem, FunctionOperation.ForReading);
                        else if (elem.Type == ElementValueType.Short) value = PyInterpreter.ExecuteFunction<short>(elem, FunctionOperation.ForReading);
                        else if (elem.Type == ElementValueType.UShort) value = PyInterpreter.ExecuteFunction<ushort>(elem, FunctionOperation.ForReading);
                        else if (elem.Type == ElementValueType.UInt) value = PyInterpreter.ExecuteFunction<uint>(elem, FunctionOperation.ForReading);
                    }

                    // Pre row write caching to avoid expensive duplicate calls between cells
                    switch (sselem.type)
                    {
                        case ElementValueType.FormID:
                            {
                                var id = (uint)value;
                                strValue = id.ToString("X8");
                                if (id != 0)
                                {
                                    record = formIDLookupR != null ? formIDLookupR(id) : null;
                                }

                                if (record != null)
                                {
                                    var pref = record.GetPlugin();
                                    recprefix = pref != null ? string.Format("{0}@{1}", pref.Name, record.Name) : record.Name;
                                    strDesc = record.DescriptiveName;
                                    var full = record.SubRecords.FirstOrDefault(x => x.Name == "FULL");
                                    if (full != null)
                                    {
                                        // split the cell 2 in 2 if full name found
                                        var data = new ArraySegment<byte>(full.GetReadonlyData());
                                        bool isString = TypeConverter.IsLikelyString(data);
                                        string lvalue = isString ? full.GetStrData() : strLookup != null ? strLookup(TypeConverter.h2i(data)) : null;
                                        if (!string.IsNullOrEmpty(lvalue))
                                        {
                                            var first = cds[cds.Count - 1];
                                            Size sz = s.MeasureText(lvalue);
                                            int width = Math.Min(40, Math.Max(sz.Width / 12, 10));

                                            // approximate convert pixels to twips as the rtflib has crap documentation
                                            var second = new RTFCellDefinition(width, RTFAlignment.MiddleLeft, RTFBorderSide.Default, 0, Color.DarkGray, Padding.Empty);
                                            cds.Add(second);
                                            strDesc2 = lvalue;
                                        }
                                    }
                                }
                            }

                            break;
                        case ElementValueType.LString:
                            {
                                if (elem.Type == ElementValueType.String)
                                {
                                    strValue = string.Empty;
                                    strDesc = value.ToString();
                                }
                                else if (TypeConverter.IsLikelyString(elem.Data))
                                {
                                    strValue = string.Empty;
                                    strDesc = TypeConverter.GetString(elem.Data);
                                }
                                else
                                {
                                    uint id = TypeConverter.h2i(elem.Data);
                                    strValue = id.ToString("X8");
                                    strDesc = strLookup != null ? strLookup(id) : null;
                                }
                            }

                            break;
                        case ElementValueType.Blob:
                            strValue = TypeConverter.GetHexData(elem.Data);
                            break;
                        case ElementValueType.SByte:
                        case ElementValueType.Int:
                        case ElementValueType.Short:
                            {
                                if (sselem.hexview || hasFlags)
                                {
                                    if (sselem.hexviewwithdec)
                                        strValue = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count * 2), value) + string.Format(" : {0}", value);
                                    else
                                        strValue = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count * 2), value);
                                }
                                else
                                {
                                    strValue = value == null ? string.Empty : value.ToString();
                                }

                                if (hasOptions)
                                {
                                    int intVal = Convert.ToInt32(value);
                                    for (int k = 0; k < sselem.options.Length; k += 2)
                                    {
                                        int intValOption;
                                        if (int.TryParse(sselem.options[k + 1], out intValOption) && intVal == intValOption)
                                        {
                                            strDesc = sselem.options[k];
                                        }
                                    }
                                }
                                else if (hasFlags)
                                {
                                    int intVal = Convert.ToInt32(value);
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

                                    strDesc = tmp2.ToString();
                                }
                            }

                            break;

                        case ElementValueType.UInt:
                        case ElementValueType.Byte:
                        case ElementValueType.UShort:
                            {
                                if (sselem.hexview || hasFlags)
                                {
                                    if (sselem.hexviewwithdec)
                                        strValue = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count * 2), value) + string.Format(" : {0}", value);
                                    else
                                        strValue = string.Format(string.Format("{{0:X{0}}}", elem.Data.Count * 2), value);
                                }
                                else
                                {
                                    strValue = value == null ? string.Empty : value.ToString();
                                }

                                if (hasOptions)
                                {
                                    uint intVal = Convert.ToUInt32(value);
                                    for (int k = 0; k < sselem.options.Length; k += 2)
                                    {
                                        if (intVal == uint.Parse(sselem.options[k + 1]))
                                        {
                                            strDesc = sselem.options[k];
                                        }
                                    }
                                }
                                else if (hasFlags)
                                {
                                    uint intVal = Convert.ToUInt32(value);
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

                                    strDesc = tmp2.ToString();
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
                                if (colIdx == 0)
                                {
                                    // name
                                    rb.FontStyle(FontStyle.Bold).Append(sselem.name);
                                }
                                else if (colIdx == 1)
                                {
                                    // value
                                    switch (sselem.type)
                                    {
                                        case ElementValueType.FormID:
                                            if (((uint)value) == 0)
                                            {
                                                rb.Append(strValue);
                                            }
                                            else if (record != null)
                                            {
                                                RTFRenderer.AppendLink(rb, strValue, record.GetLink());
                                            }
                                            else if (!string.IsNullOrEmpty(sselem.FormIDType))
                                            {
                                                RTFRenderer.AppendLink(rb, strValue, string.Format("{0}:{1}", sselem.FormIDType, strValue));
                                            }
                                            else
                                            {
                                                RTFRenderer.AppendLink(rb, strValue, string.Format("XXXX:{0}", strValue));
                                            }

                                            break;
                                        default:
                                            rb.Append(strValue);
                                            break;
                                    }
                                }
                                else if (colIdx == 2)
                                {
                                    // desc
                                    if (!string.IsNullOrEmpty(strDesc))
                                    {
                                        rb.Append(strDesc);
                                    }
                                }
                                else if (colIdx == 3)
                                {
                                    // desc2
                                    if (!string.IsNullOrEmpty(strDesc2))
                                    {
                                        rb.Append(strDesc2);
                                    }
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
    }
}
