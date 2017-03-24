using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class ByteXmlTranslation : TypicalXmlTranslation<byte>
    {
        public readonly static ByteXmlTranslation Instance = new ByteXmlTranslation();

        protected override byte ParseNonNullString(string str)
        {
            if (byte.TryParse(str, out byte parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
