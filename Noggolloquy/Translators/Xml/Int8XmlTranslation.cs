using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int8XmlTranslation : TypicalXmlTranslation<sbyte>
    {
        public readonly static Int8XmlTranslation Instance = new Int8XmlTranslation();

        protected override sbyte ParseNonNullString(string str)
        {
            if (sbyte.TryParse(str, out sbyte parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
