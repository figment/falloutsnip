import startup
import shared.util as util
gameDir = util.getGameDirectory()

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan

import TESVSnip.Domain

sw = Stopwatch.StartNew()

# use an match filter to minimize the load time of skyrim.esm
includeList = ('RACE', 'NPC_', 'LVLN')
filter = System.Func[str,bool]( lambda x: x in includeList )


plugins = TESVSnip.Domain.Model.PluginList.All
pluginList = util.loadMasterPluginIndex()
from System import Random
rand = Random()
pluginName = pluginList.items()[ rand.Next(0,len(pluginList)-1) ][0]
plugins.AddRecord(TESVSnip.Domain.Model.Plugin(gameDir + pluginName, filter))

import ExtractNPCs
skyrimRaces = TESVSnip.Domain.Model.Plugin(gameDir + 'skyrim.esm', filter)
records = [skyrimRaces]
records.extend(plugins.Records)
races = ExtractNPCs.getNPCRaces(records)
for race in races:
	print race

p = util.newPlugin()
from TESVSnip.Domain.Services import Spells

aRaces = System.Collections.Generic.List[TESVSnip.Domain.Model.BaseRecord](races).ToArray()
Spells.CopyRecordsTo(aRaces, p, False)

sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'