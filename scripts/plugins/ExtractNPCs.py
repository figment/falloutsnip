#
# Extract NPC Example
#
#   Extract NPCs from Plugins
#
from shared.util import *

import System
import FalloutSnip.Domain
from FalloutSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import TimeSpan, UInt32
from System.Diagnostics import Stopwatch

creatureKYWDs = (0x0001397A, 0x00013798, 0x00013797, 0x00013795) #0x00013796 (undead)

def getNPCRaces(plugins):
	""" Get List of NPC Races.  Basically exclude the creature keywords
	"""
	from FalloutSnip.Domain.Data.Structure import ElementValueType
	result = []
	for plugin in plugins:
		races = plugin.GetRecordList('RACE')
		for race in races:
			race.MatchRecordStructureToRecord()
			for keyword in race.GetSubRecords('KWDA'):
				isNPC = isCreature = False
				for elem in keyword.EnumerateElements():
					if elem.Type == ElementValueType.FormID:
						if elem.GetValue[UInt32]() == 0x00013794: # ActorTypeNPC
							isNPC = True
						elif elem.GetValue[UInt32]() in creatureKYWDs:
							isCreature = True
				if isNPC and not isCreature:
					result.append(race)
	return result

def getNonCreatureNPCs(plugins):
	""" Get List of NPC Races.  Basically exclude the creature keywords
	"""
	from FalloutSnip.Domain.Data.Structure import ElementValueType
	result = []
	for plugin in plugins:
		races = plugin.GetRecordList('NPC_')
		'TPLT' # Exclude 'Is CharGen Face Preset' in ACBS
		for race in races:
			race.MatchRecordStructureToRecord()
			for keyword in race.GetSubRecords('KWDA'):
				isNPC = isCreature = False
				for elem in keyword.EnumerateElements():
					if elem.Type == ElementValueType.FormID:
						if elem.GetValue[UInt32]() == 0x00013794: # ActorTypeNPC
							isNPC = True
						elif elem.GetValue[UInt32]() in creatureKYWDs:
							isCreature = True
				if isNPC and not isCreature:
					result.append(race)
	return result
	
def copyRecords(records):
	import FalloutSnip.Domain
	from shared.SelectItem import SelectItem
	GetPluginFromNode = FalloutSnip.Domain.Spells.GetPluginFromNode
	skyrim = __plugins__['skyrim.esm']
	if not skyrim:
		print 'ERROR: Skyrim.esm not found'
		return
	parents = set( [ GetPluginFromNode(r) for r in records ] )
	
	p = newPlugin()
	#reclist
	for rec in records:
		FalloutSnip.Domain.Spells.CopyRecordsTo(plugin.GetRecordList('NPC_'), p, override = True)
		pass
	return p
	
if __name__ == '<module>':
	import FalloutSnip
	
	class ScriptPlugin(FalloutSnip.UI.Services.PluginBase):
		def Execute(self, recs):
			from FalloutSnip.UI.Hosting import ScriptSupport
			from System.Drawing import SystemColors, Color
			from FalloutSnip.UI.Hosting import ScriptSupport
		
			sw = Stopwatch.StartNew()
			print getNPCRaces(recs)
			#p = copyRecords(recs)
			#__plugins__.AddRecord(p)  # add new plugin
			sw.Stop()
			t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
			ScriptSupport.SendStatusText('Script took %s to complete' % t.ToString() , Color.Blue)
			
		def IsValidSelection(self, recs):
			if recs is None or len(recs) == 0: return False
			for rec in recs:
				if (not isinstance(rec, Plugin) and not (isinstance(rec, Record) and rec.Name == 'NPC_') and not (isinstance(rec, GroupRecord) and rec.ContentsType == 'NPC_')):
					return False
			return True
		
	FalloutSnip.UI.Services.PluginStore.AddPlugins([ScriptPlugin("extract.npc", "&Extract NPCS", supportSelection=True, supportGlobal=True)])
