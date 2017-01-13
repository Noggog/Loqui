using Noggog;
using System;

namespace Noggolloquy
{
    public class ByteXmlTranslation : TypicalXmlTranslation<byte>
    {
        public readonly static ByteXmlTranslation Instance = new ByteXmlTranslation();

        public override TryGet<byte?> ParseNonNullString(string str)
        {
            byte parsed;
            if (byte.TryParse(str, out parsed))
            {
                return TryGet<byte?>.Success(parsed);
            }
            return TryGet<byte?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
