namespace TESVSnip.Domain.Model
{
    using System.Collections.Generic;

    using TESVSnip.Domain.Data.RecordStructure;

    public class ColumnSubrecord : ColumnCriteria
    {
        public ICollection<ColumnElement> Children { get; set; }

        public SubrecordStructure Record { get; set; }
    }
}