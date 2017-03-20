using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class BooleanXmlTranslation : TypicalXmlTranslation<bool>
    {
        public readonly static BooleanXmlTranslation Instance = new BooleanXmlTranslation();

        protected override TryGet<bool?> ParseNonNullString(string str)
        {
            if (Boolean.TryParse(str, out bool parsed))
            {
                return TryGet<bool?>.Success(parsed);
            }
            return TryGet<bool?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
