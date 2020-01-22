using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class CharXmlTranslation : PrimitiveXmlTranslation<char>
    {
        public readonly static CharXmlTranslation Instance = new CharXmlTranslation();

        protected override bool ParseNonNullString(string str, out char value, ErrorMaskBuilder errorMask)
        {
            if (char.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
