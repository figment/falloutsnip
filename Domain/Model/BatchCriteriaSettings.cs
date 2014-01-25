namespace TESVSnip.Domain.Model
{
    using System.Collections.Generic;

    public class BatchCriteriaSettings
    {
        public IEnumerable<BatchCriteria> Items;

        public string Type;
    }
}