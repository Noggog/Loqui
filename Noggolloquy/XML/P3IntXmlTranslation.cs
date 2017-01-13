using Noggog;
using System;

namespace Noggolloquy
{
    public class P3IntXmlTranslation : TypicalXmlTranslation<P3Int>
    {
        public readonly static P3IntXmlTranslation Instance = new P3IntXmlTranslation();

        public override string GetItemStr(P3Int? item)
        {
            if (!item.HasValue) return null;
            return $"{item.Value.X}, {item.Value.Y}, {item.Value.Z}";
        }

        public override TryGet<P3Int?> ParseNonNullString(string str)
        {
            P3Int parsed;
            if (P3Int.TryParse(str, out parsed))
            {
                return TryGet<P3Int?>.Success(parsed);
            }
            return TryGet<P3Int?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
