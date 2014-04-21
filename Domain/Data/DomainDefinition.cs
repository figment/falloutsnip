#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TESVSnip.Domain.Data.Structure;
using TESVSnip.Domain.Data.Structure.Xml;
using TESVSnip.Domain.Properties;
using TESVSnip.Domain.Services;
using TESVSnip.Framework.IO;

#endregion

namespace TESVSnip.Domain.Data
{
    /// <summary>
    ///     The intent of this class is to hold all "static" information related to
    ///     configuration of a game.  This allows Morrowind, Oblivion, Skyrim config files
    ///     to be loaded simultaneously yet seperately.  This is mostly for command line
    ///     and scripting application.
    /// </summary>
    public class DomainDefinition
    {
        private static readonly Dictionary<string, DomainDefinition> Domains = new Dictionary<string, DomainDefinition>(StringComparer.InvariantCultureIgnoreCase);
        private readonly string xmlPath;

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Master { get; private set; }
        public string RegistryKey { get; private set; }
        public string[] FilteredESM { get; private set; }
        public string[] AllESMRecords { get; private set; }
        public string HEDRType { get; private set; }
        public float HEDRVersion { get; private set; }

        public Settings Settings { get; private set; }
        public bool Loaded { get; private set; }
        public Dictionary<string, RecordStructure> Records { get; private set; }

        public static event EventHandler DomainLoaded;


        static DomainDefinition()
        {
            var iniFile = Path.Combine(Folders.SettingsDirectory, "Domains.ini");
            foreach (var section in IniFile.GetSectionNames(iniFile))
            {
                var values = IniFile.GetPropertyValues(section);
                var define = new DomainDefinition(section);
                define.DisplayName = GetValue(values, "Display", section);
                define.Master = GetValue(values, "Master", section+".esm");
                define.RegistryKey = GetValue(values, "Registry", "Bethesda Softworks\\" + section);
                define.FilteredESM = GetValue(values, "FilteredESM", "").Split(';');
                define.AllESMRecords = GetValue(values, "AllESMRecords", "").Split(';');
                define.HEDRType = GetValue(values, "HEDRType", "TES4");
                define.HEDRVersion = float.Parse(GetValue(values, "HEDRVersion", "1.0"));
                Domains[section] = define;
            }
        }

        private static string GetValue(Dictionary<string, string> dict, string key, string defaultValue)
        {
            string result;
            if (dict.TryGetValue(key, out result))
                return result;
            return defaultValue;
        }

        public DomainDefinition(string name)
        {
            Name = name;
            Settings = Settings.Default;
            xmlPath = Path.Combine(Folders.SettingsDirectory, Name, @"RecordStructure.xml");
            Records = new Dictionary<string, RecordStructure>(0);
        }

        public static IEnumerable<DomainDefinition> LoadedDomains()
        {
            return Domains.Values;
        }

        public static DomainDefinition Load(string p)
        {
            DomainDefinition define;
            if (!Domains.TryGetValue(p, out define))
            {
                define = new DomainDefinition(p);
                Domains[p] = define;
            }
            if (!define.Loaded)
            {
                define.Records = RecordStructure.Load(define.xmlPath);
                define.Loaded = true;
                if (DomainLoaded != null)
                    DomainLoaded(define, EventArgs.Empty);
            }
            return define;
        }

        public static void Reload()
        {
            foreach (var domainDefinition in Domains.Values)
            {
                if (domainDefinition.Loaded)
                    domainDefinition.Records = RecordStructure.Load(domainDefinition.xmlPath);
            }
            if (DomainLoaded != null)
                DomainLoaded(null, EventArgs.Empty);
        }

        public static DomainDefinition DetectDefinitionFromVersion(string type, float version)
        {
            const float EPSILON = Single.Epsilon * 10;
            foreach (var domain in Domains.Values.Where(domain => type == domain.HEDRType 
                && Math.Abs(version - domain.HEDRVersion) < EPSILON))
            {
                return domain;
            }
            throw new Exception("File is not a known TES4 file (Unexpected version)");
        }

        public static RecordStructure GetFirstRecordOfType(string type)
        {
            RecordStructure rec = null;
            if (LoadedDomains().Any(domain => domain.Records.TryGetValue(type, out rec)))
                return rec;
            return null;
        }

        public static string[] GetRecordNames()
        {
            var recNames = new List<string>();
            foreach (var domain in LoadedDomains())
                recNames.AddRange(domain.Records.Keys);
            return recNames.Distinct().ToArray();
        }

        public static object[] GetRecordNamesAsObjects()
        {
            var names = GetRecordNames();
            var dest = new object[names.Length];
            Array.Copy(names, 0, dest, 0, names.Length);
            return dest;
        }
    }
}