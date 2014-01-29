using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace viewhtml
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string fileToWatch = null;
            IBrowser browser = null;
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("-") && arg.Length > 1)
                {
                    switch (arg.ToLower()[1])
                    {
                        case 'i':
                            browser = new MSHTMLView();
                            break;
                        case 'h':
                            browser = new HtmlForm();
                            break;
                    }
                }
                else
                {
                    fileToWatch = Path.GetFullPath(arg);
                }
            }
            if (string.IsNullOrEmpty(fileToWatch))
            {
                MessageBox.Show("No file specified.");
                return;
            }
            if (browser == null)
                browser = new DualForm();

            Environment.CurrentDirectory = Path.GetFullPath(Path.GetDirectoryName(fileToWatch));

            browser.File = fileToWatch;

            Application.Run( browser as Form );
        }
    }
}
