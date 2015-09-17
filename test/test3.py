import startup
import shared.util as util
gameDir = util.getGameDirectory()

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan

import FalloutSnip.Domain

sw = Stopwatch.StartNew()

# use an match filter to minimize the load time of skyrim.esm
includeList = ('RACE', 'NPC_', 'LVLN')
filter = System.Func[str,bool]( lambda x: x in includeList )


plugins = FalloutSnip.Domain.Model.PluginList.All
pluginList = util.loadMasterPluginIndex()
from System import Random
rand = Random()
pluginName = pluginList.items()[ rand.Next(0,len(pluginList)-1) ][0]
plugins.AddRecord(FalloutSnip.Domain.Model.Plugin(gameDir + pluginName, filter))

import ExtractNPCs
skyrimRaces = FalloutSnip.Domain.Model.Plugin(gameDir + 'skyrim.esm', filter)
records = [skyrimRaces]
records.extend(plugins.Records)
races = ExtractNPCs.getNPCRaces(records)
for race in races:
	print race

p = util.newPlugin()
from FalloutSnip.Domain.Services import Spells

aRaces = System.Collections.Generic.List[FalloutSnip.Domain.Model.BaseRecord](races).ToArray()
Spells.CopyRecordsTo(aRaces, p, False)

sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'