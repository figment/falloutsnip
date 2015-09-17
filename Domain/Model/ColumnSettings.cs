namespace FalloutSnip.Domain.Model
{
    using System.Collections.Generic;

    public class ColumnSettings
    {
        public IEnumerable<ColumnCriteria> Items;

        public string Type;
    }
}