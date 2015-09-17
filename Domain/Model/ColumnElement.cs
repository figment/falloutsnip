namespace FalloutSnip.Domain.Model
{
    using Data.Structure;

    public class ColumnElement : ColumnCriteria
    {
        public ColumnSubrecord Parent { get; set; }

        public ElementStructure Record { get; set; }
    }
}
