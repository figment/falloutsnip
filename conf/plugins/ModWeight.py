#
# NPC Weight manipulator Plugin
#
#   Enumerate through all NPCs and list/manipulate the weight
#      some mods like rbs.esp I think have too many fat people
#
from shared.util import *

import System
import TESVSnip
from TESVSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from System import Action, Func, Predicate, TimeSpan
from System.Diagnostics import Stopwatch
from System.Drawing import SystemColors, Color
from TESVSnip.UI.Hosting import ScriptSupport as ss
from System import Random
rand = Random()

def norminv(q,sigma=1.0,mu=0.0,min=None,max=None):
	from System.Math import *
	if q == 0.5: return 0
	q = 1.0 - q
	p = (q > 0.0 and q < 0.5) and q or (1.0 - q)
	t = Sqrt(Log(1.0 / Pow(p, 2.0)))
	c0,c1,c2 = 2.515517, 0.802853, 0.010328
	d1,d2,d3 = 1.432788, 0.189269, 0.001308
	x = t - (c0 + c1 * t + c2 * Pow(t, 2.0)) / (1.0 + d1 * t + d2 * Pow(t, 2.0) + d3 * Pow(t, 3.0))
	if q > 0.5: x = x * -1.0
	z = ( x * sigma ) + mu
	if min != None: z = Max(z,min)
	if max != None: z = Min(z,max)
	return z

def rnorminv(sigma=1.0,mu=0.0,min=None,max=None):
	
	return norminv( rand.NextDouble(), sigma, mu, min, max)

def getWeight(rec):
	item = getFirstSubrecord(rec, 'NAM7') # weight
	if not item: return None
	return item.GetValue[System.Single](0) # single precision float

def setWeight(rec, value):
	item = getFirstSubrecord(rec, 'NAM7') # weight
	if not item: return None
	return item.TrySetValue[System.Single](0, value) # single precision float
	
def getScale(rec):
	item = getFirstSubrecord(rec, 'NAM6') # height/scale
	if not item: return None
	return item.GetValue[System.Single](0) # single precision float

def listNPCWeights(records):
	from System import Predicate
	from System.Text import StringBuilder
	from System.Text.RegularExpressions import Regex
	sb = StringBuilder()
	masterIdx = loadMasterPluginIndex() # build dictionary of masters
	#types = set(('NPC_',))
	#matchType = Predicate[BaseRecord](lambda rec: (isinstance(rec, Plugin) 
	#  or isinstance(rec,GroupRecord) or (isinstance(rec,Record) and rec.Name in types)))
	
	for plugin in records:
		lowerName = plugin.Name.lower()
		# Build master map with invalid masters mapping to 0xFF
		pluginIdMap = buildPluginMasterIndexMap(plugin, masterIdx) 
		pluginidx = masterIdx.get(lowerName, 255) & 0xFF
		
		first = True
		pluginID = plugin.GetMasters().Length
		for rec in plugin.GetRecordList('NPC_'):
			itemMaster = (rec.FormID & 0xFF000000) >> 24
			formid = translateRecordID(rec, pluginIdMap)
			
			if first:
				sb.AppendFormat("\n; [{0:X2}] {1}\n", pluginidx, plugin.DescriptiveName)
				first = False

			try:
				fullname = getTrimFullName(rec)
				weight = getWeight(rec)
				scale = getScale(rec)
								
				sb.AppendFormat("{0:X8}.SetNPCWeight {3} ; {1} \t{2}\n", 
					formid, rec.DescriptiveName, fullname, weight
					)
				# if scale <> 1.0 and scale <> None:
					# sb.AppendFormat("{0:X8}.SetScale {3} ; {1} \t{2}\n", 
						# formid, rec.DescriptiveName, fullname, weight
						# )
			except Exception, e:
				print str(e)
				pass
	return sb.ToString()

def modifyNPCWeights(records):
	""" Modify NPC Weight to lower with distribution around 35%
	"""
	from System import Predicate
	from System.Text import StringBuilder
	from System.Text.RegularExpressions import Regex
	sb = StringBuilder()
	#types = set(('NPC_',))
	masterIdx = loadMasterPluginIndex() # build dictionary of masters
	#matchType = Predicate[BaseRecord](lambda rec: (isinstance(rec, Plugin) or isinstance(rec,GroupRecord) or (isinstance(rec,Record) and rec.Name in types)))
	
	for plugin in records:
		lowerName = plugin.Name.lower()
		pluginIdMap = buildPluginMasterIndexMap(plugin, masterIdx) # build dictionary of masters.  invalid map to 0xFF
		pluginidx = masterIdx.get(lowerName, 255) & 0xFF
		
		first = True
		pluginID = plugin.GetMasters().Length
		for rec in plugin.GetRecordList('NPC_'):
			itemMaster = (rec.FormID & 0xFF000000) >> 24
			formid = translateRecordID(rec, pluginIdMap)
			
			if first:
				sb.AppendFormat("\n; [{0:X2}] {1}\n", pluginidx, plugin.DescriptiveName)
				first = False

			try:
				fullname = getTrimFullName(rec)
				weight = getWeight(rec)
				print weight
				if not weight or weight >= 10: # leave alone
					newweight = rnorminv(35,20,0,100) # generate random value between 0 and 100 centered around 35 with sigma of 20 
					if not newweight: newweight = rnorminv(35,20,0,100) # try again
					setWeight(rec, newweight)
								
					sb.AppendFormat("{0:X8}.SetNPCWeight {4} ; {3} \t{1} \t{2} \n", 
						formid, rec.DescriptiveName, fullname, weight, newweight
						)
			except Exception, e:
				print str(e)
				pass
	return sb.ToString()

	
class ScriptPlugin(TESVSnip.Framework.Services.PluginBase):
	def Execute(self, recs):
		sw = Stopwatch.StartNew()
		str = None
		if self.Name == 'listweight':
			str = listNPCWeights(recs)
		if self.Name == 'modweight':
			str = modifyNPCWeights(recs)
		
		if str:
			sw.Stop()
			t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
			toClipboard(__window__, str)
			window = ss.CreateTextWindow("Script Output")
			if window: 
				window.Focus()
				window.SetText(str)
			ss.SendStatusText('Script took %s to complete. List added to clipboard'%t.ToString() , Color.Blue )
		
	def IsValidSelection(self, recs):
		if recs is None or len(recs) == 0: return False
		for rec in recs:
			if not isinstance(rec, Plugin):
				return False
		return True
		
if __name__ == '<module>':
	TESVSnip.Framework.Services.PluginStore.AddPlugins(
		[ ScriptPlugin("listweight", "List &NPC Weights", supportSelection=True, supportGlobal=True) 
		, ScriptPlugin("modweight", "&Modify NPC Weights", supportSelection=True, supportGlobal=False)  # only support for explicit selected plugin
		]
	)