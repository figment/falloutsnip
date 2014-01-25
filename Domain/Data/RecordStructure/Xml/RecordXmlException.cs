using System;

namespace TESVSnip.Domain.Data.RecordStructure.Xml
{
    internal class RecordXmlException : Exception
    {
        public RecordXmlException(string msg)
            : base(msg)
        {
        }
    }
}