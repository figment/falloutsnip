using System;
using System.Collections;
using Microsoft.Scripting;

namespace TESVSnip.UI.Services
{
    public class PluginBase : IDisposable
    {
        public PluginBase()
        {
            SupportGlobal = true;
        }

        public PluginBase(string name)
            : this()
        {
            this.Name = name;
            this.DisplayName = name;
        }

        public PluginBase(string name, string displayname)
            : this(name)
        {
            this.DisplayName = displayname;
        }

        public PluginBase(string name, string displayname, bool supportSelection)
            : this(name, displayname)
        {
            this.SupportsSelection = supportSelection;
            this.SupportGlobal = !supportSelection;
        }
        public PluginBase(string name, string displayname, bool supportSelection, bool supportGlobal)
            : this(name, displayname)
        {
            this.SupportsSelection = supportSelection;
            this.SupportGlobal = supportGlobal;
        }

        public string Name { get; protected set; }

        public string DisplayName { get; protected set; }

        public string ToolTipText { get; protected set; }

        public System.Drawing.Image DisplayImage { get; protected set; }
        
        public bool SupportsSelection { get; protected set; }

        public bool SupportGlobal { get; protected set; }

        public virtual bool IsValidSelection(IList records)
        {
            return false;
        }

        public virtual void Execute(IList records)
        {
            throw new InvalidImplementationException("Not Implemented");
        }

        public void Dispose()
        {
            // Nothing to do 
        }
    }
}
