using FalloutSnip.Domain.Data.Structure.Xml;

namespace FalloutSnip.Domain.Data.Structure
{
    internal class SubrecordGroup : SubrecordBase
    {
        public readonly SubrecordBase[] elements;

        public SubrecordGroup(Group node, SubrecordBase[] items)
            : base(node)
        {
            this.elements = items;
        }

        public override bool IsGroup
        {
            get { return true; }
        }
    }
}