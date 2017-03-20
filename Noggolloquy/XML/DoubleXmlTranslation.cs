using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class DoubleXmlTranslation : TypicalXmlTranslation<double>
    {
        public readonly static DoubleXmlTranslation Instance = new DoubleXmlTranslation();

        protected override TryGet<double?> ParseNonNullString(string str)
        {
            if (double.TryParse(str, out double parsed))
            {
                return TryGet<double?>.Success(parsed);
            }
            return TryGet<double?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
