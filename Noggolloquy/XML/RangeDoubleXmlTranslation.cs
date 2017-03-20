using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class RangeDoubleXmlTranslation : TypicalXmlTranslation<RangeDouble>
    {
        public readonly static RangeDoubleXmlTranslation Instance = new RangeDoubleXmlTranslation();

        protected override string GetItemStr(RangeDouble? item)
        {
            if (!item.HasValue) return null;
            return item.Value.Min + "-" + item.Value.Max;
        }

        protected override TryGet<RangeDouble?> ParseNonNullString(string str)
        {
            if (RangeDouble.TryParse(str, out RangeDouble parsed))
            {
                return TryGet<RangeDouble?>.Success(parsed);
            }
            return TryGet<RangeDouble?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
