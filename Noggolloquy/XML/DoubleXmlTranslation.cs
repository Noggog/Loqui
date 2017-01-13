using Noggog;
using System;

namespace Noggolloquy
{
    public class DoubleXmlTranslation : TypicalXmlTranslation<double>
    {
        public readonly static DoubleXmlTranslation Instance = new DoubleXmlTranslation();

        public override TryGet<double?> ParseNonNullString(string str)
        {
            double parsed;
            if (double.TryParse(str, out parsed))
            {
                return TryGet<double?>.Success(parsed);
            }
            return TryGet<double?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
