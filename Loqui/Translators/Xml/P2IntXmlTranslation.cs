using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class P2IntXmlTranslation : PrimitiveXmlTranslation<P2Int>
    {
        public readonly static P2IntXmlTranslation Instance = new P2IntXmlTranslation();

        protected override string GetItemStr(P2Int item)
        {
            return $"{item.X}, {item.Y}";
        }

        protected override bool Parse(string str, out P2Int value, ErrorMaskBuilder? errorMask)
        {
            if (P2Int.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
