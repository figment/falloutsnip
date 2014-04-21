namespace TESVSnip.Domain.Data.Structure
{
    public struct Conditional
    {
        public readonly ElementValueType type;

        public readonly object value;

        public Conditional(ElementValueType type, object value)
        {
            this.type = type;
            this.value = value;
        }
    }
}