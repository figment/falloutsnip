using System;
using TESVSnip;
namespace TESVSnip
{
    interface IRecord : ICloneable
    {
        string Name { get; set; }
        string DescriptiveName { get; }
        BaseRecord Parent { get; }
        long Size { get; }
        long Size2 { get; }
    }
}
