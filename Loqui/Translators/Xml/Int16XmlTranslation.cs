using Loqui.Internal;
using Noggog;
using System;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class Int16XmlTranslation : PrimitiveXmlTranslation<short>
    {
        public readonly static Int16XmlTranslation Instance = new Int16XmlTranslation();

        protected override bool ParseNonNullString(string str, out short value, ErrorMaskBuilder errorMask)
        {
            if (short.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
