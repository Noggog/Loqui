using Noggog;
using System;

namespace Noggolloquy
{
    public class RangeIntXmlTranslation : TypicalXmlTranslation<RangeInt>
    {
        public readonly static RangeIntXmlTranslation Instance = new RangeIntXmlTranslation();

        public override string GetItemStr(RangeInt? item)
        {
            if (!item.HasValue) return null;
            return item.Value.Min + "-" + item.Value.Max;
        }

        public override TryGet<RangeInt?> ParseNonNullString(string str)
        {
            RangeInt parsed;
            if (RangeInt.TryParse(str, out parsed))
            {
                return TryGet<RangeInt?>.Success(parsed);
            }
            return TryGet<RangeInt?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
