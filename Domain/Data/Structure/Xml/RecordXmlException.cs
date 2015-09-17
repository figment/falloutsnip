using System;

namespace FalloutSnip.Domain.Data.Structure.Xml
{
    internal class RecordXmlException : Exception
    {
        public RecordXmlException(string msg)
            : base(msg)
        {
        }
    }
}