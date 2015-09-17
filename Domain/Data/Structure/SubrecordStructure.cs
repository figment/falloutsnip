using System;
using System.Collections.Generic;
using System.Linq;
using FalloutSnip.Domain.Data.Structure.Xml;

namespace FalloutSnip.Domain.Data.Structure
{
    public class SubrecordStructure : SubrecordBase
    {
        public readonly int CondID;

        public readonly string CondOperand;

        public readonly CondType Condition;

        public readonly bool ContainsConditionals;

        public readonly bool UseHexEditor;

        public readonly ElementStructure[] elements;

        public readonly ElementBase[] elementTree;

        public readonly bool notininfo;

        public readonly int size;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SubrecordStructure" /> class.
        ///     Clone structure with optional and repeat values overridden.
        /// </summary>
        /// <param name="src">
        /// </param>
        /// <param name="optional">
        /// </param>
        /// <param name="repeat">
        /// </param>
        public SubrecordStructure(SubrecordStructure src, int optional, int repeat)
            : base(src, optional, repeat)
        {
            this.elements = src.elements;
            this.elementTree = src.elementTree;
            this.notininfo = src.notininfo;
            this.size = src.size;
            this.Condition = src.Condition;
            this.CondID = src.CondID;
            this.CondOperand = src.CondOperand;
            this.ContainsConditionals = src.ContainsConditionals;
            this.UseHexEditor = src.UseHexEditor;
        }

        public SubrecordStructure(Subrecord node, ElementBase[] elementTree, ElementStructure[] elements)
            : base(node)
        {
            this.notininfo = node.notininfo;
            this.size = node.size;
            this.Condition = (!string.IsNullOrEmpty(node.condition))
                                 ? (CondType) Enum.Parse(typeof (CondType), node.condition, true)
                                 : CondType.None;
            this.CondID = node.condid;
            this.CondOperand = node.condvalue;
            this.UseHexEditor = node.usehexeditor;

            this.elementTree = elementTree;
            this.elements = elements;

            this.ContainsConditionals = this.elements.Count(x => x.CondID != 0) > 0;
        }

        public SubrecordStructure(Xml.Subrecord node)
            : base(node)
        {
            this.notininfo = node.notininfo;
            this.size = node.size;
            this.Condition = (!string.IsNullOrEmpty(node.condition))
                                 ? (CondType) Enum.Parse(typeof (CondType), node.condition, true)
                                 : CondType.None;
            this.CondID = node.condid;
            this.CondOperand = node.condvalue;
            this.UseHexEditor = node.usehexeditor;

            // if (optional && repeat)
            // {
            // throw new RecordXmlException("repeat and optional must both have the same value if they are non zero");
            // }

            this.elementTree = GetElementTree(node.Items).ToArray();
            this.elements = GetElementArray(elementTree).ToArray();
            this.ContainsConditionals = this.elements.Count(x => x.CondID != 0) > 0;
        }


        /// <summary>
        ///     Build the Element array with groups expanded.
        /// </summary>
        /// <param name="list">
        ///     The list.
        /// </param>
        /// <returns>
        ///     The System.Collections.Generic.IEnumerable`1[T -&gt; FalloutSnip.ElementStructure].
        /// </returns>
        private static IEnumerable<ElementStructure> GetElementArray(IEnumerable<ElementBase> list)
        {
            foreach (var sr in list)
            {
                if (sr is ElementStructure)
                {
                    yield return (ElementStructure) sr;
                }
                else if (sr is ElementGroup)
                {
                    var sg = sr as ElementGroup;
                    var sss = GetElementArray(sg.elements).ToArray();
                    for (int index = 0; index < sss.Length; index++)
                    {
                        var ss = sss[index];
                        if (index != 0)
                            yield return ss;
                        else if (sg.repeat > 0) // first element is special
                        {
                            yield return new ElementStructure(ss, sss.Length, sss.Length); // replace
                        }
                        else if (sg.optional > 0)
                        {
                            yield return new ElementStructure(ss, sss.Length, 0); // optional
                        }
                    }
                }
            }
        }

        private static IEnumerable<ElementBase> GetElementTree(IEnumerable<Xml.ElementBase> items)
        {
            foreach (var sr in items)
            {
                if (sr is Xml.SubrecordElement)
                {
                    yield return new ElementStructure((Xml.SubrecordElement) sr);
                }
                else if (sr is Xml.ElementGroup)
                {
                    var g = sr as Xml.ElementGroup;
                    var ssr = GetElementTree(g.Items).ToArray();
                    if (ssr.Length > 0)
                        yield return new ElementGroup(g, ssr);
                }
            }
        }
    }
}