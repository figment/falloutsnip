using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TESsnip
{

    static class Program
    {
        public static string settingsDir { get; set; }
        public static string exeDir { get; set; }
        public static string gameDir { get; set; }
        public static string gameDataDir { get; set; }
        

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            settingsDir = System.Environment.CurrentDirectory;
            exeDir = System.Environment.CurrentDirectory;
            gameDir = System.Environment.CurrentDirectory;
            gameDataDir = System.Environment.CurrentDirectory;
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                exeDir = System.IO.Path.GetDirectoryName(asm.Location);
                settingsDir = System.IO.Path.Combine(exeDir, "conf");

                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Bethesda Softworks\Skyrim"))
                {
                    gameDir = key.GetValue("Installed Path", gameDir, Microsoft.Win32.RegistryValueOptions.None) as string;
                    gameDataDir = System.IO.Path.Combine(gameDir, "Data");
                }
            }
            catch
            {
            	
            }
            List<string> plugins = new List<string>();
            for (int i=0;i<args.Length;++i)
            {
                string arg = args[i];
                if (string.IsNullOrEmpty(arg))
                    continue;
                if (arg[0] == '-' || arg[0] == '/')
                {
                    if (arg.Length == 1)
                        continue;
                    switch (char.ToLower(arg[1]))
                    {
                        case 'c':
                            settingsDir = (arg.Length > 2 && arg[2] == ':') ? arg.Substring(3) : args[++i];
                            break;
                    }
                }
                else
                {
                    plugins.Add(arg);
                }

            }

            if (System.IO.Directory.Exists(gameDataDir))
            {
                System.Environment.CurrentDirectory = gameDataDir;
            }

            Properties.Settings.Default.Reload();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainView main = new MainView();
            foreach (string arg in plugins)
            {
                main.LoadPlugin(arg);
            }
            Application.Run(main);
            Properties.Settings.Default.Save();
        }
    }
}
