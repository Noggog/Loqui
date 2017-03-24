using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class CharXmlTranslation : TypicalXmlTranslation<char>
    {
        public readonly static CharXmlTranslation Instance = new CharXmlTranslation();

        protected override char ParseNonNullString(string str)
        {
            if (char.TryParse(str, out char parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
