namespace TESVSnip.Domain.Data.RecordStructure
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    /// <summary>
    /// The subrecord element.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class SubrecordElement
    {
        /// <summary>
        /// The condid.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(0)] public int condid;

        /// <summary>
        /// The desc.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string desc = string.Empty;

        /// <summary>
        /// The flags.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string flags;

        [XmlIgnore] [DefaultValue(0)] public int group;

        /// <summary>
        /// The hexview.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(false)] public bool hexview;

        /// <summary>
        /// The hexview with decimal
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(false)] public bool hexviewwithdec;

        /// <summary>
        /// The multiline.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(false)] public bool multiline;

        /// <summary>
        /// The name.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] public string name;

        /// <summary>
        /// The notininfo.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(false)] public bool notininfo;

        /// <summary>
        /// The optional.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(0)] public int optional;

        /// <summary>
        /// The options.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] public string options;

        /// <summary>
        /// The refid.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string refid;

        /// <summary>
        /// The reftype.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string reftype;

        /// <summary>
        /// The repeat.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue(0)] public int repeat;

        [XmlIgnore] [DefaultValue(0)] public int size;

        /// <summary>
        /// The type.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] public string type;

        /// <summary>
        /// The funcr.
        /// For transform a value
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string funcr = string.Empty;

        /// <summary>
        /// The funcw.
        /// To write a transfromed value
        /// </summary>
        /// <remarks>
        /// </remarks>
        [XmlAttribute] [DefaultValue("")] public string funcw = string.Empty;

        public SubrecordElement()
        {
            this.hexview = false;
            this.hexviewwithdec = false;
            this.reftype = string.Empty;
            this.multiline = false;
            this.condid = 0;
            this.notininfo = false;
            this.repeat = 0;
            this.flags = string.Empty;
            this.optional = 0;
            this.refid = string.Empty;
            this.size = 0;
            this.funcr = string.Empty;
            this.funcw = string.Empty;
        }
    }
}