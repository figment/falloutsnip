using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PythonConsoleControl;
using TESVSnip.Domain.Model;
using TESVSnip.Domain.Services;

namespace TESVSnip.Framework.Services
{
    /// <summary>
    /// Service Class for loading the IronPython Installation
    /// </summary>
    public static class Scripting
    {
        static readonly HashSet<string> registeredLibraries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static void Initialize()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Options.Value.IronPythonDirectory)
                    && Directory.Exists(Options.Value.IronPythonDirectory))
                {
                    registeredLibraries.Add("IronPython");
                    registeredLibraries.Add("IronPython.Modules");
                    registeredLibraries.Add("Microsoft.Dynamic");
                    registeredLibraries.Add("Microsoft.Scripting");
                    registeredLibraries.Add("Microsoft.Scripting.Metadata");

                    AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
                }
            }
            catch
            {
            }
        }

        private static Assembly CustomResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Split(new[] {','}).FirstOrDefault().Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (Path.GetExtension(name).ToLower() == ".dll") 
                    name = Path.GetFileNameWithoutExtension(name);
                if (registeredLibraries.Contains(name))
                {
                    string fileName = Path.GetFullPath(Path.Combine(Options.Value.IronPythonDirectory, name + ".dll"));
                    if (File.Exists(fileName))
                        return Assembly.LoadFile(fileName);
                }
            }
            return null;
        }

        internal static void BootstrapConsole(IronPythonConsoleControl console, Action<PythonConsole> initCallback = null)
        {
            // initialize when ready
            console.WithHost(host => 
            {
                ConfigureVariables(host.Console);
                CallInitializeScript(host.Console);
                if (initCallback != null)
                    initCallback(host.Console);
            });
        }

        internal static void ConfigureVariables(PythonConsole console)
        {           
            
        }

        internal static void CallInitializeScript(PythonConsole console)
        {
            
        }
    }
}
