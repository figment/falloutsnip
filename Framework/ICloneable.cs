namespace FalloutSnip.Framework
{
    using System;

    public interface ICloneable<out TClonedType> : ICloneable
    {
        new TClonedType Clone();
    }
}