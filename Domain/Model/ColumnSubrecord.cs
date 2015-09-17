namespace FalloutSnip.Domain.Model
{
    using System.Collections.Generic;

    using Data.Structure;

    public class ColumnSubrecord : ColumnCriteria
    {
        public ICollection<ColumnElement> Children { get; set; }

        public SubrecordStructure Record { get; set; }
    }
}