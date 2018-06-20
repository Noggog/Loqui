using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3FloatXmlTranslation : PrimitiveXmlTranslation<P3Float>
    {
        public readonly static P3FloatXmlTranslation Instance = new P3FloatXmlTranslation();

        protected override string GetItemStr(P3Float item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override bool ParseNonNullString(string str, out P3Float value, ErrorMaskBuilder errorMask)
        {
            if (P3Float.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {NullableName}"));
            return false;
        }
    }
}
