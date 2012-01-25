using TESVSnip.Collections.Generic;

namespace TESVSnip
{
    public enum LocalizedStringFormat
    {
        Base,
        DL,
        IL,
    }

    public class LocalizedStringDict : OrderedDictionary<uint, string>
    {
    }
}