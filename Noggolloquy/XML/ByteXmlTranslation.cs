using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class ByteXmlTranslation : TypicalXmlTranslation<byte>
    {
        public readonly static ByteXmlTranslation Instance = new ByteXmlTranslation();

        protected override TryGet<byte?> ParseNonNullString(string str)
        {
            if (byte.TryParse(str, out byte parsed))
            {
                return TryGet<byte?>.Success(parsed);
            }
            return TryGet<byte?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
