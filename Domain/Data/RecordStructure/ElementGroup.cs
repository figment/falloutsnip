namespace TESVSnip.Domain.Data.RecordStructure
{
    internal class ElementGroup : ElementBase
    {
        public readonly ElementBase[] elements;

        public ElementGroup(Xml.ElementGroup node, ElementBase[] items)
            : base(node)
        {
            this.elements = items;
        }

        public override bool IsGroup
        {
            get
            {
                return true;
            }
        }
    }
}