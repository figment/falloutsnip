#
# Copy to Example
#
#   Copy Selected Group or Record to another plugin or new plugin
#
from shared.util import *

import System
import TESVSnip.Domain
from TESVSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import TimeSpan
from System.Diagnostics import Stopwatch

def copyToRecord(records, override):
	import TESVSnip.Domain.Spells
	from shared.SelectItem import SelectItem
	GetPluginFromNode = TESVSnip.Domain.Spells.GetPluginFromNode
	
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
			__plugins__.AddRecord(p)
		else:
			p = __plugins__[form.GetSelectedItem()]
		if p:
			TESVSnip.Domain.Spells.CopyRecordsTo(records, p, override)
			pass
			

if __name__ == '<module>':
	import TESVSnip
	from TESVSnip.UI.Hosting import ScriptSupport
	
	class ScriptPlugin(TESVSnip.UI.Services.PluginBase):
		from System.Drawing import Color
		def Execute(self, recs):
			sw = Stopwatch.StartNew()
			if self.Name == 'copyto.over':
				copyToRecord(recs, True)
			elif self.Name == 'copyto.new':
				copyToRecord(recs, False)
			sw.Stop()
			t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
			ScriptSupport.SendStatusText('Script took %s to complete. List added to clipboard'%t.ToString() , Color.Blue )
			
		def IsValidSelection(self, recs):
			if recs is None or len(recs) == 0: return False
			for rec in recs:
				if not isinstance(rec, GroupRecord) and not isinstance(rec, Record):
					return False
			return True

	TESVSnip.UI.Services.PluginStore.AddPlugins(
		[ ScriptPlugin("copyto.over", "&Copy Override To", supportSelection=True, supportGlobal=False)
		, ScriptPlugin("copyto.new", "&Copy New To", supportSelection=True, supportGlobal=False)
		]
	)