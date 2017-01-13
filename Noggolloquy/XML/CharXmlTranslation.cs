using Noggog;
using System;

namespace Noggolloquy
{
    public class CharXmlTranslation : TypicalXmlTranslation<char>
    {
        public readonly static CharXmlTranslation Instance = new CharXmlTranslation();

        public override TryGet<char?> ParseNonNullString(string str)
        {
            char parsed;
            if (char.TryParse(str, out parsed))
            {
                return TryGet<char?>.Success(parsed);
            }
            return TryGet<char?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
