using Noggog;
using System;

namespace Noggolloquy
{
    public class BooleanXmlTranslation : TypicalXmlTranslation<bool>
    {
        public readonly static BooleanXmlTranslation Instance = new BooleanXmlTranslation();

        public override TryGet<bool?> ParseNonNullString(string str)
        {
            bool parsed;
            if (Boolean.TryParse(str, out parsed))
            {
                return TryGet<bool?>.Success(parsed);
            }
            return TryGet<bool?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
