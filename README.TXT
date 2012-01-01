figments's TESsnip Fork (Skyrim Edition) version 1.2
=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

TESVSnip is a fork of the TESsnip utility bundled with Fallout Mod Manager with changes specifically for Skyrim.

In the word of the original author, TESsnip is a low level plugin editor that allows you to open esm/esp files, view the record structure and cut/copy/paste records and subrecords between different parents, including different esp's. You can also view and edit the data saved in a subrecord, in either hex or string format, as well as editing record headers.

Change Log:
v 1.2
- Bug fix in handling lstring which prevent sub editor from opening
- Support multiple subrecord copy/paste (only from the subrecord toolbar)
- Change IDs for GMST and EFID

v 1.1
- Major update to RecordStructure to allow inline groups
- Programatically tried to discover layout of all records so we have a better first pass
- Improved usability on the subrecords by adding toolbar
- Improved incremental search and nonconforming record search
- Improved performance when doing a lot of activities like search or generation of detailed reports
- Added Ability to exclude certain record types on load of skyrim.esm to conserve memory
- Added Str4 data type for fixed 4-byte strings

v 1.0
- Add LString data type to represent localized string
- Import the strings directory if present 
- Export strings to the Strings directory if strings are present for a plugin
- Add a simple string viewer for localized strings (hard coded to English)
- Update the LString data type for thinks like FULL subrecords

Notes:
- The check box on the LString record editor is for controlling whether to 
  store the integer ID (which is in Hex) or to store the actual text.
- When checked it stores the actual text.  When not checked it stores the id.
   
TODO:
- QUST record is mess and may require a new XML format to deal with it properly
- Substructures like MO2T and MO3T have complicated repeating structures not possible to edit currently
- Sorry the new string editor got away from me and is rather 
  fiddly and not intuitive but that is in line with TESsnip anyway
- Detect localization or allow selection of localization in future
- Add/update spell to convert from ESP to ESM which also fixes LString types

Credits:
* timeslip - The original author of TESsnip
* Dave Humphrey - Maintainer of http://www.uesp.net/wiki a source ESM layout
                - Tes5Lib (http://github.com/uesp/tes5lib)

License:
* The TESsnip code is licensed under GPL so this is by extension also GPL
* Included third party components are covered by their own license

Source:
git://github.com/figment/tesvsnip.git