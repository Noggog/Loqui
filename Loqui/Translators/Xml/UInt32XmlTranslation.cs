using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class UInt32XmlTranslation : PrimitiveXmlTranslation<uint>
    {
        public readonly static UInt32XmlTranslation Instance = new UInt32XmlTranslation();

        protected override bool ParseNonNullString(string str, out uint value, ErrorMaskBuilder errorMask)
        {
            if (uint.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
