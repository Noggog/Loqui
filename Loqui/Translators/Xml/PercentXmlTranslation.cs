using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class PercentXmlTranslation : PrimitiveXmlTranslation<Percent>
    {
        public readonly static PercentXmlTranslation Instance = new PercentXmlTranslation();

        protected override bool ParseNonNullString(string str, out Percent value, ErrorMaskBuilder errorMask)
        {
            if (Percent.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
