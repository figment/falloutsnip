using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TESVSnip.Domain.Services
{
    public static class Folders
    {
       
        static Folders()
        {
            var assembly = Assembly.GetExecutingAssembly();
            ModuleDirectory = Path.GetDirectoryName(assembly.Location);
            if (!string.IsNullOrWhiteSpace(ModuleDirectory))
            {
                var dir = new DirectoryInfo(ModuleDirectory);
                if (System.String.Compare(dir.Name, "Debug", System.StringComparison.OrdinalIgnoreCase) == 0
                    || System.String.Compare(dir.Name, "Release", System.StringComparison.OrdinalIgnoreCase) == 0)
                    dir = dir.Parent;
                if (System.String.Compare(dir.Name, "bin", System.StringComparison.OrdinalIgnoreCase) == 0)
                    dir = dir.Parent;
                ModuleDirectory = dir.FullName;
            }
            if (!string.IsNullOrWhiteSpace(ModuleDirectory))
            {
                var confFolder = Path.Combine(ModuleDirectory, "conf");
                if (!Directory.Exists(confFolder))
                    confFolder = Path.GetFullPath(Path.Combine(ModuleDirectory, "..", "conf"));
                SettingsDirectory = confFolder;
                ScriptsDirectory = Path.Combine(SettingsDirectory, "scripts");
            }
        }

        public static string ModuleDirectory { get; set; }
        public static string SettingsDirectory { get; set; }
        public static string ScriptsDirectory { get; set; }
        
    }
}
