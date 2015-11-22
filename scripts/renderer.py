import markup
from System import Single, SByte, Byte, Int16, UInt16, Int32, UInt32, Double
from System import ArraySegment, Convert, UriBuilder
from System.Text import StringBuilder
#from System.Web import HttpUtility
import FalloutSnip.Domain
from FalloutSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from FalloutSnip.Domain.Data.Structure import RecordStructure, ElementValueType
from FalloutSnip.UI.Scripts import PyInterpreter, FunctionOperation
from FalloutSnip.Domain.Services import Spells
from FalloutSnip.Framework import TypeConverter
from FalloutSnip.Framework.Services import Encoding


def ifnull(value, default):
    return value and value or default


def firstOrDefault(list, func):
    for item in list:
        if func(item):
            return item
    return None


def getFirstSubrecord(rec, name):
    return firstOrDefault(rec.SubRecords, lambda x: x.Name == name)


def getFullName(rec):
    item = getFirstSubrecord(rec, 'FULL')
    if not item: return None
    return item.GetLString()


def getFullNameWithID(rec):
    item = getFirstSubrecord(rec, 'FULL')
    if not item: return (None, '')
    plugin = rec.GetPlugin()
    data = ArraySegment[Byte](item.GetReadonlyData())
    if TypeConverter.IsLikelyString(data):
        return (None, item.GetStrData())
    else:
        id = TypeConverter.h2i(data)
        return (id, plugin.LookupFormStrings(id))


def getEditorID(rec):
    item = getFirstSubrecord(rec, 'EDID')
    if not item: return None
    return item.GetStrData()


def createLink(plugin, formIDType, formID, master=None):
    fmt = "nav:%s?t=%s&v=%s"
    fmt2 = "&m=%s"
    if not plugin: plugin = '.'
    if not formIDType: formIDType = 'XXXX'
    result = fmt % (plugin, formIDType, formID)
    if master: result = result + fmt2 % (master)
    return result


