using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class Int64XmlTranslation : PrimitiveXmlTranslation<long>
    {
        public readonly static Int64XmlTranslation Instance = new Int64XmlTranslation();

        protected override bool ParseNonNullString(string str, out long value, ErrorMaskBuilder errorMask)
        {
            if (long.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
