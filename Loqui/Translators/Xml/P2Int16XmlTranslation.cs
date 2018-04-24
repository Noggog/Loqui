using Noggog;
using System;

namespace Loqui.Xml
{
    public class P2Int16XmlTranslation : PrimitiveXmlTranslation<P2Int16>
    {
        public readonly static P2Int16XmlTranslation Instance = new P2Int16XmlTranslation();

        protected override string GetItemStr(P2Int16 item)
        {
            return $"{item.X}, {item.Y}";
        }

        protected override P2Int16 ParseNonNullString(string str)
        {
            if (P2Int16.TryParse(str, out P2Int16 parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
