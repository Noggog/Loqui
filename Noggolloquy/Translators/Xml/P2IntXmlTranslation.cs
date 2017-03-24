using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class P2IntXmlTranslation : TypicalXmlTranslation<P2Int>
    {
        public readonly static P2IntXmlTranslation Instance = new P2IntXmlTranslation();

        protected override string GetItemStr(P2Int? item)
        {
            if (!item.HasValue) return null;
            return $"{item.Value.X}, {item.Value.Y}";
        }

        protected override P2Int ParseNonNullString(string str)
        {
            if (P2Int.TryParse(str, out P2Int parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
