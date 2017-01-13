using Noggog;
using System;

namespace Noggolloquy
{
    public class UDoubleXmlTranslation : TypicalXmlTranslation<UDouble>
    {
        public readonly static UDoubleXmlTranslation Instance = new UDoubleXmlTranslation();

        public override TryGet<UDouble?> ParseNonNullString(string str)
        {
            UDouble parsed;
            if (UDouble.TryParse(str, out parsed))
            {
                return TryGet<UDouble?>.Success(parsed);
            }
            return TryGet<UDouble?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
