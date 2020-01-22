using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class UInt16XmlTranslation : PrimitiveXmlTranslation<ushort>
    {
        public readonly static UInt16XmlTranslation Instance = new UInt16XmlTranslation();

        protected override bool ParseNonNullString(string str, out ushort value, ErrorMaskBuilder errorMask)
        {
            if (ushort.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
