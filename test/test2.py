import startup
import shared.util as util
gameDir = util.getGameDirectory()

import System
from System.Diagnostics import Stopwatch
from System import TimeSpan
import TESVSnip.Domain
from TESVSnip.Domain.Services import Spells

import markup

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

page = markup.page( )
page.init( title="My title"
	,css=( 'bootstrap.min.css', 'bootstrap-theme.min.css' ) 
    ,script=('jquery-1.11.0.min.js','bootstrap.min.js')
	,header="Something at the top"
	,footer="The bitter end." 
	)
items = ( "Item one", "Item two", "Item three", "Item four" )
paras = ( "This was a list.", "And now for something completely different." )
images = ( "thumb1.jpg", "thumb2.jpg", "more.jpg", "more2.jpg" )
with page.ul( class_='mylist' ):
    page.li( items, class_='myitem' )
page.p( paras )
page.img( src=images, width=100, height=80, alt="Thumbnails" )

with page.div( class_="row" ):
	with page.div( class_="col-md-9" ):
		page.p('Level 1: .col-md-9')
	with page.div( class_="row" ):
		page.div(class_="col-md-6").add("Level 2: .col-md-6")
		page.div(class_="col-md-6").add("Level 2: .col-md-6")

strpage = str(page)
with open('test2.html', "w") as f:
	f.write(strpage)
#print strpage
#browser( strpage )

sw.Stop()
t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)            
print 'Script took',t,'to complete'