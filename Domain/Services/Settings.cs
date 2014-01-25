using System.Globalization;

namespace TESVSnip.Domain.Services
{
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;

    internal static class Settings
    {
        private static readonly string XmlPath = Path.Combine(Options.Value.SettingsDirectory, "settings.xml");

        private static XmlElement _rootNode;

        private static XmlDocument _xmlDoc;

        /// <summary>
        /// Constructor
        /// </summary>
        static Settings()
        {
            Init();
        }

        /// <summary>
        /// Retrieve a boolean value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>True or False</returns>
        public static bool GetBool(string name)
        {
            var xe = _rootNode.SelectSingleNode("descendant::boolValue[@name='" + name + "']") as XmlElement;
            if (xe == null) return false;

            return xe.InnerText == "true";
        }

        /// <summary>
        /// Retrieve a string value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>THe string</returns>
        public static string GetString(string name)
        {
            var xe = _rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
            if (xe == null) return null;

            return xe.InnerText;
        }

        /// <summary>
        /// Retrieve a string array
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>A string array</returns>
        public static string[] GetStringArray(string name)
        {
            var xe = _rootNode.SelectSingleNode("descendant::strArray[@name='" + name + "']") as XmlElement;
            if (xe == null) return null;

            var result = new string[xe.ChildNodes.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = xe.ChildNodes[i].InnerText;

            return result;
        }

        /// <summary>
        /// Retrieve the window position
        /// </summary>
        /// <param name="window">Window name</param>
        /// <param name="f">Windows form reference</param>
        public static void GetWindowPosition(string window, Form f)
        {
            var xe = _rootNode.SelectSingleNode("descendant::window[@name='" + window + "']") as XmlElement;
            if (xe == null) return;

            if (xe.Attributes.GetNamedItem("maximized").Value == "true")
            {
                f.WindowState = FormWindowState.Maximized;
            }
            else
            {
                f.Location = new Point(int.Parse(xe.Attributes.GetNamedItem("left").Value), int.Parse(xe.Attributes.GetNamedItem("top").Value));
                f.ClientSize = new Size(int.Parse(xe.Attributes.GetNamedItem("width").Value), int.Parse(xe.Attributes.GetNamedItem("height").Value));
                f.StartPosition = FormStartPosition.Manual;
            }
        }

        /// <summary>
        /// Init class Settings
        /// </summary>
        public static void Init()
        {
            _xmlDoc = new XmlDocument();
            if (File.Exists(XmlPath))
            {
                try
                {
                    _xmlDoc.Load(XmlPath);
                    _rootNode = (XmlElement)_xmlDoc.LastChild;
                }
                catch
                {
                    MessageBox.Show(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "Domain_Services_Settings_UnableLoadSettings"),
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "Global_Msg_TESVsnipError"));
                    _xmlDoc = new XmlDocument();
                    _xmlDoc.AppendChild(_rootNode = _xmlDoc.CreateElement("settings"));
                }
            }
            else
            {
                _xmlDoc.AppendChild(_rootNode = _xmlDoc.CreateElement("settings"));
            }
        }

        /// <summary>
        /// Remove string parameters
        /// </summary>
        /// <param name="name">Parameter name</param>
        public static void RemoveString(string name)
        {
            var xe = _rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
            if (xe != null)
                if (xe.ParentNode != null)
                    xe.ParentNode.RemoveChild(xe);
        }

        /// <summary>
        /// Set a boolean value in a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Boolean value</param>
        public static void SetBool(string name, bool value)
        {
            var xe = _rootNode.SelectSingleNode("descendant::boolValue[@name='" + name + "']") as XmlElement;
            if (xe == null)
            {
                _rootNode.AppendChild(xe = _xmlDoc.CreateElement("boolValue"));
                xe.Attributes.Append(_xmlDoc.CreateAttribute("name"));
                xe.Attributes[0].Value = name;
            }

            xe.InnerText = value ? "true" : "false";

            _xmlDoc.Save(XmlPath);
        }

        /// <summary>
        /// Set a string value in a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">String value</param>
        public static void SetString(string name, string value)
        {
            var xe = _rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
            if (xe == null)
            {
                _rootNode.AppendChild(xe = _xmlDoc.CreateElement("strValue"));
                xe.Attributes.Append(_xmlDoc.CreateAttribute("name"));
                xe.Attributes[0].Value = name;
            }

            xe.InnerText = value;

            _xmlDoc.Save(XmlPath);
        }

        /// <summary>
        /// Set a string array in a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="items">String array</param>
        public static void SetStringArray(string name, string[] items)
        {
            var xe = _rootNode.SelectSingleNode("descendant::strArray[@name='" + name + "']") as XmlElement;
            if (xe == null)
            {
                _rootNode.AppendChild(xe = _xmlDoc.CreateElement("strArray"));
                xe.Attributes.Append(_xmlDoc.CreateAttribute("name"));
                xe.Attributes[0].Value = name;
            }

            while (xe.ChildNodes.Count > 0)
            {
                xe.RemoveChild(xe.FirstChild);
            }

            foreach (string str in items)
            {
                XmlElement xe2 = _xmlDoc.CreateElement("element");
                xe2.InnerText = str;
                xe.AppendChild(xe2);
            }

            _xmlDoc.Save(XmlPath);
        }

        /// <summary>
        /// Set windows position on screen
        /// </summary>
        /// <param name="window">Windows name</param>
        /// <param name="f">Windows form reference</param>
        public static void SetWindowPosition(string window, Form f)
        {
            if (f.WindowState == FormWindowState.Minimized) return;

            Point location = f.Location;
            Size size = f.ClientSize;
            bool maximized = f.WindowState == FormWindowState.Maximized;
            var xe = _rootNode.SelectSingleNode("descendant::window[@name='" + window + "']") as XmlElement;
            if (xe == null)
            {
                _rootNode.AppendChild(xe = _xmlDoc.CreateElement("window"));
                xe.Attributes.Append(_xmlDoc.CreateAttribute("name"));
                xe.Attributes[0].Value = window;
            }

            XmlAttribute xa = _xmlDoc.CreateAttribute("left");
            xa.Value = location.X.ToString(CultureInfo.InvariantCulture);
            xe.Attributes.SetNamedItem(xa);
            xa = _xmlDoc.CreateAttribute("top");
            xa.Value = location.Y.ToString(CultureInfo.InvariantCulture);
            xe.Attributes.SetNamedItem(xa);
            xa = _xmlDoc.CreateAttribute("width");
            xa.Value = size.Width.ToString(CultureInfo.InvariantCulture);
            xe.Attributes.SetNamedItem(xa);
            xa = _xmlDoc.CreateAttribute("height");
            xa.Value = size.Height.ToString();
            xe.Attributes.SetNamedItem(xa);
            xa = _xmlDoc.CreateAttribute("maximized");
            xa.Value = maximized ? "true" : "false";
            xe.Attributes.SetNamedItem(xa);

            _xmlDoc.Save(XmlPath);
        }
    }
}