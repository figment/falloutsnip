using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace FalloutSnip.Domain.Data.Structure.Xml
{
    /// <summary>
    ///     The group.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ElementGroup : ElementBase
    {
        ///// <remarks/>
        // [System.Xml.Serialization.XmlElementAttribute("Subrecord")]
        // public List<Subrecord> Subrecords = new List<Subrecord>();
        ///// <remarks/>
        // [System.Xml.Serialization.XmlElementAttribute("Group")]
        // public List<Group> Groups = new List<Group>();
        [XmlElement("Group", typeof (ElementGroup))] [XmlElement("Element", typeof (SubrecordElement))] public
            List<ElementBase> Items = new List<ElementBase>();

        /// <summary>
        ///     The id.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string id = string.Empty;

        [XmlIgnore]
        public IEnumerable<ElementGroup> Groups
        {
            get { return this.Items.OfType<ElementGroup>(); }
        }

        [XmlIgnore]
        public IEnumerable<SubrecordElement> Elements
        {
            get { return this.Items.OfType<SubrecordElement>(); }
        }
    }
}