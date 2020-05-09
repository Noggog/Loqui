using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class BooleanXmlTranslation : PrimitiveXmlTranslation<bool>
    {
        public readonly static BooleanXmlTranslation Instance = new BooleanXmlTranslation();

        protected override bool Parse(string str, out bool value, ErrorMaskBuilder? errorMask)
        {
            if (Boolean.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
