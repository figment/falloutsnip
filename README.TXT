    figments's TESsnip Fork 1.0
    =-=-=-=-=-=-=-=-=-=-=-=-=-=

    This packages includes a fork of the Fallout Mod Manager 
      TESsnip utility.

Change Log:
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
- Sorry the new string editor got away from me and is rather 
  fiddly and not intuitive but that is in line with TESsnip anyway
- Detect localization or allow selection of localization in future
- Add/update spell to convert from ESP to ESM which also fixes LString types

Credits:
* timeslip - The original author of TESsnip


License:
* The TESsnip code is licensed under GPL so this is by extension also GPL
* Included third party components are covered by their own license

Source:
git://github.com/figment/tesvsnip.git