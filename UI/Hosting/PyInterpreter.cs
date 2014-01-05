using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using TESVSnip.Domain.Scripts;
using TESVSnip.Domain.Services;

namespace TESVSnip.UI.Scripts
{
    /// <summary>
    /// CLASS PyInterpreter
    /// </summary>
    internal static class PyInterpreter
    {
        private static readonly string ScriptsPyPath = Path.Combine(Options.Value.SettingsDirectory, @"UI");

        // Engine to access IronPython
        private static ScriptEngine _pyEngine;

        // Performing tasks with the script
        private static ObjectOperations _objOps;

        // compililed source code
        private static CompiledCode _compiledCode;

        // Load the script
        private static ScriptSource _source;

        // Default scope for executing the script
        private static ScriptScope _scope;

        /// <summary>
        /// Init the py interpreter
        /// </summary>
        public static void InitPyInterpreter()
        {
            _pyEngine = Python.CreateEngine(); // Create an engine to access IronPython
            _objOps = _pyEngine.CreateOperations(); // Performing tasks with the script
            _scope = _pyEngine.CreateScope(); // Default scope for executing the script
            _source = _pyEngine.CreateScriptSourceFromFile(ScriptsPyPath); // Load the script
            _compiledCode = _source.Compile();
            _compiledCode.Execute(_scope); // Create class object
        }
    }
}
