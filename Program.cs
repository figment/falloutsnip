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
        static void Main()
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


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainView());
        }
    }
}
