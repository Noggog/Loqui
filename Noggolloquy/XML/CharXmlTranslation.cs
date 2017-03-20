using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class CharXmlTranslation : TypicalXmlTranslation<char>
    {
        public readonly static CharXmlTranslation Instance = new CharXmlTranslation();

        protected override TryGet<char?> ParseNonNullString(string str)
        {
            if (char.TryParse(str, out char parsed))
            {
                return TryGet<char?>.Success(parsed);
            }
            return TryGet<char?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
