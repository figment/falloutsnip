#
# Required imports by all scripts
#
import clr
clr.AddReferenceByPartialName("FalloutSnip.Framework")
clr.AddReferenceByPartialName("FalloutSnip.Domain")
clr.AddReferenceByPartialName("System.Core")
try:
	# FalloutSnip Application may not be loaded on command line
	clr.AddReferenceByPartialName("FalloutSnip")
	clr.AddReferenceByPartialName("System.Windows.Forms")
	clr.AddReferenceByPartialName("System.Drawing")
except:
	pass
