using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace viewhtml
{
    public partial class MSHTMLView : Form, IBrowser
    {
        public MSHTMLView()
        {
            InitializeComponent();
        }

        public string File { get; set; }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Reload();
        }

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            Reload();
        }

        private void fileSystemWatcher1_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            Reload();
        }

        void Reload()
        {
            if (System.IO.File.Exists(this.File))
            {
                var fileToOpen = new Uri(this.File);
                this.webBrowser1.Url = fileToOpen;
            }
            else
            {
                var ub = new UriBuilder("about", "blank");
                this.webBrowser1.Url = ub.Uri;
            }
        }

        private void MSHTMLView_Load(object sender, EventArgs e)
        {
            this.fileSystemWatcher1.Path = Path.GetFullPath(Path.GetDirectoryName(this.File));
            this.fileSystemWatcher1.Filter = Path.GetFileName(this.File);
            this.fileSystemWatcher1.IncludeSubdirectories = false;
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            Reload();
        }
    }
}
