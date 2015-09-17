#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace FalloutSnip.Framework.IO
{

    #region class IniFile

    /// <summary>
    ///     Summary description for INIFileInterop.
    /// </summary>
    public class IniFile
    {
        private static string defaultIniFile = "";

        #region Imports

        [DllImport("KERNEL32.DLL",
            EntryPoint = "GetPrivateProfileString")]
        protected internal static extern int
            GetPrivateProfileString(string lpAppName,
                                    string lpKeyName, string lpDefault,
                                    StringBuilder lpReturnedString, int nSize,
                                    string lpFileName);

        [DllImport("KERNEL32.DLL")]
        protected internal static extern int
            GetPrivateProfileInt(string lpAppName,
                                 string lpKeyName, int iDefault,
                                 string lpFileName);

        [DllImport("KERNEL32.DLL",
            EntryPoint = "WritePrivateProfileString")]
        protected internal static extern bool
            WritePrivateProfileString(string lpAppName,
                                      string lpKeyName, string lpString,
                                      string lpFileName);

        [DllImport("KERNEL32.DLL",
            EntryPoint = "GetPrivateProfileSection")]
        protected internal static extern int
            GetPrivateProfileSection(string lpAppName,
                                     byte[] lpReturnedString, int nSize,
                                     string lpFileName);

        [DllImport("KERNEL32.DLL",
            EntryPoint = "WritePrivateProfileSection")]
        protected internal static extern bool
            WritePrivateProfileSection(string lpAppName,
                                       byte[] data, string lpFileName);

        [DllImport("KERNEL32.DLL",
            EntryPoint = "GetPrivateProfileSectionNames")]
        protected internal static extern int
            GetPrivateProfileSectionNames(
            byte[] lpReturnedString,
            int nSize, string lpFileName);

        #endregion

        public static String DefaultIniFileName
        {
            get
            {
                if (string.IsNullOrEmpty(defaultIniFile))
                {
                    var asm = Assembly.GetEntryAssembly();
                    if (asm != null)
                    {
                        return defaultIniFile =
                               Path.ChangeExtension(
                                   Path.Combine(Path.GetDirectoryName(asm.Location),
                                                Path.GetFileNameWithoutExtension(asm.Location)), ".ini");
                    }
                }
                return defaultIniFile;
            }
            set { defaultIniFile = value; }
        }

        public static String GetValue(String section, String key)
        {
            return GetValue(DefaultIniFileName, section, key);
        }

        public static String GetValue(String filename, String section, String key)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            var buffer = new StringBuilder(256);
            const string sDefault = "";
            if (GetPrivateProfileString(section, key, sDefault, buffer, buffer.Capacity, filename) != 0)
            {
                return buffer.ToString();
            }
            return null;
        }

        public static String GetValue(String filename, String section, String key, String sDefault)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            if (sDefault == null)
                sDefault = "";

            var buffer = new StringBuilder(256);
            if (GetPrivateProfileString(section, key, sDefault, buffer, buffer.Capacity, filename) != 0)
            {
                return buffer.ToString();
            }
            return sDefault;
        }

        public static bool WriteValue(String section, String key, String sValue)
        {
            return WriteValue(DefaultIniFileName, section, key, sValue);
        }

        public static bool WriteValue(String filename, String section, String key, String sValue)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            return WritePrivateProfileString(section, key, sValue, filename);
        }

        public static int GetInt(String section, String key)
        {
            return GetInt(DefaultIniFileName, section, key);
        }

        public static int GetInt(String filename, String section, String key)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            const int iDefault = -1;
            return GetPrivateProfileInt(section, key,
                                        iDefault, filename);
        }

        public static List<string> GetSection(String section)
        {
            return GetSection(DefaultIniFileName, section);
        }

        public static List<string> GetSection(String filename, String section)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            var items = new List<string>();
            var buffer = new byte[32768];
            int bufLen = GetPrivateProfileSection(section, buffer, buffer.GetUpperBound(0), filename);
            if (bufLen > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char) buffer[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            items.Add(sb.ToString());
                            sb = new StringBuilder();
                        }
                    }
                }
            }
            return items;
        }


        public static Dictionary<string, string> GetPropertyValues(String section)
        {
            return GetPropertyValues(DefaultIniFileName, section);
        }

        public static Dictionary<string, string> GetPropertyValues(String filename, String section)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            var sc = GetSection(filename, section);
            var dict = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var s in sc)
            {
                int idx = s.IndexOf("=", StringComparison.Ordinal);
                if (idx >= 0)
                {
                    string lhs = s.Substring(0, idx).Trim();
                    string rhs = s.Substring(idx + 1).Trim();
                    dict[lhs] = rhs;
                }
                else
                {
                    dict[s.Trim()] = string.Empty;
                }
            }
            return dict;
        }

        public static bool WriteSection(string section, IEnumerable items)
        {
            return WriteSection(DefaultIniFileName, section, items);
        }

        public static bool WriteSection(string filename, string section, IEnumerable items)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            if (items == null)
                return WritePrivateProfileSection(section, null, filename);

            var b = new byte[32768];
            int j = 0;
            foreach (string s in items)
            {
                Encoding.ASCII.GetBytes(s, 0, s.Length, b, j);
                j += s.Length;
                b[j] = 0;
                j += 1;
            }
            b[j] = 0;
            return WritePrivateProfileSection(section, b, filename);
        }

        public static List<string> GetSectionNames()
        {
            return GetSectionNames(DefaultIniFileName);
        }

        public static List<string> GetSectionNames(String filename)
        {
            if (string.IsNullOrEmpty(filename))
                filename = DefaultIniFileName;

            var sections = new List<string>();
            var buffer = new byte[32768];
            int bufLen = GetPrivateProfileSectionNames(buffer,
                                                       buffer.GetUpperBound(0), filename);
            if (bufLen > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char) buffer[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sections.Add(sb.ToString());
                            sb = new StringBuilder();
                        }
                    }
                }
            }
            return sections;
        }
    }

    #endregion
}