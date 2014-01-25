#
# Copy to Example
#
#   Copy Selected Group or Record to another plugin or new plugin
#
from shared.util import *

import System
import TESVSnip
from TESVSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import TimeSpan
from System.Diagnostics import Stopwatch
from System.Drawing import Color

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
	__plugins__.AddRecord(p)
	return p

def copyToRecord(records, override):
	import TESVSnip.UI.Spells
	from shared.SelectItem import SelectItem
	GetPluginFromNode = TESVSnip.UI.Spells.GetPluginFromNode
	
	parents = set( [ GetPluginFromNode(r).Name for r in records ] )
	form = SelectItem()
	form.SetLabel('Select Plugin to Copy Records into:')
	items = [ x.Name for x in __plugins__ if x.Name not in parents ]
	items.append('<New>')
	form.SetItems(items)
	result = form.ShowDialog(__window__)
	if str(result) == 'OK': 
		if '<New>' == form.GetSelectedItem():
			p = newPlugin()
		else:
			p = __plugins__[form.GetSelectedItem()]
		if p:
			TESVSnip.UI.Spells.CopyRecordsTo(records, p, override)
			pass
			
	
class ScriptPlugin(TESVSnip.Framework.Services.PluginBase):
	def Execute(self, recs):
		sw = Stopwatch.StartNew()
		if self.Name == 'copyto.over':
			copyToRecord(recs, True)
		elif self.Name == 'copyto.new':
			copyToRecord(recs, False)
		sw.Stop()
		t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
		ss.SendStatusText('Script took %s to complete. List added to clipboard'%t.ToString() , Color.Blue )
		
	def IsValidSelection(self, recs):
		if recs is None or len(recs) == 0: return False
		for rec in recs:
			if not isinstance(rec, GroupRecord) and not isinstance(rec, Record):
				return False
		return True
		
if __name__ == '<module>':
	TESVSnip.Framework.Services.PluginStore.AddPlugins(
		[ ScriptPlugin("copyto.over", "&Copy Override To", supportSelection=True, supportGlobal=False)
		, ScriptPlugin("copyto.new", "&Copy New To", supportSelection=True, supportGlobal=False)
		]
	)