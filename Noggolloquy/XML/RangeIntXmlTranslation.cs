using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class RangeIntXmlTranslation : TypicalXmlTranslation<RangeInt>
    {
        public readonly static RangeIntXmlTranslation Instance = new RangeIntXmlTranslation();

        protected override string GetItemStr(RangeInt? item)
        {
            if (!item.HasValue) return null;
            return item.Value.Min + "-" + item.Value.Max;
        }

        protected override TryGet<RangeInt?> ParseNonNullString(string str)
        {
            if (RangeInt.TryParse(str, out RangeInt parsed))
            {
                return TryGet<RangeInt?>.Success(parsed);
            }
            return TryGet<RangeInt?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
