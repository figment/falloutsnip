namespace FalloutSnip.Domain.Model
{
    using System.Collections.Generic;

    public delegate string dFormIDLookupI(uint id);

    public delegate string dLStringLookup(uint id);

    public delegate Record dFormIDLookupR(uint id);

    public delegate string dFormIDLookupS(string id);

    public delegate string[] dFormIDScan(string type);

    public delegate Record[] dFormIDScanR(string type);

    public delegate IEnumerable<KeyValuePair<uint, Record>> dFormIDScanRec(string type);
}