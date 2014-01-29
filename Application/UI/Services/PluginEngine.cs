using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using TESVSnip.Framework.Services;
using TESVSnip.UI.Forms;

namespace TESVSnip.UI.Services
{
    internal class PluginEngine : IDisposable
    {
        // Path for dynamic UI scripts
        public static readonly string PluginsPyPath = Path.Combine(Options.Value.ScriptsDirectory, @"plugins");

        // Engine to access IronPython
        private ScriptEngine pyEngine;

        readonly private PluginStream outputStream = new PluginStream();

        public class MessageEventArgs : EventArgs
        {
            public MessageEventArgs(string text)
            {
                this.Text = text;
            }
            public string Text { get; private set; }
        }

        class PluginStream : MemoryStream
        {
            public event EventHandler<MessageEventArgs> OnWrite;
            public override void Write(byte[] buffer, int offset, int count)
            {
                base.Write(buffer, offset, count);
                if (OnWrite != null)
                    OnWrite(this, new MessageEventArgs(System.Text.Encoding.UTF8.GetString(buffer, offset, count)));
            }
        }

        static PluginEngine()
        {
            PluginEngine.Default = new PluginEngine();
        }
        public static PluginEngine Default { get; private set; }

        public List<PluginBase> Plugins
        {
            get { return PluginStore.Plugins; }
        }

        public event EventHandler<MessageEventArgs> OnConsoleMessage
        {
            add { this.outputStream.OnWrite += value; }
            remove { this.outputStream.OnWrite -= value; }
        }

        public event EventHandler<MessageEventArgs> OnErrorMessage;

        public void Initialize()
        {
            ClearOutput();

            pyEngine = Python.CreateEngine(); // Create an engine to access IronPython
            var runtime = pyEngine.Runtime;

            var paths = pyEngine.GetSearchPaths().ToList();
            paths.Add(PluginsPyPath);
            paths.Add(Path.Combine(Options.Value.ScriptsDirectory, "lib"));
            pyEngine.SetSearchPaths(paths);

            runtime.IO.SetOutput(outputStream, System.Text.Encoding.UTF8);
            runtime.IO.SetErrorOutput(outputStream, System.Text.Encoding.UTF8);

            runtime.LoadAssembly(Assembly.GetExecutingAssembly());
            runtime.LoadAssembly(typeof(TESVSnip.Domain.Model.BaseRecord).Assembly);
            runtime.LoadAssembly(typeof(String).Assembly);
            runtime.LoadAssembly(typeof(System.Drawing.Icon).Assembly);
            runtime.LoadAssembly(typeof(Python).Assembly);
            runtime.LoadAssembly(typeof(System.Dynamic.DynamicObject).Assembly);
            runtime.LoadAssembly(typeof(System.Windows.Forms.Cursor).Assembly);

            LoadPlugins();
        }

        public void Cleanup()
        {
            UnloadPlugins();
            if (pyEngine != null)
                pyEngine.Runtime.Shutdown();
            pyEngine = null;
            ClearOutput();
            GC.Collect();
        }

        public void Dispose()
        {
            Cleanup();
        }

        public void Reinitialize()
        {
            Cleanup();
            Initialize();
        }

        public void LoadPlugins()
        {
            if (Directory.Exists(PluginsPyPath))
            {
                foreach (var file in Directory.EnumerateFiles(PluginsPyPath, "*.py", SearchOption.TopDirectoryOnly))
                {
                    RegisterPlugin(file);                   
                }
            }
        }

        public void UnloadPlugins()
        {
            PluginStore.Cleanup();
        }

        public void RegisterPlugin(string path)
        {
            try
            {
                var scope = pyEngine.CreateScope();
                scope.SetVariable("__window__", Application.OpenForms.OfType<MainView>().FirstOrDefault());
                scope.SetVariable("__plugins__", TESVSnip.Domain.Model.PluginList.All);
                scope.SetVariable("__options__", Options.Value);
                scope.SetVariable("__settings__", TESVSnip.Properties.Settings.Default);
                pyEngine.ExecuteFile(path, scope);
            }
            catch (SyntaxErrorException e)
            {
                ShowError("Syntax error in \"{0}\"", Path.GetFileName(path), e);
            }
            catch (SystemExitException e)
            {
                ShowError("SystemExit in \"{0}\"", Path.GetFileName(path), e);
            }
            catch (Exception e)
            {
                ShowError("Error loading plugin \"{0}\"", Path.GetFileName(path), e);
            }
        }

        public string GetOutputText()
        {
            return System.Text.Encoding.UTF8.GetString(outputStream.ToArray());
        }

        public void ClearOutput()
        {
            outputStream.Position = 0;
            outputStream.SetLength(0);            
        }
        
        public void ShowError(string title, string name, Exception e)
        {
            var eo = pyEngine.GetService<ExceptionOperations>();

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("ERROR: ").AppendFormat(title, name).AppendLine().Append(eo.FormatException(e));
            var error = sb.ToString();
            var bytes = System.Text.Encoding.UTF8.GetBytes(error);
            this.outputStream.Write(bytes, 0, bytes.Length);

            if (OnErrorMessage != null)
                OnErrorMessage(this, new MessageEventArgs(error));
        }

        public void ExecuteByName(string name)
        {
            var plugins = PluginStore.Plugins.FindAll(x => x.SupportGlobal && x.Name == name).ToList();
            foreach (var plugin in plugins)
            {
                try
                {
                    var recs = TESVSnip.Domain.Model.PluginList.All.Records
                        .Cast<TESVSnip.Domain.Model.Plugin>().ToArray();
                    plugin.Execute(recs);
                }
                catch (Exception e)
                {
                    ShowError("Error executing plugin \"{0}\"", plugin.Name, e);
                }                
            }
        }

        public bool IsValidSelectionByName(string name, IList selection)
        {
            var plugins = PluginStore.Plugins.FindAll(x => x.SupportsSelection && x.Name == name).ToList();
            foreach (var plugin in plugins)
            {
                try
                {
                    if (plugin.IsValidSelection(selection))
                        return true;
                }
                catch (Exception e)
                {
                    ShowError("Error executing plugin \"{0}\"", plugin.Name, e);
                }
            }
            return false;
        }

        public void ExecuteSelectionByName(string name, IList selection)
        {
            var plugins = PluginStore.Plugins.FindAll(x => x.SupportsSelection && x.Name == name).ToList();
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Execute(selection);
                }
                catch (Exception e)
                {
                    ShowError("Error executing plugin \"{0}\"", plugin.Name, e);
                }
            }
        }
    }
}
