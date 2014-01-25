using TESVSnip.Domain.Data.RecordStructure.Xml;

namespace TESVSnip.Domain.Data.RecordStructure
{
    using System;

    internal class ElementStructure : ElementBase
    {
        public readonly int CondID;

        public readonly string FormIDType;

        public readonly string[] flags;

        public readonly int group;

        public readonly bool hexview;

        public readonly bool hexviewwithdec;

        public readonly bool multiline;

        public readonly bool notininfo;

        public readonly string[] options;

        public readonly string funcr;

        public readonly string funcw;

        public readonly ElementValueType type;

        public ElementStructure()
            : base()
        {
            this.@group = 0;
            this.hexview = true;
            this.hexviewwithdec = false;
            this.notininfo = true;
            this.options = null;
            this.flags = null;
            this.CondID = 0;
            this.FormIDType = null;
            this.multiline = false;
            this.funcr = string.Empty;
            this.funcw = string.Empty;
            this.type = ElementValueType.Blob;
        }
        public ElementStructure(ElementStructure src, int optional = 0, int repeat = 0)
            : base(src, optional, repeat)
        {
            this.CondID = src.CondID;
            this.FormIDType = src.FormIDType;
            this.flags = src.flags;
            this.group = src.group;
            this.hexview = src.hexview;
            this.hexviewwithdec = src.hexviewwithdec;
            this.multiline = src.multiline;
            //this.name = src.name;
            this.notininfo = src.notininfo;
            //this.optional = @optional;
            this.options = src.options;
            //this.repeat = repeat;
            this.funcr = src.funcr;
            this.funcw = src.funcw;
            this.type = src.type;
        }

        public ElementStructure(SubrecordElement node)
            : base(node)
        {
            //this.name = node.name;
            //this.desc = node.desc;
            this.@group = node.group;
            this.hexview = node.hexview;
            this.hexviewwithdec = node.hexviewwithdec;
            this.notininfo = node.notininfo;
            //this.optional = node.optional != 0;
            this.options = node.options == null ? new string[0] : node.options.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            this.flags = node.flags == null ? new string[0] : node.flags.Split(new[] { ';' });
            //this.repeat = node.repeat;
            this.funcr = node.funcr;
            this.funcw = node.funcw;
            this.CondID = node.condid;
            if (this.optional > 0 || this.repeat > 0)
            {
                if (this.@group != 0)
                {
                    throw new RecordXmlException("Elements with a group attribute cant be marked optional or repeat");
                }
            }

            this.FormIDType = null;
            this.multiline = node.multiline;
            this.type = (ElementValueType)Enum.Parse(typeof(ElementValueType), node.type, true);
            switch (this.type)
            {
                case ElementValueType.FormID:
                    this.FormIDType = node.reftype;
                    break;
                case ElementValueType.Blob:
                    if (this.repeat > 0 || this.optional > 0)
                    {
                        throw new RecordXmlException("blob type elements can't be marked with repeat or optional");
                    }

                    break;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.desc) || this.desc == this.name)
            {
                return this.name;
            }

            return string.Format("{0}: {1}", this.name, this.desc);
        }
    }
}