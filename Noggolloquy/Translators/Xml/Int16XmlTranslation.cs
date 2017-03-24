using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int16XmlTranslation : TypicalXmlTranslation<short>
    {
        public readonly static Int16XmlTranslation Instance = new Int16XmlTranslation();

        protected override short ParseNonNullString(string str)
        {
            if (short.TryParse(str, out short parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
