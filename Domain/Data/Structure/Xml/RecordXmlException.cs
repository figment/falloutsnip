using System;

namespace FalloutSnip.Domain.Data.Structure.Xml
{
    public class RecordXmlException : Exception
    {
        public RecordXmlException(string msg)
            : base(msg)
        {
        }
    }
}