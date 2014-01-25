using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TESVSnip.Framework.Services
{
    public class Platform
    {

        [DllImport("Kernel32.dll")]
        public static extern bool SetDllDirectory([In]string lpPathName);

        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;
        private const uint LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010;
        private const uint LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040;
        private const uint LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr dllPointer);

        private static string assemblyDir;
        static HashSet<string> registeredLibraries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static void Initialize()
        {
            try
            {
                // bootstrap zlib for correct platform
                Assembly asm = Assembly.GetExecutingAssembly();
                assemblyDir = Path.GetDirectoryName(asm.Location);
                //var platformPath = Path.Combine(exeDir, Path.Combine("platform", Environment.Is64BitProcess ? "x64" : "win32"));
                var platformPath = Path.Combine(assemblyDir, "platform", Environment.Is64BitProcess ? "x64" : "x86"); // Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
                SetDllDirectory(platformPath);

                AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
            }
            catch
            {
            }
        }

        public static Assembly LoadAssembly(string assembly)
        {
            return Assembly.Load(assembly);
        }

        /// <summary>
        /// Register library name as dynamic
        /// </summary>
        /// <param name="name">Complete library name including extension without path</param>
        public static void RegisterLibrary(string name)
        {
            registeredLibraries.Add(name);
        }

        public static void UnregisterLibrary(string name)
        {
            registeredLibraries.Remove(name);
        }

        private static Assembly CustomResolve(object sender, ResolveEventArgs args)
        {
            if (registeredLibraries.Contains(args.Name))
            {
                // Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
                string fileName = Path.GetFullPath(Path.Combine(assemblyDir, "platform", Environment.Is64BitProcess ? "x64" : "x86", args.Name));
                if (File.Exists(fileName))
                {
                    return Assembly.LoadFile(fileName);
                }
            }
            return null;
        }
    }
}
