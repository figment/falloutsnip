#
# Required imports by all scripts
#
import os, sys
binpath = os.getcwd() + r'\..\Domain\bin\release\\'
sys.path.append(binpath)
sys.path.append(os.getcwd() + r'\..\conf\plugins')

import clr
#clr.AddReferenceByPartialName("TESVSnip.Domain")
clr.AddReferenceToFileAndPath(binpath + 'TESVSnip.Framework.dll')
clr.AddReferenceToFileAndPath(binpath + 'TESVSnip.Domain.dll')
clr.AddReferenceByPartialName("System.Core")
#import TESVSnip.Framework, TESVSnip.Domain