#
# Required imports by all scripts
#
import os, sys
binpath = os.getcwd() + r'\..\Domain\bin\release\\'
sys.path.append(binpath)
sys.path.append(os.getcwd() + r'\..\conf\plugins')

import clr
#clr.AddReferenceByPartialName("FalloutSnip.Domain")
clr.AddReferenceToFileAndPath(binpath + 'FalloutSnip.Framework.dll')
clr.AddReferenceToFileAndPath(binpath + 'FalloutSnip.Domain.dll')
clr.AddReferenceByPartialName("System.Core")
#import FalloutSnip.Framework, FalloutSnip.Domain