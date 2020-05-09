using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class P2FloatXmlTranslation : PrimitiveXmlTranslation<P2Float>
    {
        public readonly static P2FloatXmlTranslation Instance = new P2FloatXmlTranslation();

        protected override string GetItemStr(P2Float item)
        {
            return $"{item.X.ToString("G9")}, {item.Y.ToString("G9")}";
        }

        protected override bool Parse(string str, out P2Float value, ErrorMaskBuilder? errorMask)
        {
            if (P2Float.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
