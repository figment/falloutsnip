#
# Required imports by all scripts
#
import clr
clr.AddReferenceByPartialName("TESVSnip.Framework")
clr.AddReferenceByPartialName("TESVSnip.Domain")
clr.AddReferenceByPartialName("System.Core")
try:
	# TESVSnip Application may not be loaded on command line
	clr.AddReferenceByPartialName("TESVSnip")
	clr.AddReferenceByPartialName("System.Windows.Forms")
	clr.AddReferenceByPartialName("System.Drawing")
except:
	pass
