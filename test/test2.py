import startup
import shared.util as util
gameDir = util.getGameDirectory()

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan
import TESVSnip.Domain
from TESVSnip.Domain.Services import Spells


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

# add code here


sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'