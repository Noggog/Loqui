using Noggog;
using System;

namespace Noggolloquy
{
    public class RangeDoubleXmlTranslation : TypicalXmlTranslation<RangeDouble>
    {
        public readonly static RangeDoubleXmlTranslation Instance = new RangeDoubleXmlTranslation();

        public override string GetItemStr(RangeDouble? item)
        {
            if (!item.HasValue) return null;
            return item.Value.Min + "-" + item.Value.Max;
        }

        public override TryGet<RangeDouble?> ParseNonNullString(string str)
        {
            RangeDouble parsed;
            if (RangeDouble.TryParse(str, out parsed))
            {
                return TryGet<RangeDouble?>.Success(parsed);
            }
            return TryGet<RangeDouble?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
