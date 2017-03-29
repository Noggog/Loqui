using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int64XmlTranslation : TypicalXmlTranslation<long>
    {
        public readonly static Int64XmlTranslation Instance = new Int64XmlTranslation();

        protected override long ParseNonNullString(string str)
        {
            if (long.TryParse(str, out long parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