class HTMLRenderer():
    grouptypes = {0: "Top", 1: "World children", 2: "Interior Cell Block", 3: "Interior Cell Sub-Block"
    , 4: "Exterior Cell Block", 5: "Exterior Cell Sub-Block", 6: "Cell Children"
    , 7: "Topic Children", 8: "Cell Persistent Children", 9: "Cell Temporary Children"
    , 10: "Cell Visible Distant Children"}

    def __init__(self, *args, **kwargs):
        self.page = markup.page()
        self.page.init(**kwargs)

    def __str__(self):
        return str(self.page)

    def GetHeader(self, rec):
        try:
            if isinstance(rec, Record):
                return self.GetHeaderRecord(rec)
            elif isinstance(rec, SubRecord):
                return self.GetHeaderSubRecord(rec)
            elif isinstance(rec, Plugin):
                return self.GetHeaderPlugin(rec)
            elif isinstance(rec, GroupRecord):
                return self.GetHeaderGroupRecord(rec)
            return self.GetHeaderBasic(rec)
        except Exception as e:
            print(e)
            import sys, traceback; traceback.print_exc(file=sys.stdout)
            pass

    def GetHeaderBasic(self, rec):
        p = self.page
        with p.table(id='record-hdr'):
            with p.thead():
                with p.tr():
                    p.td('[%s] Header' % (rec.GetType().Name), colspan='2', class_='header')

            with p.tbody():
                compressRow = None
                flagstr = ''
                rows = [('Type:', rec.Name, 'type')
                    , ('Size:', rec.Size.ToString('N0'), 'float' )
                        #, ('Size2:', rec.Size.ToString('N0') + ' (includes headers)', 'float' )
                    , ('Records:', rec.Records.Count.ToString(), 'count' )
                ]
                for row in rows:
                    if not row: continue
                    with p.tr():
                        p.td(row[0], width="33%", class_='label')
                        p.td(row[1], class_=ifnull(row[2], 'value'))
        pass

    def GetHeaderRecord(self, rec):
        p = self.page
        with p.table(id='record-hdr'):
            with p.thead():
                with p.tr():
                    p.td('[%s] Header' % (rec.GetType().Name), colspan='2', class_='header')
            with p.tbody():
                compressRow = None
                flagstr = ''
                if rec.Flags1:
                    flagstr = FalloutSnip.Domain.Model.FlagDefs.GetRecFlags1Desc(rec.Flags1)
                    if flagstr: flagstr = ' : ' + flagstr
                    if flagstr.find('Compressed') > 0:
                        compressRow = ( 'Compression Level:', rec.CompressionLevel.ToString(), 'flags')

                type = rec.Name
                structure = rec.GetStructure()
                if structure: type = "%s: %s" % (rec.Name, structure.description)

                rows = [('Type:', type, 'type')
                    , ('Description:', rec.DescriptiveName, 'text' )
                    , ('FormID:', rec.FormID.ToString('X8'), 'formid' )
                    , ('Flags 1:', rec.Flags1.ToString('X8') + flagstr, 'flags' ), compressRow
                    , ('Version Control Info:', rec.Flags2.ToString('X8'), 'flags' )
                    , ('Flags 2:', (rec.Flags3 >> 16).ToString('X4'), 'flags' )
                    , ('Form Version:', "%04x : %d" % (((rec.Flags3 << 16) >> 16), ((rec.Flags3 << 16) >> 16)), 'flags' )
                    , ('Size:', rec.Size.ToString('N0'), 'float' )
                    , ('Subrecords:', rec.SubRecords.Count.ToString(), 'count' )
                ]
                for row in rows:
                    if not row: continue
                    with p.tr():
                        p.td(row[0], width="33%", class_='label')
                        p.td(row[1], class_=ifnull(row[2], 'value'))

    def GetHeaderPlugin(self, rec):
        p = self.page
        with p.table(id='record-hdr'):
            with p.thead():
                with p.tr():
                    p.td('[%s] Header' % (rec.GetType().Name), colspan='2', class_='header')
            with p.tbody():
                masters = rec.GetMasters()
                rows = [('File Name:', rec.Name, 'type')
                    , ('File Size:', rec.Size.ToString('N0') + ' (uncompressed)', 'float' )
                    , ('Records:', rec.Records.Count.ToString(), 'count' )
                ]
                if rec.Filtered:
                    rows.append(('Filtered:', rec.Filtered.ToString(), 'flags' ))
                if masters:
                    rows.append(('Master Count:', len(masters), 'count'))
                for row in rows:
                    if not row: continue
                    with p.tr():
                        p.td(row[0], width="33%", class_='label')
                        p.td(row[1], class_=ifnull(row[2], 'value'))
                if masters:
                    with p.tr():
                        p.td('Masters:', width="33%", class_='label')
                        with p.td(class_=ifnull(row[2], 'value')):
                            for master in masters:
                                with p.p(): p.a(master, href='file://./' + master)

    def GetHeaderSubrecord(self, rec):
        pass

    def GetDescription(self, rec):
        try:
            if isinstance(rec, Record):
                return self.GetDescriptionRecord(rec)
            elif isinstance(rec, SubRecord):
                return self.GetDescriptionSubRecord(rec)
            #elif isinstance(rec, Plugin):
            #	return self.GetDescriptionPlugin(rec)
            elif isinstance(rec, GroupRecord):
                return self.GetDescriptionGroupRecord(rec)
            return self.GetHeaderBasic(rec)
        except Exception as e:
            pass

    def GetElementName(self, elem):
        sselem = elem.Structure
        indices = elem.Indices
        if indices:
            ssname = ''
            for index in indices:
                if ssname: ssname += '.'
                ssname += index.Item1.name
                if index.Item1.repeat > 0:
                    ssname += '[%s]'%(str(index.Item2))
            if not sselem.repeat:
                if ssname: ssname += '.'
                ssname += sselem.name
            return ssname
        else:
            return sselem.name

    def GetElementValue(self, elem):
        # presumably we can just straight up execute this but for now this is compatible
        if elem.Structure.funcr:
            if (elem.Type == ElementValueType.Float):
                return PyInterpreter.ExecuteFunction[Single](elem, FunctionOperation.ForReading)
            elif (elem.Type == ElementValueType.Int):
                return PyInterpreter.ExecuteFunction[Int32](elem, FunctionOperation.ForReading)
            elif (elem.Type == ElementValueType.Short):
                return PyInterpreter.ExecuteFunction[Int16](elem, FunctionOperation.ForReading)
            elif (elem.Type == ElementValueType.UShort):
                return PyInterpreter.ExecuteFunction[UInt16](elem, FunctionOperation.ForReading)
            elif (elem.Type == ElementValueType.UInt):
                return PyInterpreter.ExecuteFunction[UInt32](elem, FunctionOperation.ForReading)
        if elem.Value == None: return ''
        return elem.Value

    def GetDescriptionRecord(self, rec):
        p = self.page
        try:
            structure = rec.GetStructure()
            if structure:
                rec.MatchRecordStructureToRecord()
                #print structure
                p.h2(structure.description)
                for subrec in rec.SubRecords:
                    print(subrec)
                    if (((subrec.Structure != None) and (subrec.Structure.elements != None))
                        and not subrec.Structure.notininfo):
                        self.GetDescriptionSubRecord(subrec)
        except Exception as e:
            p.p('Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml',
                class_='danger')
            p.p(str(e))
            print(e)
            import sys, traceback; traceback.print_exc(file=sys.stdout)

    def GetDescriptionSubRecord(self, rec):
        p = self.page

        # table has up to 5 columns
        ss = structure = rec.Structure
        if not structure or not structure.elementTree:
            with p.table(id='record-desc'):
                if ss:
                    with p.thead():
                        with p.tr():
                            p.td(ss.name, class_='headerlabel', width="33%")
                            p.td(ss.desc, colspan='4', class_='header')
                #with p.tfoot(): # write a blank footer to fix the HtmlRenderer Control
                #	with p.tr(class_='hidden'):
                #		p.td('',class_='header',width="33%").td('').td('').td('').td('')
                with p.tr():
                    p.td("String:", width="33%", class_='label')
                    p.td(rec.GetStrData(), class_='value', colspan='4')
                with p.tr():
                    p.td("Hex:", width="33%", class_='label')
                    p.td(rec.GetHexData(), class_='value', colspan='4')
            return

        try:
            plugin = rec.GetPlugin()
            pluginFile = plugin.Name
            elems = [elem for elem in rec.EnumerateElements(True)
                     if elem.Structure != None and not elem.Structure.notininfo]
            if not elems:
                return

            with p.table(id='record-desc'):
                with p.thead():
                    with p.tr():
                        p.td(ss.name, class_='headerlabel')
                        p.td(ss.desc, colspan='4', class_='header')
                #with p.tfoot(): # write a blank footer to fix the HtmlRenderer Control
                #	with p.tr(class_='hidden'):
                #		p.td('',class_='header',width="33%").td('').td('').td('').td('')
                with p.tbody():
                    for elem in elems:
                        sselem = elem.Structure
                        ssname = self.GetElementName(elem)
                        value = self.GetElementValue(elem)
                        strValue = str(value)

                        with p.tr():
                            p.td(ssname, width="33%", class_='label')

                            if sselem.type == ElementValueType.Blob:
                                p.td(TypeConverter.GetHexData(elem.Data), class_='value', colspan='4')
                            elif sselem.type == ElementValueType.Str4:
                                p.td(TypeConverter.GetString(elem.Data), class_='text', colspan='4')
                            elif sselem.type == ElementValueType.BString:
                                p.td(TypeConverter.GetBString(elem.Data), class_='text', colspan='4')
                            elif sselem.type == ElementValueType.IString:
                                p.td(TypeConverter.GetIString(elem.Data), class_='text', colspan='4')
                            elif sselem.type == ElementValueType.FormID:
                                if not value:
                                    p.td(strValue, class_='value', colspan='4')
                                else:
                                    formid = value.ToString("X8")
                                    record = plugin.GetRecordByID(value)
                                    if not record: # lookup plugin name using the id
                                        prefName = plugin.GetRecordMaster(value)
                                        with p.td(class_='formid', colspan='4'):
                                            p.a(formid, href=createLink(pluginFile, sselem.FormIDType, formid, prefName))
                                    else: # lookup actual record to know actual type
                                        pref = record.GetPlugin()
                                        with p.td(class_='formid', width="15%"):
                                            p.a(formid, href=createLink(pluginFile, record.Name, record.FormID.ToString("X8"), pref.Name))

                                        if record.Name != sselem.FormIDType:
                                            p.td(record.DescriptiveName, class_='text', width='20%')
                                        else:
                                            p.td(getEditorID(record), class_='text', width='20%')

                                        id, fullStr = getFullNameWithID(record)
                                        if id == None:
                                            p.td(fullStr, class_='text', colspan=2)
                                        else:
                                            p.td(str(id), class_='textid', width="15%")
                                            p.td(fullStr, class_='text')

                            elif sselem.type == ElementValueType.LString:
                                if elem.Type == ElementValueType.String:
                                    p.td(value, class_='text', colspan=4)
                                elif TypeConverter.IsLikelyString(elem.Data):
                                    p.td(TypeConverter.GetString(elem.Data), class_='text', colspan=4)
                                else:
                                    id = TypeConverter.h2i(elem.Data)
                                    p.td(id.ToString("X8"), class_='text')
                                    p.td(plugin.LookupFormStrings(id), class_='text', colspan=3)

                            elif sselem.type in (ElementValueType.SByte, ElementValueType.Int
                                                 , ElementValueType.Short, ElementValueType.Byte
                                                 , ElementValueType.UInt, ElementValueType.UShort):

                                if sselem.type in (ElementValueType.Byte, ElementValueType.UInt, ElementValueType.UShort):
                                    intVal = Convert.ToUInt32(value)
                                else:
                                    intVal = Convert.ToInt32(value)

                                hasOptions = sselem.options != None and sselem.options.Length > 0;
                                hasFlags = sselem.flags != None and sselem.flags.Length > 1;

                                if sselem.hexview or hasFlags:
                                    hexstr = value.ToString("X" + str(elem.Data.Count * 2))
                                    if sselem.hexviewwithdec:
                                        p.td(hexstr, class_='text', width="15%")
                                        p.td(strValue, class_='text', width="15%")
                                    else:
                                        p.td(hexstr, class_='text', colspan=3, width="30%")
                                else:
                                    p.td(strValue, class_='text', colspan=3, width="30%")

                                strDesc = ''
                                if hasOptions:
                                    for k in xrange(0, sselem.options.Length, 2):
                                        ok, intValOption = int.TryParse(sselem.options[k + 1])
                                        if ok and intVal == intValOption:
                                            strDesc = sselem.options[k]
                                elif hasFlags:
                                    sb = StringBuilder()
                                    for k in xrange(0, sselem.flags.Length, 1):
                                        if ((intVal & (1 << k)) != 0):
                                            if (sb.Length > 0):
                                                sb.Append("<br/>")
                                            sb.Append(sselem.flags[k])
                                    strDesc = sb.ToString()
                                p.td(strDesc, class_='desc', colspan=3, width='50%')
                                pass

                            else:
                                #p.td(str(sselem.type), class_='text',width='auto' )
                                p.td(strValue, class_='text', colspan=4)
        except Exception as e:
            p.p("Warning: Subrecord doesn't seem to match the expected structure", class_='danger')
            p.p(str(e), class_='danger')
            #print(e)
            #import sys,traceback
            #traceback.print_exc(file=sys.stdout)

    def GetHeaderGroupRecord(self, rec):
        p = self.page
        with p.table(id='record-hdr'):
            with p.thead():
                with p.tr():
                    p.td('[%s] Header' % (rec.GetType().Name), colspan='2', class_='header')

            with p.tbody():
                groupDesc = None
                data = rec.GetReadonlyData()
                if rec.groupType == 0:
                    type = Encoding.Instance.GetString(data, 0, 4)
                    groupDesc = ('Contains:', type, 'type')
                elif rec.groupType in (2, 3):
                    value = (data[0] + data[1] * 0x100 + data[2] * 0x10000 + data[3] * 0x1000000)
                    groupDesc = ('Block number:', value, 'type')
                elif rec.groupType in (4, 5):
                    x = (data[0] + data[1] * 0x100)
                    y = (data[2] + data[3] * 0x100)
                    groupDesc = ('Coordinates:', '[%2d,%2d]' % (x, y), 'type')
                elif rec.groupType in (1, 6, 7, 8, 9, 10):
                    value = (data[0] + data[1] * 0x100 + data[2] * 0x10000 + data[3] * 0x1000000)
                    groupDesc = ('Parent FormID:', value.ToString('X8'), 'formid')

                rows = [('Group Type:', '%d: %s' % (rec.groupType, HTMLRenderer.grouptypes.get(rec.GroupType, 'Unknown')), 'type')
                    , groupDesc #('Type:', rec.ContentsType, 'type')
                    , ('Records:', rec.Records.Count.ToString(), 'count' )
                    , ('Size:', rec.Size.ToString('N0') + ' bytes (including header)', 'float' )
                ]
                for row in rows:
                    if not row: continue
                    with p.tr():
                        p.td(row[0], width="33%", class_='label')
                        p.td(row[1], class_=ifnull(row[2], 'value'))

    def GetDescriptionGroupRecord(self, rec):
        # Only enumerate through the children if groupType = 0
        #if rec.groupType != 0:
        #	return None

        p = self.page
        try:
            pref = rec.GetPlugin()
            prefFile = pref and pref.Name or ''
            type = desc = ''
            data = rec.GetReadonlyData()
            if rec.groupType == 0:
                type = Encoding.Instance.GetString(data, 0, 4)
                structure = rec.GetStructure()
                if not structure: return None
                desc = structure.description
            with p.table(id='record-desc'):
                with p.thead():
                    with p.tr():
                        p.td(type, class_='headerlabel', width="20%")
                        p.td(desc, colspan=4, class_='header')
                #with p.tfoot(): # write a blank footer to fix the HtmlRenderer Control
                #	with p.tr(class_='hidden'):
                #		p.td('',class_='header',width="33%").td('').td('').td('').td('')
                for record in rec.Records:
                    if isinstance(record, Record):
                        formid = record.FormID
                        strValue = formid.ToString('X8')
                        with p.tr():
                            if not formid:
                                p.td(strValue, class_='value', colspan=4)
                            else:
                                with p.td(class_='formid', width="20%"):
                                    p.a(strValue, href=createLink(prefFile, record.Name, record.FormID.ToString("X8"), prefFile))
                                if record.Name != type:
                                    p.td(record.DescriptiveName, class_='text', width='20%')
                                else:
                                    p.td(getEditorID(record), class_='text', width='20%')
                                id, fullStr = getFullNameWithID(record)
                                if id == None:
                                    p.td(fullStr, class_='text', colspan=2)
                                else:
                                    p.td(id, class_='textid', width="8%")
                                    p.td(fullStr, class_='text')
                    elif isinstance(record, GroupRecord):
                        with p.tr():
                            p.td(record.DescriptiveName, class_='value', colspan=1, width="20%")
                            if rec.groupType == 0:
                                p.td(Encoding.Instance.GetString(data, 0, 4), class_='value', colspan=1)
                            elif rec.groupType in (2, 3):
                                value = (data[0] + data[1] * 0x100 + data[2] * 0x10000 + data[3] * 0x1000000)
                                p.td('Block: ' + str(value), class_='value', colspan=1)
                            elif rec.groupType in (4, 5):
                                x = (data[0] + data[1] * 0x100)
                                y = (data[2] + data[3] * 0x100)
                                p.td('Coord: [%2d,%2d]' % (x, y) + str(value), class_='value', colspan=1)
                            elif rec.groupType in (1, 6, 7, 8, 9, 10):
                                with p.td():
                                    value = (data[0] + data[1] * 0x100 + data[2] * 0x10000 + data[3] * 0x1000000)
                                    p.add('Parent ID:')
                                    strValue = value.ToString("X8")
                                    if not value:
                                        p.add(strValue)
                                    else:
                                        p.a(strValue, href=createLink(prefFile, 'XXXX', strValue, prefFile))
                            else:
                                p.td('', class_='value', colspan=1)
                            p.td("%d: %s" % (rec.groupType, HTMLRenderer.grouptypes.get(rec.GroupType, 'Unknown')), class_='value', colspan=2)



        except Exception as e:
            p.p('Warning: An error occurred while processing the record. It may not conform to the structure defined in RecordStructure.xml',
                class_='danger')
            p.p(str(e))
        #print(e)
        #import sys,traceback
        #traceback.print_exc(file=sys.stdout)


if __name__ == '<module>':
    import FalloutSnip.Domain

    class Renderer(FalloutSnip.Domain.Rendering.IRenderer):

        def Render(self, rec, kwargs):
            try:
                csslist = None
                ok, titleValue = kwargs.TryGetValue('title')
                if not ok: titleValue = ''
                ok, cssarray = kwargs.TryGetValue('css')
                if ok and len(cssarray) > 0:
                    csslist = ( x for x in cssarray )
                html = HTMLRenderer(title=titleValue, css=cssarray)
                html.GetHeader(rec)
                html.page.hr()
                html.GetDescription(rec)
            except Exception as e:
                #html.p("Warning: Unexpected Error occurred while processing", class_='danger')
                #html.p(str(e), class_='danger')
                return "Unexpected Error occurred while processing record\n" + str(e)
            return str(html)