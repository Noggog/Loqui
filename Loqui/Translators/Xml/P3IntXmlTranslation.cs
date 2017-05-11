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

        protected override P3Int ParseNonNullString(string str)
        {
            if (P3Int.TryParse(str, out P3Int parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
