using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using FalloutSnip.Domain.Model;

namespace FalloutSnip.UI.Services
{
    /// <summary>
    /// Global program options.
    /// </summary>
    public sealed class Options
    {
        private static Options _instance;

        private readonly List<string> _plugins = new List<string>();

        private Options(string[] args)
        {
            this.SettingsDirectory = Environment.CurrentDirectory;
            this.ApplicationDirectory = Environment.CurrentDirectory;
            this.SetupApplicationDirectory();
            this.SetupScriptHostDirectory();
            Reconfigure();
            this.ParseCommandLine(args);
        }

        /// <summary>
        /// Gets access to the single instance of this class.
        /// </summary>
        public static Options Value
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "Domain_Services_Options_NotInitializedYet")); // Program options are not initialized yet.
                }

                return _instance;
            }
        }

        public string ApplicationDirectory { get; private set; }

        public string GameDataDirectory { get; private set; }

        public string GameDirectory { get; private set; }

        public string IronPythonDirectory { get; private set; }

        public string ScriptsDirectory
        {
            get { return Domain.Services.Folders.ScriptsDirectory; }
            private set { Domain.Services.Folders.ScriptsDirectory = value; }
        }

        /// <summary>
        /// Gets the list of plugins to pre-load specified using the command-line options.
        /// </summary>
        public IEnumerable<string> Plugins
        {
            get { return this._plugins; }
        }

        public string SettingsDirectory 
        {
            get { return Domain.Services.Folders.SettingsDirectory; }
            private set { Domain.Services.Folders.SettingsDirectory = value; }
        }

        /// <summary>
        /// Initializes the global options parsing the given <paramref name="args"/> array.
        /// </summary>
        /// <param name="args">
        /// The received command-line options.
        /// </param>
        public static void Initialize(string[] args)
        {
            _instance = new Options(args);
        }

        public void Reconfigure()
        {
            this.SetupGameDirectory();
            this.PrepareDirectories();
        }

        /// <summary>
        /// Parse the command line <paramref name="args"/> array.
        /// </summary>
        /// <param name="args"></param>
        private void ParseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }

                if (arg[0] == '-' || arg[0] == '/')
                {
                    if (arg.Length == 1)
                    {
                        continue;
                    }

                    switch (char.ToLower(arg[1]))
                    {
                        case 'c':
                            this.SettingsDirectory = (arg.Length > 2 && arg[2] == ':') ? arg.Substring(3) : args[++i];
                            break;
                    }
                }
                else
                {
                    this._plugins.Add(arg);
                }
            }
        }

        /// <summary>
        /// Prepare directories
        /// </summary>
        private void PrepareDirectories()
        {
            if (string.IsNullOrWhiteSpace(this.GameDirectory))
            {
                this.GameDirectory = Environment.CurrentDirectory;
            }

            if (string.IsNullOrWhiteSpace(this.GameDataDirectory))
            {
                this.GameDataDirectory = Environment.CurrentDirectory;
            }

            if (Directory.Exists(this.GameDataDirectory))
            {
                Environment.CurrentDirectory = this.GameDataDirectory;
            }
        }

        /// <summary>
        /// Retrieve the application directory and settings directory (conf)
        /// </summary>
        private void SetupApplicationDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            this.ApplicationDirectory = Path.GetDirectoryName(assembly.Location);
            var applicationDirectory = this.ApplicationDirectory;
            if (!string.IsNullOrWhiteSpace(applicationDirectory))
            {
                var dir = new DirectoryInfo(applicationDirectory);
                if (string.Compare(dir.Name, "Debug", StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(dir.Name, "Release", StringComparison.OrdinalIgnoreCase) == 0)
                    dir = dir.Parent;
                if (dir != null && string.Compare(dir.Name, "bin", StringComparison.OrdinalIgnoreCase) == 0)
                    dir = dir.Parent;
                if (dir != null && String.Compare(dir.Name, "Application", StringComparison.OrdinalIgnoreCase) == 0)
                    dir = dir.Parent;
                applicationDirectory = dir.FullName;

            }
            if (!string.IsNullOrWhiteSpace(applicationDirectory))
            {
                var confFolder = Path.GetFullPath(Path.Combine(applicationDirectory, "conf"));
                if (!Directory.Exists(confFolder))
                    confFolder = Path.GetFullPath(Path.Combine(applicationDirectory, "..", "conf"));
                SettingsDirectory = confFolder;

                ScriptsDirectory = Path.GetFullPath(Path.Combine(applicationDirectory, "scripts"));
                if (!Directory.Exists(confFolder))
                    ScriptsDirectory = Path.GetFullPath(Path.Combine(applicationDirectory, "..", "scripts"));
            }
        }

        /// <summary>
        ///     Locate the IronPython install location for scripting
        /// </summary>
        private void SetupScriptHostDirectory()
        {
            var path = Properties.Settings.Default.IronPythonPath;

            if (!string.IsNullOrWhiteSpace(path) 
                && Directory.Exists(path)
                && Directory.Exists(Path.Combine(path, "lib"))
                && Directory.Exists(Path.Combine(path, "DLLs"))
                )
            {
                IronPythonDirectory = path;
            }
            else // search 
            {
                using (var key = Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem
                    ? @"SOFTWARE\Wow6432Node\IronPython\2.7\InstallPath"
                    : @"SOFTWARE\IronPython\2.7\InstallPath"))
                {
                    if (key != null)
                    {
                        IronPythonDirectory = key.GetValue(null, "", RegistryValueOptions.None) as string;
                    }
                    else
                    {
                        path = Path.GetFullPath(Path.Combine(ApplicationDirectory, "..", "..", "..", "vendor", "IronPython"));
                        if (!string.IsNullOrWhiteSpace(path)
                            && Directory.Exists(path)
                            && Directory.Exists(Path.Combine(path, "lib"))
                            && Directory.Exists(Path.Combine(path, "DLLs"))
                            )
                        {
                            IronPythonDirectory = path;
                        }
                        else
                        {
                            IronPythonDirectory = ApplicationDirectory;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Search the skyrim directory
        /// </summary>
        private void SetupGameDirectory()
        {
            try
            {
                string defaultDomain = Properties.Settings.Default.DefaultDomain ?? "Skyrim";
                var domain = FalloutSnip.Domain.Data.DomainDefinition.Lookup(defaultDomain);
                if (domain == null)
                    return;

                this.GameDirectory = domain.GameDirectory;
                this.GameDataDirectory = domain.GameDataDirectory;
            }
            catch (Exception ex)
            {
                string msg = "Options.SetupGameDirectory" + Environment.NewLine + "Message: " + ex.Message + Environment.NewLine + "StackTrace: " + ex.StackTrace;
                //Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }
        }
    }
}