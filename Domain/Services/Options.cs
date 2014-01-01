namespace TESVSnip.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using TESVSnip.Domain.Model;

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
            this.SetupGameDirectory();
            this.SetupApplicationDirectory();
            this.ParseCommandLine(args);
            this.PrepareDirectories();
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

        /// <summary>
        /// Gets the list of plugins to pre-load specified using the command-line options.
        /// </summary>
        public IEnumerable<string> Plugins
        {
            get { return this._plugins; }
        }

        public string SettingsDirectory { get; private set; }

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
            if (applicationDirectory != null)
            {
                this.SettingsDirectory = Path.Combine(applicationDirectory, "conf");
            }
        }

        /// <summary>
        /// Search the skyrim directory
        /// </summary>
        private void SetupGameDirectory()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Bethesda Softworks\Skyrim"))
                {
                    //on 64bits
                    if (key == null)
                    {
                        using (var key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Bethesda Softworks\Skyrim"))
                        {
                            //on 32bits
                            if (key2 == null) return;
                            this.GameDirectory = key2.GetValue("Installed Path", this.GameDirectory, RegistryValueOptions.None) as string;
                        }
                    }
                    else
                        this.GameDirectory = key.GetValue("Installed Path", this.GameDirectory, RegistryValueOptions.None) as string;

                    var gameDirectory = this.GameDirectory;
                    if (gameDirectory != null)
                    {
                        this.GameDataDirectory = Path.Combine(gameDirectory, "Data");
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Options.SetupGameDirectory" + Environment.NewLine + "Message: " + ex.Message + Environment.NewLine + "StackTrace: " + ex.StackTrace;
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }
        }
    }
}