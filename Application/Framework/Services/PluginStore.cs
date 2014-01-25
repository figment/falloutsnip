using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TESVSnip.Domain.Services;

namespace TESVSnip.Framework.Services
{
    public static class PluginStore
    {
        static readonly List<PluginBase> PluginList = new List<PluginBase>();

        public static void Initialize()
        {
            RegisterPlugins();
        }

        public static List<PluginBase> Plugins
        {
            get { return PluginList; }
        }

        public static void RegisterPlugins()
        {
            
        }

        public static void AddPlugin(PluginBase plugin)
        {
            PluginList.Add(plugin);    
        }

        public static void AddPlugins(IEnumerable<PluginBase> plugins)
        {
            PluginList.AddRange(plugins);
        }

        public static void RemovePlugin(string name)
        {
            PluginList.RemoveAll(x => x.Name == name);
        }

        public static void Cleanup()
        {
            var plugins = PluginList.ToArray();
            PluginList.Clear();
            foreach (var plugin in plugins)
            {
                try { plugin.Dispose(); }
                catch{}
            }
        }
    }
}
