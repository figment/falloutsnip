import startup
import shared.util as util
gameDir = util.getGameDirectory()

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan

sw = Stopwatch.StartNew()

import FalloutSnip.Domain
import ListItems
plugins = FalloutSnip.Domain.Model.PluginList.All

excludeList = 'RGDL;CLDC;PWAT;SCOL;SCPT;HAIR;REGN;NAVI;WRLD;DIAL;CELL;IMAD;WTHR'.Split(';')
filter = System.Func[str,bool]( lambda x: x not in excludeList )

pluginList = util.loadMasterPluginIndex()
from System import Random
rand = Random()
pluginName = pluginList.items()[ rand.Next(0,len(pluginList)-1) ][0]
plugins.AddRecord(FalloutSnip.Domain.Model.Plugin(gameDir + pluginName, filter))
#plugins.AddRecord(FalloutSnip.Domain.Model.Plugin(gameDir + 'skyrim.esm', filter))
print ListItems.generateItemList(plugins)

import ModWeight
print ModWeight.listNPCWeights(plugins)
print ModWeight.modifyNPCWeights(plugins)

sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'