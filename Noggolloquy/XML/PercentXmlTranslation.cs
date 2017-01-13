using Noggog;
using System;

namespace Noggolloquy
{
    public class PercentXmlTranslation : TypicalXmlTranslation<Percent>
    {
        public readonly static PercentXmlTranslation Instance = new PercentXmlTranslation();

        public override TryGet<Percent?> ParseNonNullString(string str)
        {
            Percent parsed;
            if (Percent.TryParse(str, out parsed))
            {
                return TryGet<Percent?>.Success(parsed);
            }
            return TryGet<Percent?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
