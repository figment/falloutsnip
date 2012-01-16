figments's TESsnip Fork (Skyrim Edition)
=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

TESVSnip is a fork of the TESsnip utility bundled with Fallout Mod Manager with changes specifically for Skyrim.

In the word of the original author, TESsnip is a low level plugin editor that allows you to open esm/esp files, view the record structure and cut/copy/paste records and subrecords between different parents, including different esp's. You can also view and edit the data saved in a subrecord, in either hex or string format, as well as editing record headers.

Requirements:
  .NET 3.5

Change Log:
v 2.4
- Bug fix to so that repeating elements such as KWDA can be added in new control
- Add config only option to allow old subrecord editor to be used

v 2.3
- Use RTF5 control if installed
  Greatly improves text rendering when links are involved and has better layout as well
- Docking windows for Record and Subrecord (not much new but can undock and relocate)
- Reformated the main display text for subrecords to use tables
- New Medium Level Record (looks like old but overhauled for future extensibility)

v 2.2
- Add toggle all button to the ESM load filter dialog
- Add expand/collapse options to tree context and edit menus
- Add option to Add Master to plugins from Edit menu or context when plugin selected
- Bug fixes for errors preventing subrecord from loading

v 2.1
- Fix bug where hex editor was not saving changes

v 2.0
- Change REFR display so X,Y are divided by 4096 to match cell locations
- Add "Form ID Ref" search type which is a backreference search for records using given formid
- Fix Conditionals on global variables display
- Fix progress bar update / cancel during search

v 1.10
- New language handling items to better support localization (Experimental)
- Better handling for Windows Copy/Paste mode
- New Icon - Scissors snipping at Skyrim is what its supposed to be

v 1.9
- Add Setting for MaxHistoryItems in config file and default to 30
- Fix History control so that redo is truncated when a new selection is made
- Add Disable Hyperlinks since some pages are very slow to load due to hyperlink processing
- Fix subrecord copy/paste so Ctrl+C and Ctrl+V work again
- Workaround issue with richtext box causing sound to be made when setting text
- Fix bug with search backward not actually searching backward
- Hacked workaround so that Del key mostly works on the find text field and is not handled by main menu

v 1.8
- Fix QUST after change to CONDITIONAL broke it

v 1.7
- Convert Text Search to a background incremental search 
  (Enables cancel during full text searches)
- Add Name w/Type and Full w/Type to allow for searches limited by record type
- Experimental short descriptions for REFR, ACHR showing location information
- Fix FirstTimeLoad of skyrim.esm where user selects cancel to options and filter is still applied
- Change main text field area to RichTextBox and add hyper links to items
- Add Forward/Back navigation buttons for the Record TreeNode


v 1.6
- Add option to use Windows Clipboard instead of internal copy
  (This is experimental and may have issues especially when copying large chunks of data)

v 1.5
- Change copy/paste semantics so appended items are not reversed

v 1.4
- Add setting to allow specification of file open or file save locations
- Add Context menu on tree to allow copy to other plugins
- Update the Sanitize Order Spell
- Substructure matching probably slower but more flexible
- Added NAVI, CELL, REFR, WRLD, LAND, PHZD, PGRE, ACHR and SCEN
- Fixed form look ups for plugins

v 1.3
- Bug fixes for crashes and other misbehaviors
- Fixed positioning for first time loading
- Synchronized RecordStructure.XML with UESP wiki
- Fixed nonconforming record search

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