import sys
import startup
import shared.util as util
gameDir = util.getGameDirectory('Oblivion')

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan
from System.IO import Path

import FalloutSnip.Domain
from FalloutSnip.Domain.Services import Spells
from FalloutSnip.Domain.Model import BaseRecord, Record, Plugin, SubRecord, GroupRecord
from FalloutSnip.Domain.Data.Structure import RecordStructure, ElementValueType
from FalloutSnip.Domain.Scripts import PyInterpreter
from FalloutSnip.Domain.Services import Spells

import markup
import HTML as tb
import renderer

def browser(htmlstr):
	import BaseHTTPServer
	import webbrowser
	class RequestHandler(BaseHTTPServer.BaseHTTPRequestHandler):
		def do_GET(self):
			bufferSize = 1024*1024
			for i in range(0, len(htmlstr), bufferSize):
				self.wfile.write(htmlstr[i:i+bufferSize])
	server = BaseHTTPServer.HTTPServer(('127.0.0.1', 0), RequestHandler)
	webbrowser.open('http://127.0.0.1:%s' % server.server_port)
	server.handle_request()           

sw = Stopwatch.StartNew()
plugin = FalloutSnip.Domain.Model.Plugin(gameDir + 'asdf.esp')
rec = None
# for rec in plugin.Records:
	# if isinstance(rec, GroupRecord):
		# break
for kvp in plugin.EnumerateRecords('RACE'):
	rec = kvp.Value
	#break
cssfile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sys.argv[0]),'../scripts/renderer.css'))
html = renderer.HTMLRenderer(title="Record",css=( cssfile, ) )
html.GetHeader(rec)
#html.GetHeader(plugin)
html.page.hr()
html.GetDescription(rec)

strpage = str(html)
with open('test4.html', "w") as f:
	f.write(strpage)
#print strpage
#browser( strpage )

sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'