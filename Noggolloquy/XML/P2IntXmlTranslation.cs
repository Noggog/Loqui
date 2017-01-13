using Noggog;
using System;

namespace Noggolloquy
{
    public class P2IntXmlTranslation : TypicalXmlTranslation<P2Int>
    {
        public readonly static P2IntXmlTranslation Instance = new P2IntXmlTranslation();

        public override string GetItemStr(P2Int? item)
        {
            if (!item.HasValue) return null;
            return $"{item.Value.X}, {item.Value.Y}";
        }

        public override TryGet<P2Int?> ParseNonNullString(string str)
        {
            P2Int parsed;
            if (P2Int.TryParse(str, out parsed))
            {
                return TryGet<P2Int?>.Success(parsed);
            }
            return TryGet<P2Int?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
