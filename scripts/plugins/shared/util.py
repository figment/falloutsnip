# NPC Weight manipulator Plugin
#
#   Enumerate through all NPCs and list/manipulate the weight
#      some mods like rbs.esp I think have too many fat people
#
import shared.required
import System
import TESVSnip.Domain
from TESVSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import Action, Func, Predicate, TimeSpan
from System.Diagnostics import Stopwatch
from System.Text.RegularExpressions import Regex
#from TESVSnip.UI.Hosting import ScriptSupport as ss

reWhite = Regex(r"[\n\t\r]") # can probably use re but user might not have full IronPython

def newByteArray(items):
	array = System.Array.CreateInstance(System.Byte, len(items))
	for i,a in enumerate(items):
		array[i] = a
	return array

def newPlugin():
	p = Plugin()
	r = Record()
	r.Name = "TES4"
	sr = SubRecord()
	sr.Name = "HEDR"
	sr.SetData(newByteArray([0xD7, 0xA3, 0x70, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x01]));
	r.AddRecord(sr)
	sr = SubRecord()
	sr.Name = "CNAM";
	sr.SetData(TESVSnip.Framework.Services.Encoding.Instance.GetBytes("Default\0"))
	r.AddRecord(sr)
	p.AddRecord(r)
	#__plugins__.AddRecord(p)
	return p

# read the plugins.txt to a lower case dictionary
def loadMasterPluginIndex():
	""" Loads the Master Plugin Index List into a map
	"""
	import System
	from System.IO import Path
	from System import Environment
	file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),'Skyrim','plugins.txt')
	with open(file, "rt") as f:
		first = f.readline().strip().lower()
		offset = first != 'skyrim.esm' and 1 or 0
		masterIdx= dict( [(r.strip().lower(), i+offset) for i,r in enumerate(f.readlines())] )
		masterIdx[first] = offset
		if offset: masterIdx['skyrim.esm'] = 0
	return masterIdx

	
def buildPluginMasterIndexMap(plugin, masterIdx = None):
	""" Generate an plugin master index to plugins.txt master map
	"""
	if not masterIdx: masterIdx = loadMasterPluginIndex()
	indexMap = {} # could be array like [255]*(len(masters) + 1) but more problems when esp has errors
	masters = plugin.GetMasters()
	for i, master in enumerate(masters):
		indexMap[i] = masterIdx.get(master.lower(),255)
	indexMap[len(masters)] = masterIdx.get(plugin.Name.lower(),255)
	return indexMap

def translateRecordID(rec, pluginIdMap):
	upperbyte = (rec.FormID & 0xFF000000) >> 24
	return (rec.FormID & 0x00FFFFFF) | (pluginIdMap.get(upperbyte, 255) << 24)
	
def alternateLoadMasterPluginIndex():
	import System, System.Windows.Forms
	from System.IO import Path, File, StreamReader
	from System import Environment
	p = []
	r = StreamReader(file)
	while 1:
		t = r.ReadLine()
		if t == None:
			break
		p.append(t)
	r.Close()

	
def getGameDirectory(game='Skyrim'):
	from System.IO import Path
	from Microsoft.Win32 import Registry, RegistryValueOptions
	key = Registry.LocalMachine.OpenSubKey(r"SOFTWARE\Wow6432Node\Bethesda Softworks\%s"%game)
	if not key: key = Registry.LocalMachine.OpenSubKey(r"SOFTWARE\Bethesda Softworks\%s"%game)
	result = key.GetValue("Installed Path", '', RegistryValueOptions.None)
	if key: key.Dispose()
	if not result: return None
	return Path.Combine(result, "Data") + Path.DirectorySeparatorChar

	
def messageBox(owner, title, message):
	from System.Windows.Forms import MessageBox
	return MessageBox.Show(owner,title,message)

def toClipboard(owner, str):
	def callback(value): 
		from System.Windows.Forms import Clipboard
		Clipboard.Clear()
		Clipboard.SetText(value)
	actionCallback = System.Action[System.String](callback)
	ar = owner.BeginInvoke(actionCallback, str)
	#owner.EndInvoke(ar)
	
def firstOrDefault(list, func):
	for item in list:
		if func(item): 
			return item
	return None
	
def getFirstSubrecord(rec, name):
	return firstOrDefault( rec.SubRecords, lambda x: x.Name == name)
	
def getFullName(rec):
	item = getFirstSubrecord(rec, 'FULL')
	if item: return item.GetLString()
	return None

def getTrimFullName(rec):
	fullname = getFullName(rec)
	if fullname: fullname = reWhite.Replace(fullname, " ").Trim()
	if not fullname: fullname = ''
	return fullname
