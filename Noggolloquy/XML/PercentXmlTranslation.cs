using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class PercentXmlTranslation : TypicalXmlTranslation<Percent>
    {
        public readonly static PercentXmlTranslation Instance = new PercentXmlTranslation();

        protected override TryGet<Percent?> ParseNonNullString(string str)
        {
            if (Percent.TryParse(str, out Percent parsed))
            {
                return TryGet<Percent?>.Success(parsed);
            }
            return TryGet<Percent?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
