using Noggog;
using System;

namespace Noggolloquy
{
    public class UInt16XmlTranslation : TypicalXmlTranslation<ushort>
    {
        public readonly static UInt16XmlTranslation Instance = new UInt16XmlTranslation();

        public override TryGet<ushort?> ParseNonNullString(string str)
        {
            ushort parsed;
            if (ushort.TryParse(str, out parsed))
            {
                return TryGet<ushort?>.Success(parsed);
            }
            return TryGet<ushort?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
