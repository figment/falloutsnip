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
    public partial class DualForm : Form, IBrowser
    {
        public DualForm()
        {
            InitializeComponent();
        }

        HashSet<string> watchedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase); 
        Dictionary<string, FileSystemWatcher> otherFolders = new Dictionary<string, FileSystemWatcher>(StringComparer.InvariantCultureIgnoreCase); 

        public string File { get; set; }

        private void watcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            if (!watchedFiles.Contains(e.FullPath)) return;
            Reload();
        }

        void Reload()
        {
            if (System.IO.File.Exists(this.File))
            {
                try
                {
                    using (var stream = new FileStream(this.File, FileMode.Open, FileAccess.Read,
                        FileShare.Read, 4096, FileOptions.SequentialScan))
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd();
                        this.htmlPanel1.Text = text;
                        this.webBrowser1.DocumentText = text;
                        //this.webBrowser1.Url = new Uri(this.File);
                    }
                    return;
                }
                catch
                {
                }
            }
            this.htmlPanel1.Text = "";
            this.webBrowser1.DocumentText = "";
        }

        private void HtmlForm_Load(object sender, EventArgs e)
        {
            this.fileSystemWatcher1.Path = Path.GetFullPath(Path.GetDirectoryName(this.File));
            //this.fileSystemWatcher1.Filter = Path.GetFileName(this.File);
            this.fileSystemWatcher1.IncludeSubdirectories = true;
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            watchedFiles.Add(Path.GetFullPath(this.File));
            Reload();
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //e.Cancel = true;
            var link = e.Url.ToString();
            //if (link != "about:blank")
            //{
            //    e.Cancel = true;
            //    MessageBox.Show(link);
            //}
        }

        private void htmlPanel1_LinkClicked(object sender, HtmlRenderer.Entities.HtmlLinkClickedEventArgs e)
        {
            e.Handled = true;
            if (!string.IsNullOrWhiteSpace(e.Link))
                MessageBox.Show(e.Link.ToString());
        }

        private void htmlPanel1_RenderError(object sender, HtmlRenderer.Entities.HtmlRenderErrorEventArgs e)
        {

        }

        private void htmlPanel1_ImageLoad(object sender, HtmlRenderer.Entities.HtmlImageLoadEventArgs e)
        {
            watchedFiles.Add(Path.GetFullPath(Path.Combine(this.fileSystemWatcher1.Path, e.Src)));
        }

        private void htmlPanel1_StylesheetLoad(object sender, HtmlRenderer.Entities.HtmlStylesheetLoadEventArgs e)
        {
            var path = Path.GetFullPath(Path.IsPathRooted(e.Src) ? e.Src : Path.Combine(this.fileSystemWatcher1.Path, e.Src));
            watchedFiles.Add(path);

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                FileSystemWatcher watcher;
                if (!otherFolders.TryGetValue(dir, out watcher))
                {
                    watcher = new FileSystemWatcher(dir) {EnableRaisingEvents = true};
                    watcher.Changed += watcherChanged;
                    watcher.Created += watcherChanged;
                    watcher.Deleted += watcherChanged;
                    otherFolders.Add(dir, watcher);
                }
            }
        }
    }
}
