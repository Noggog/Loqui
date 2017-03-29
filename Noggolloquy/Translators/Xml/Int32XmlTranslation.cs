using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int32XmlTranslation : TypicalXmlTranslation<int>
    {
        public readonly static Int32XmlTranslation Instance = new Int32XmlTranslation();

        protected override int ParseNonNullString(string str)
        {
            if (int.TryParse(str, out int parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
