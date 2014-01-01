using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TESVSnip.Domain.Data.RecordStructure;
using TESVSnip.Domain.Services;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace TESVSnip.Domain.Scripts
{
    public enum FunctionOperation
    {
        ForReading,
        ForWriting
    }

    /// <summary>
    /// CLASS PyElement
    /// </summary>
    public class PyElement
    {
        public string Name;
        public string Desc; //Description

        public int CondId;
        public string FormIdType;
        public string[] Flags;
        public int Group;
        public bool Multiline;
        public bool NotInInfo;
        public bool Optional;
        public string[] Options;
        public int Repeat;
        public string FuncRead;
        public string FuncWrite;
        public ElementValueType ValueType;
    }

    /// <summary>
    /// CLASS PyFunctionDefinition
    /// </summary>
    public class PyFunctionDefinition
    {
        public string Name;
        public string[] Parameters;

        public PyFunctionDefinition()
        {
            Name = string.Empty;
            Parameters = null;
        }
    }
        
    /// <summary>
    /// CLASS PyInterpreter
    /// </summary>
    internal static class PyInterpreter
    {
        private static readonly string ScriptsPyPath = Path.Combine(Options.Value.SettingsDirectory, @"scripts.py");
        private static System.Globalization.NumberFormatInfo _ni;
        private static Dictionary<string, PyFunctionDefinition> _pyDictionary;

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

        // The class object.
        private static Object _snipClass;

        // Instance of the SnipClass
        private static Object _snipClassObj;

        /// <summary>
        /// Init the py interpreter
        /// </summary>
        public static void InitPyInterpreter()
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            _ni = (System.Globalization.NumberFormatInfo) ci.NumberFormat.Clone();
            _ni.NumberDecimalSeparator = ".";

            _pyEngine = Python.CreateEngine(); // Create an engine to access IronPython
            _objOps = _pyEngine.CreateOperations(); // Performing tasks with the script
            _scope = _pyEngine.CreateScope(); // Default scope for executing the script
            _source = _pyEngine.CreateScriptSourceFromFile(ScriptsPyPath); // Load the script
            _compiledCode = _source.Compile();
            _compiledCode.Execute(_scope); // Create class object
            _snipClass = _scope.GetVariable("SnipClass"); // Retrieve the class object
            _snipClassObj = _objOps.Invoke(_snipClass); // Create an instance of the SnipClass

            _pyDictionary = new Dictionary<string, PyFunctionDefinition>();

            LoadAllPrototypeFunction();
        }

        /// <summary>
        /// Load prototype
        /// </summary>
        public static void LoadAllPrototypeFunction()
        {
            string line;

            bool startProto = false;

            // Read the file and display it line by line.
            var file = new StreamReader(ScriptsPyPath);
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace("\t", "").Trim();
                if (line.Length <= 0) continue;

                if (line == "#proto: START")
                {
                    startProto = true;
                    continue;
                }

                if (line == "#proto: END") break;

                if (startProto)
                {
                    if (line.Length <= 1) continue;

                    if (line.Substring(0, 1) == "#")
                    {
                        var def = new PyFunctionDefinition();
                        line = line.Substring(1, line.Length - 1);
                        string[] s1 = line.Split('(');
                        def.Name = s1[0];
                        line = s1[1].Substring(0, s1[1].Length - 1);
                        def.Parameters = line.Split(',');
                        for (int i = 0; i < def.Parameters.Length; i++)
                            def.Parameters[i] = def.Parameters[i].Trim().ToLower();
     
                        if (_pyDictionary.ContainsKey(def.Name))
                        {
                            string msg = "Function " + def.Name + " already defined in 'scripts.py' ! ";
                            MessageBox.Show(msg, @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                            _pyDictionary.Add(def.Name, def);
                    }
                }
            }

            file.Close();
            if (_pyDictionary.Count == 0)
                MessageBox.Show("@The 'scripts.py' file is empty !", @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Execute a script function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elemt"></param>
        /// <returns></returns>
        public static T ExecuteFunction<T>(Model.Element elemt, FunctionOperation funcOp) where T : struct
        {
            if (funcOp == FunctionOperation.ForReading)
                if (string.IsNullOrWhiteSpace(elemt.Structure.funcr))
                    return (T)Convert.ChangeType(elemt.Value, Type.GetTypeCode(typeof(T)));

            if (funcOp == FunctionOperation.ForWriting)
                if (string.IsNullOrWhiteSpace(elemt.Structure.funcw))
                    return (T)Convert.ChangeType(elemt.Value, Type.GetTypeCode(typeof(T)));

            switch (elemt.Type)
            {
                case ElementValueType.Float:
                case ElementValueType.Int:
                case ElementValueType.Short:
                case ElementValueType.UInt:
                case ElementValueType.UShort:
                    break;
                default:
                    return (T) Convert.ChangeType(elemt.Value, Type.GetTypeCode(typeof (T)));
            }

            // Parse function name ElementValueType
            string func = elemt.Structure.funcr;
            if (funcOp == FunctionOperation.ForWriting) func = elemt.Structure.funcw;
            string[] s1 = func.Split('(');
            string funcName = s1[0];

            PyFunctionDefinition pyFunc;
            if (!_pyDictionary.TryGetValue(funcName, out pyFunc))
            {
                string msg = "ExecuteReadingFunction: The '" + funcName + "' function doesn't exist !!!";
                MessageBox.Show(msg, "TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (T) Convert.ChangeType(elemt.Value, Type.GetTypeCode(typeof (T)));
            }

            object function = _objOps.GetMember(_snipClassObj, funcName); // get function

            // Parse parameters
            string p = s1[1].Replace("(", "").Replace(")", "").Trim();
            var param = new object[0];
            if (!string.IsNullOrWhiteSpace(p))
            {
                string[] parameters = p.Split(',');
                param = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    // management of the decimal separator
                    parameters[i] = parameters[i].Trim();
                    parameters[i] = parameters[i].Replace(".", _ni.CurrencyDecimalSeparator);
                    parameters[i] = parameters[i].Replace(",", _ni.CurrencyDecimalSeparator);
                }

                try
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        switch (pyFunc.Parameters[i + 3]) //+2 jump self, element and value parameter
                        {
                            case "float":
                                param[i] = float.Parse(parameters[i], _ni);
                                break;
                            case "int":
                                param[i] = int.Parse(parameters[i], _ni);
                                break;
                            case "short":
                                param[i] = short.Parse(parameters[i], _ni);
                                break;
                            case "uint":
                                param[i] = uint.Parse(parameters[i], _ni);
                                break;
                            case "ushort":
                                param[i] = ushort.Parse(parameters[i], _ni);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(@"ExecuteReadingFunction: {0}", ex.Message), @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            var result = elemt.Value;
            try
            {
                var el = new PyElement
                             {
                                 Name = elemt.Structure.name,
                                 Desc = elemt.Structure.desc,
                                 CondId = elemt.Structure.CondID,
                                 FormIdType = elemt.Structure.FormIDType,
                                 Flags = elemt.Structure.flags,
                                 Group = elemt.Structure.@group,
                                 Multiline = elemt.Structure.multiline,
                                 NotInInfo = elemt.Structure.notininfo,
                                 Optional = elemt.Structure.optional,
                                 Options = elemt.Structure.options,
                                 Repeat = elemt.Structure.repeat,
                                 FuncRead = elemt.Structure.funcr,
                                 FuncWrite = elemt.Structure.funcw,
                                 ValueType = elemt.Structure.type
                             };

                switch (elemt.Type)
                {
                    case ElementValueType.Float:
                        result = _objOps.Invoke(function, el, (float) elemt.Value, param);
                        break;
                    case ElementValueType.Int:
                        result = _objOps.Invoke(function, el, (int)elemt.Value, param);
                        break;
                    case ElementValueType.Short:
                        result = _objOps.Invoke(function, el, (short)elemt.Value, param);
                        break;
                    case ElementValueType.UInt:
                        result = _objOps.Invoke(function, el, (uint)elemt.Value, param);
                        break;
                    case ElementValueType.UShort:
                        result = _objOps.Invoke(function, el, (ushort)elemt.Value, param);
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ExecuteReadingFunction: {0}", ex.Message), @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return (T) Convert.ChangeType(result, Type.GetTypeCode(typeof (T)));
        }

        /// <summary>
        /// Execute a script function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elemt"></param>
        /// <param name="value"></param>
        /// <param name="funcOp"></param>
        /// <returns></returns>
        public static T ExecuteFunction<T>(ElementStructure elemt, object value, FunctionOperation funcOp) where T : struct
        {
            if (funcOp == FunctionOperation.ForReading)
                if (string.IsNullOrWhiteSpace(elemt.funcr))
                {
                    var changeType = Convert.ChangeType(value, Type.GetTypeCode(typeof (T)));
                    if (changeType != null) return (T) changeType;
                }

            if (funcOp == FunctionOperation.ForWriting)
                if (string.IsNullOrWhiteSpace(elemt.funcw))
                {
                    var type = Convert.ChangeType(value, Type.GetTypeCode(typeof (T)));
                    if (type != null) return (T)type;
                }

            switch (elemt.type)
            {
                case ElementValueType.Float:
                case ElementValueType.Int:
                case ElementValueType.Short:
                case ElementValueType.UInt:
                case ElementValueType.UShort:
                    break;
                default:
                    var changeType = Convert.ChangeType(value, Type.GetTypeCode(typeof (T)));
                    if (changeType != null) return (T)changeType;
                    break;
            }

            // Parse function name ElementValueType
            string func = elemt.funcr;
            if (funcOp == FunctionOperation.ForWriting) func = elemt.funcw;
            string[] s1 = func.Split('(');
            string funcName = s1[0];

            PyFunctionDefinition pyFunc;
            if (!_pyDictionary.TryGetValue(funcName, out pyFunc))
            {
                string msg = "ExecuteWritingFunction: The '" + funcName + "' function doesn't exist !!!";
                MessageBox.Show(msg, @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var changeType = Convert.ChangeType(value, Type.GetTypeCode(typeof (T)));
                if (changeType != null) return (T)changeType;
            }

            object function = _objOps.GetMember(_snipClassObj, funcName); // get function

            // Parse parameters
            string p = s1[1].Replace("(", "").Replace(")", "").Trim();
            var param = new object[0];
            if (!string.IsNullOrWhiteSpace(p))
            {
                string[] parameters = p.Split(',');
                param = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    // management of the decimal separator
                    parameters[i] = parameters[i].Trim();
                    parameters[i] = parameters[i].Replace(".", _ni.CurrencyDecimalSeparator);
                    parameters[i] = parameters[i].Replace(",", _ni.CurrencyDecimalSeparator);
                }

                try
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (pyFunc != null)
                            switch (pyFunc.Parameters[i + 3]) //+2 jump self, element and value parameter
                            {
                                case "float":
                                    param[i] = float.Parse(parameters[i], _ni);
                                    break;
                                case "int":
                                    param[i] = int.Parse(parameters[i], _ni);
                                    break;
                                case "short":
                                    param[i] = short.Parse(parameters[i], _ni);
                                    break;
                                case "uint":
                                    param[i] = uint.Parse(parameters[i], _ni);
                                    break;
                                case "ushort":
                                    param[i] = ushort.Parse(parameters[i], _ni);
                                    break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(@"ExecuteWritingFunction: {0}", ex.Message), @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            var result = value;
            try
            {
                var el = new PyElement
                {
                    Name = elemt.name,
                    Desc = elemt.desc,
                    CondId = elemt.CondID,
                    FormIdType = elemt.FormIDType,
                    Flags = elemt.flags,
                    Group = elemt.@group,
                    Multiline = elemt.multiline,
                    NotInInfo = elemt.notininfo,
                    Optional = elemt.optional,
                    Options = elemt.options,
                    Repeat = elemt.repeat,
                    FuncRead = elemt.funcr,
                    FuncWrite = elemt.funcw,
                    ValueType = elemt.type
                };

                switch (elemt.type)
                {
                    case ElementValueType.Float:
                        result = _objOps.Invoke(function, el, (float)value, param);
                        break;
                    case ElementValueType.Int:
                        result = _objOps.Invoke(function, el, (int)value, param);
                        break;
                    case ElementValueType.Short:
                        result = _objOps.Invoke(function, el, (short)value, param);
                        break;
                    case ElementValueType.UInt:
                        result = _objOps.Invoke(function, el, (uint)value, param);
                        break;
                    case ElementValueType.UShort:
                        result = _objOps.Invoke(function, el, (ushort)value, param);
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ExecuteWritingFunction: {0}", ex.Message), @"TESSnip Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return (T)Convert.ChangeType(result, Type.GetTypeCode(typeof(T)));
        }

    }
}