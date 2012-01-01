using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace TESVSnip {

    class RecordXmlException : Exception { public RecordXmlException(string msg) : base(msg) { } }
    enum ElementValueType {
        String, Float, Int, Short, Byte, FormID, fstring, Blob, LString, BString, UShort, UInt, SByte, Str4
    }
    enum CondType {
        None, Equal, Not, Greater, Less, GreaterEqual, LessEqual, StartsWith, EndsWith, Contains, Exists, Missing
    }

    class SubrecordBase
    {
        protected SubrecordBase(SubrecordBase src, int optional, int repeat)
        {
            this.name = src.name;
            this.desc = src.desc;
            this.optional = optional;
            this.repeat = repeat;
        }

        protected SubrecordBase(TESVSnip.Data.Subrecord node)
        {
            this.name = node.name;
            this.repeat = node.repeat;
            this.optional = node.optional;
            this.desc = node.desc;
        }
        protected SubrecordBase(TESVSnip.Data.Group node)
        {
            this.name = node.name;
            this.repeat = node.repeat;
            this.optional = node.optional;
            this.desc = node.desc;
        }

        public readonly string name;
        public readonly string desc;
        public readonly int repeat;
        public readonly int optional;

        public virtual bool IsGroup { get { return false; } }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(desc) ||  this.name == this.desc)
                return this.name;
            return string.Format("{0}: {1}", this.name, this.desc);
        }
    }

    class SubrecordGroup : SubrecordBase
    {
        public SubrecordGroup(TESVSnip.Data.Group node, SubrecordBase[] items) : base(node) { elements = items; }
        public override bool IsGroup { get { return true; } }
        public readonly SubrecordBase[] elements;
    }

    class SubrecordStructure : SubrecordBase
    {
        public readonly ElementStructure[] elements;
        public readonly bool notininfo;
        public readonly int size;

        public readonly CondType Condition;
        public readonly int CondID;
        public readonly string CondOperand;
        public readonly bool ContainsConditionals;
        public readonly bool UseHexEditor;

        /// <summary>
        /// Clone structure with optional and repeat values overridden
        /// </summary>
        /// <param name="src"></param>
        /// <param name="optional"></param>
        /// <param name="repeat"></param>
        public SubrecordStructure(SubrecordStructure src, int optional, int repeat) : base(src, optional, repeat)
        {
            this.elements = src.elements;
            this.notininfo = src.notininfo;
            this.size = src.size;
            this.Condition = src.Condition;
            this.CondID = src.CondID;
            this.CondOperand = src.CondOperand;
            this.ContainsConditionals = src.ContainsConditionals;
            this.UseHexEditor = src.UseHexEditor;
        }
        public SubrecordStructure(TESVSnip.Data.Subrecord node) : base(node)
        {
            this.notininfo = node.notininfo;
            this.size = node.size;
            this.Condition = (!string.IsNullOrEmpty(node.condition)) ? (CondType)Enum.Parse(typeof(CondType), node.condition, true) : CondType.None;
            this.CondID = node.condid;
            this.CondOperand = node.condvalue;
            this.UseHexEditor = node.usehexeditor;
            //if (optional && repeat)
            //{
            //    throw new RecordXmlException("repeat and optional must both have the same value if they are non zero");
            //}

            var elements = new List<ElementStructure>();
            foreach (var elem in node.Elements)
                elements.Add(new ElementStructure(elem));
            this.elements = elements.ToArray();

            ContainsConditionals = this.elements.Count(x => x.CondID != 0) > 0;
        }
    }

    class ElementStructure 
    {
        public readonly string name;
        public readonly string desc;
        public readonly int group;
        public readonly ElementValueType type;
        public readonly string FormIDType;
        public readonly string[] options;
        public readonly int CondID;
        public readonly bool notininfo;
        public readonly bool multiline;
        public readonly int repeat;
        public readonly bool optional;
        public readonly bool hexview;
        public readonly string[] flags;

        public ElementStructure(TESVSnip.Data.SubrecordElement node)
        {
            this.name = node.name;
            this.desc = node.desc;
            this.group = node.group;
            this.hexview = node.hexview;
            this.notininfo = node.notininfo;
            this.optional = node.optional != 0;
            this.options = node.options == null ? new string[0] : node.options.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            this.flags = node.flags == null ? new string[0] : node.flags.Split(new char[] { ';' });
            this.repeat = node.repeat;
            this.CondID = node.condid;
            if (optional || repeat > 0)
            {
                if (group != 0) throw new RecordXmlException("Elements with a group attribute cant be marked optional or repeat");
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
                    if (repeat > 0 || optional) throw new RecordXmlException("blob or fstring type elements can't be marked with repeat or optional");
                    break;
                case ElementValueType.fstring:
                    if (repeat > 0 || optional) throw new RecordXmlException("blob or fstring type elements can't be marked with repeat or optional");
                    break;
            }

        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(desc) ||  this.desc == this.name)
                return this.name;
            return string.Format("{0}: {1}", this.name, this.desc);
        }
    }

    class RecordStructure {
        #region Static
        private static bool loaded;
        public static bool Loaded { get { return loaded; } }

        public static Dictionary<string, RecordStructure> Records = new Dictionary<string, RecordStructure>(StringComparer.InvariantCultureIgnoreCase);
        private static string xmlPath=System.IO.Path.Combine(Program.settingsDir, @"RecordStructure.xml");
        
        private RecordStructure(TESVSnip.Data.RecordsRecord rec, SubrecordBase[] subrecordTree, SubrecordStructure[] subrecords)
        {
            this.name = rec.name;
            this.description = rec.desc;
            this.subrecordTree = subrecordTree;
            this.subrecords = subrecords;
        }

        private static List<SubrecordBase> GetSubrecordStructures(System.Collections.ICollection items, Dictionary<string, TESVSnip.Data.Group> dict)
        {
            var subrecords = new List<SubrecordBase>();
            foreach (var sr in items)
            {
                if (sr is TESVSnip.Data.Subrecord)
                {
                    subrecords.Add(new SubrecordStructure((TESVSnip.Data.Subrecord)sr));
                }
                else if (sr is TESVSnip.Data.Group)
                {
                    var g = sr as TESVSnip.Data.Group;
                    var ssr = GetSubrecordStructures((g.Items.Count > 0) ? g.Items : dict[g.id].Items, dict);
                    if (ssr.Count > 0)
                    {
                        //if (!ssr[0].IsGroup && (ssr[0].optional || ssr[0].repeat))
                        //{
                        //    throw new RecordXmlException("repeat and optional cannot be specified on first subrecord of a group");
                        //}
                        subrecords.Add(new SubrecordGroup(g, ssr.ToArray()));
                    }
                }
            }
            return subrecords;
        }
        /// <summary>
        /// Build the Subrecord array with groups expanded
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private static List<SubrecordStructure> BuildSubrecordStructure(IEnumerable<SubrecordBase> list)
        {
            List<SubrecordStructure> subrecords = new List<SubrecordStructure>();
            foreach (var sr in list)
            {
                if (sr is SubrecordStructure)
                {
                    subrecords.Add((SubrecordStructure)sr);
                }
                else if (sr is SubrecordGroup)
                {
                    var sg = sr as SubrecordGroup;
                    List<SubrecordStructure> sss = BuildSubrecordStructure(sg.elements);
                    if (sss.Count > 0)
                    {
                        if (sg.repeat > 0)
                            sss[0] = new SubrecordStructure(sss[0], sss.Count, sss.Count); // replace
                        else if (sg.optional > 0)
                            sss[0] = new SubrecordStructure(sss[0], sss.Count, 0); // optional
                    }
                    subrecords.AddRange(sss);
                }
            }
            return subrecords;
        }

        public static void Load() 
        {
            if(loaded) {
                Records.Clear();
            } else loaded=true;


            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(TESVSnip.Data.Records));
            using (System.IO.FileStream fs = System.IO.File.OpenRead(xmlPath))
            {
                var baseRec = xs.Deserialize(fs) as TESVSnip.Data.Records;
                var groups = baseRec.Items.OfType<TESVSnip.Data.Group>().ToDictionary(x => x.id, StringComparer.InvariantCultureIgnoreCase);
                foreach (var rec in baseRec.Items.OfType<TESVSnip.Data.RecordsRecord>())
                {
                    List<SubrecordBase> subrecords = GetSubrecordStructures(rec.Items, groups);
                    var sss = BuildSubrecordStructure(subrecords);
                    Records[rec.name] = new RecordStructure(rec, subrecords.ToArray(), sss.ToArray());
                }
            }
        }
        #endregion

        //public readonly SubrecordBase[] subrecords;
        public readonly SubrecordBase[] subrecordTree;
        public readonly SubrecordStructure[] subrecords;
        public readonly string description;
        public readonly string name;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(description) && this.description != this.name)
                return this.name;
            return string.Format("{0}: {1}", this.name, this.description);
        }

    }

}
