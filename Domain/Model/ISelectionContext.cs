namespace FalloutSnip.Domain.Model
{
    using FalloutSnip.Framework;

    public interface ISelectionContext : ICloneable<ISelectionContext>
    {
        Rec Record { get; set; }

        SubRecord SubRecord { get; set; }

        void Reset();
    }
}