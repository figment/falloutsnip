#
# List Items Plugin
#
#   Create a console compliant script which lists all items in the plugin
#
#   Output looks like:
#   ; [02] Dawnguard.esm
#   player.additem 02014CCD 1 ; ARMO (DLC1SkinGargoyleAlbino) 
#   player.additem 02014758 1 ; ARMO (DLC1ArmorDawnguardGauntletsHeavy) 
#   player.additem 02014757 1 ; ARMO (DLC1ArmorDawnguardBootsHeavy) 
#
from shared.util import *
import System
import TESVSnip
from TESVSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import Action, Func, Predicate, TimeSpan
from System.Diagnostics import Stopwatch
from System.Drawing import SystemColors, Color
from TESVSnip.UI.Hosting import ScriptSupport as ss
	
def generateItemList(records):
	from System.Text import StringBuilder
	
	reWhite = Regex(r"[\n\t\r]") # can probably use re but user might not have full IronPython
	sb = StringBuilder()
	masterIdx = loadMasterPluginIndex()
	types = set(("ARMO", "WEAP", "MISC", "AMMO", "KEYM"))
	#matchType = Predicate[BaseRecord](lambda rec: (isinstance(rec, Plugin) or isinstance(rec,GroupRecord) or (isinstance(rec,Record) and rec.Name in types)))
	
	for plugin in records:
		lowerName = plugin.Name.lower()
		
		if lowerName == 'rbs.esp': continue # skip this mod since its huge and not very interesting here
		pluginidx = masterIdx.get(lowerName, 255) & 0xFF
		
		first = True
		pluginID = plugin.GetMasters().Length
		for rec in plugin.GetRecordList(types):
			itemMaster = (rec.FormID & 0xFF000000) >> 24
			if itemMaster != pluginID: # not interested in overrides
				continue
			
			if first:
				sb.AppendFormat("\n; [{0:X2}] {1}\n", pluginidx, plugin.DescriptiveName)
				first = False

			fullname = getTrimFullName(rec)
			sb.AppendFormat("player.additem {0:X2}{1:X6} 1 ; {2} \t{3}\n", 
				pluginidx, rec.FormID&0x00FFFFFF, rec.DescriptiveName, fullname )
					
	return sb.ToString()

class ScriptPlugin(TESVSnip.Framework.Services.PluginBase):
	def Execute(self, recs):
		sw = Stopwatch.StartNew()
		str = generateItemList(recs)
		if str:
			sw.Stop()
			t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
			window = ss.CreateTextWindow("Script Output")
			if window: 
				window.Focus()
				window.SetText(str)
			ss.SendStatusText('Script took %s to complete'%t.ToString() , Color.Blue )
	
	def IsValidSelection(self, recs):
		if recs is None or len(recs) == 0: return False
		for rec in recs:
			if not isinstance(rec, Plugin):
				return False
		return True

if __name__ == '<module>':
	TESVSnip.Framework.Services.PluginStore.AddPlugins(
		[ ScriptPlugin("listitems", "List &Item Script", supportSelection=True, supportGlobal=True) 
		]
	)