using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3UInt16XmlTranslation : PrimitiveXmlTranslation<P3UInt16>
    {
        public readonly static P3UInt16XmlTranslation Instance = new P3UInt16XmlTranslation();

        protected override string GetItemStr(P3UInt16 item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override P3UInt16 ParseNonNullString(string str)
        {
            if (P3UInt16.TryParse(str, out P3UInt16 parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
