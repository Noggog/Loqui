using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3IntXmlTranslation : PrimitiveXmlTranslation<P3Int>
    {
        public readonly static P3IntXmlTranslation Instance = new P3IntXmlTranslation();

        protected override string GetItemStr(P3Int item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override bool Parse(string str, out P3Int value, ErrorMaskBuilder? errorMask)
        {
            if (P3Int.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
