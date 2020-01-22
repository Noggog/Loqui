using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3Int16XmlTranslation : PrimitiveXmlTranslation<P3Int16>
    {
        public readonly static P3Int16XmlTranslation Instance = new P3Int16XmlTranslation();

        protected override string GetItemStr(P3Int16 item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override bool ParseNonNullString(string str, out P3Int16 value, ErrorMaskBuilder errorMask)
        {
            if (P3Int16.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
