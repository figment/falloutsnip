using TESVSnip.Domain.Data.RecordStructure.Xml;

namespace TESVSnip.Domain.Data.RecordStructure
{
    using System;
    using System.Globalization;

    internal class ElementBase
    {
        public readonly string desc;

        public readonly string name;

        public readonly int optional;

        public readonly int repeat;

        protected ElementBase()
        {
            this.name = "DATA";
            this.desc = "Data";
            this.optional = 0;
            this.repeat = 0;            
        }
        protected ElementBase(ElementBase src, int optional, int repeat)
        {
            if (src.name.StartsWith("&#x"))
            {
                string[] val = src.name.Split(new[] { ';' }, 2, StringSplitOptions.None);
                var c = (char)int.Parse(val[0].Substring(3), NumberStyles.HexNumber, null);
                this.name = c + val[1];
            }
            else
            {
                this.name = src.name;
            }

            this.desc = src.desc;
            this.optional = optional;
            this.repeat = repeat;
        }

        protected ElementBase(Xml.SubrecordElement node)
        {
            if (node.name.StartsWith("&#x"))
            {
                string[] val = node.name.Split(new[] { ';' }, 2, StringSplitOptions.None);
                var c = (char)int.Parse(val[0].Substring(3), NumberStyles.HexNumber, null);
                this.name = c + val[1];
            }
            else
            {
                this.name = node.name;
            }

            this.repeat = node.repeat;
            this.optional = node.optional;
            this.desc = node.desc;
        }

        protected ElementBase(Xml.ElementGroup node)
        {
            if (node.name.StartsWith("&#x"))
            {
                string[] val = node.name.Split(new[] { ';' }, 2, StringSplitOptions.None);
                var c = (char)int.Parse(val[0].Substring(3), NumberStyles.HexNumber, null);
                this.name = c + val[1];
            }
            else
            {
                this.name = node.name;
            }

            this.repeat = node.repeat;
            this.optional = node.optional;
            this.desc = node.desc;
        }

        public virtual bool IsGroup
        {
            get
            {
                return false;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.desc) || this.name == this.desc)
            {
                return this.name;
            }

            return string.Format("{0}: {1}", this.name, this.desc);
        }
    }
}