using System;
using System.Collections.Generic;
using System.Text;

namespace TESVSnip
{
    public enum LocalizedStringFormat
    {
        Base,
        DL,
        IL,
    }

    public class LocalizedStringDict : Collections.Generic.OrderedDictionary<uint, string>
    {

    }
}
