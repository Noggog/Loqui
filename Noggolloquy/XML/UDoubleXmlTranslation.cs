using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UDoubleXmlTranslation : TypicalXmlTranslation<UDouble>
    {
        public readonly static UDoubleXmlTranslation Instance = new UDoubleXmlTranslation();

        protected override TryGet<UDouble?> ParseNonNullString(string str)
        {
            if (UDouble.TryParse(str, out UDouble parsed))
            {
                return TryGet<UDouble?>.Success(parsed);
            }
            return TryGet<UDouble?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
