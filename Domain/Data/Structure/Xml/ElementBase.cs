using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace FalloutSnip.Domain.Data.Structure.Xml
{
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public abstract class ElementBase
    {
        /// <summary>
        ///     The desc.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string desc = string.Empty;

        /// <summary>
        ///     The name.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string name = string.Empty;

        /// <summary>
        ///     The optional.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(0)] public int optional;

        /// <summary>
        ///     The repeat.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(0)] public int repeat;

        /// <summary>
        ///     The notininfo.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(false)] public bool notininfo;
    }
}