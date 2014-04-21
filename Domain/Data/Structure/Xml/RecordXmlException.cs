using System;

namespace TESVSnip.Domain.Data.Structure.Xml
{
    internal class RecordXmlException : Exception
    {
        public RecordXmlException(string msg)
            : base(msg)
        {
        }
    }
}