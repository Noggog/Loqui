using Noggog;
using System;

namespace Noggolloquy
{
    public class UInt32XmlTranslation : TypicalXmlTranslation<uint>
    {
        public readonly static UInt32XmlTranslation Instance = new UInt32XmlTranslation();

        public override TryGet<uint?> ParseNonNullString(string str)
        {
            uint parsed;
            if (uint.TryParse(str, out parsed))
            {
                return TryGet<uint?>.Success(parsed);
            }
            return TryGet<uint?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
