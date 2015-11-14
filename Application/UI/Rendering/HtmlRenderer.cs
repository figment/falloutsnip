using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using FalloutSnip.Domain.Model;
using FalloutSnip.Domain.Rendering;
using FalloutSnip.Domain.Services;
using FalloutSnip.UI.Services;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace FalloutSnip.UI.Rendering
{
    /// <summary>
    /// CLASS PyInterpreter
    /// </summary>
    public static class HtmlRenderer
    {
        private static readonly string RendererPyPath = Path.Combine(Folders.ScriptsDirectory, @"renderer.py");

        // Engine to access IronPython
        private static ScriptEngine pyEngine;

        // Performing tasks with the script
        private static ObjectOperations objOps;

        // compililed source code
        private static CompiledCode compiledCode;

        // Load the script
        private static ScriptSource source;

        // Default scope for executing the script
        private static ScriptScope scope;

        // The class object.
        private static Object rendererClass;

        // Instance of the Renderer
        private static IRenderer rendererImpl;

        // watcher to watch for changes to the renderer and reload on change
        private static FileSystemWatcher watcher;

        // list of watched files
        private static readonly HashSet<string> watchedFiles =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        // default renderer text
        private const string DefaultRendererText = "ERROR: Renderer incorrectly configured";
        private static string defaultRenderText = DefaultRendererText;

        /// <summary>
        /// Init the py interpreter
        /// </summary>
        public static void Initialize()
        {
            pyEngine = Python.CreateEngine(); // Create an engine to access IronPython
            objOps = pyEngine.CreateOperations(); // Performing tasks with the script
            scope = pyEngine.CreateScope(); // Default scope for executing the script

            //var oldPaths = pyEngine.GetSearchPaths().ToList();
            var paths = new List<string>();
            //paths.Add(PluginEngine.PluginsPyPath);
            if (!string.IsNullOrEmpty(Options.Value.IronPythonDirectory))
            {
                paths.Add(Options.Value.IronPythonDirectory);
                paths.Add(Path.Combine(Options.Value.IronPythonDirectory, "lib"));
                paths.Add(Path.Combine(Options.Value.IronPythonDirectory, "DLLs"));
            }
            paths.Add(Folders.ScriptsDirectory);
            paths.Add(Path.Combine(Folders.ScriptsDirectory, "lib"));

            pyEngine.SetSearchPaths(paths.Where(Directory.Exists).ToList());

            var runtime = pyEngine.Runtime;
            runtime.LoadAssembly(Assembly.GetExecutingAssembly());
            runtime.LoadAssembly(typeof(FalloutSnip.Framework.TypeConverter).Assembly);
            runtime.LoadAssembly(typeof(FalloutSnip.Domain.Model.BaseRecord).Assembly);
            runtime.LoadAssembly(typeof(String).Assembly);
            runtime.LoadAssembly(typeof(IronPython.Hosting.Python).Assembly);
            runtime.LoadAssembly(typeof(System.Dynamic.DynamicObject).Assembly);


            watcher = new FileSystemWatcher
                {
                    Path = Path.GetFullPath(Path.GetDirectoryName(RendererPyPath)),
                    Filter = Path.GetFileNameWithoutExtension(RendererPyPath) + ".*",
                    IncludeSubdirectories = false
                };
            watcher.Changed += watcher_Changed;
            watchedFiles.Add(Path.GetFullPath(RendererPyPath));
            watchedFiles.Add(Path.ChangeExtension(RendererPyPath, ".css"));
            watcher.EnableRaisingEvents = true;
            Reload();
        }

        private static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            //if (watchedFiles.Contains(e.FullPath))
            System.Threading.Thread.Sleep(1000);
            Reload();
        }

        public static void Reload()
        {
            try
            {
                defaultRenderText = DefaultRendererText;
                source = pyEngine.CreateScriptSourceFromFile(RendererPyPath); // Load the script
                compiledCode = source.Compile();
                compiledCode.Execute(scope); // Create class object
                rendererClass = scope.GetVariable("Renderer"); // Retrieve the class object
                if (rendererClass != null)
                    rendererImpl = objOps.Invoke(rendererClass) as IRenderer; // Create an instance of the Renderer
            }
            catch (Exception e)
            {
                defaultRenderText = e.ToString();
                source = null;
                compiledCode = null;
                rendererClass = null;
                rendererImpl = null;
            }
        }

        public static void Shutdown()
        {
            if (watcher != null)
            {
                watcher.Changed -= watcher_Changed;
                watcher.EnableRaisingEvents = false;
                watcher = null;
            }
            source = null;
            compiledCode = null;
            rendererClass = null;
            rendererImpl = null;
            pyEngine = null;
            objOps = null;
        }

        public static string GetDescription(BaseRecord rec)
        {
            if (rendererImpl == null)
            {
return defaultRenderText;
//                var sb = new StringBuilder();
//                Extensions.StringRenderer.GetFormattedData(rec, sb);
//                return sb.ToString();
            }
            try
            {
                var kwargs = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                kwargs["title"] = "Record";
                kwargs["css"] = new string[] {Path.ChangeExtension(RendererPyPath, ".css")};
                return rendererImpl.Render(rec, kwargs);
            }
            catch (Exception ex)
            {
                return ex.ToString();
                //return Extensions.StringRenderer.GetDesc(rec);
            }            
        }
    }
}