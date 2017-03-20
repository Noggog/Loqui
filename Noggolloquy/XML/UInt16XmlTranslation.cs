using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UInt16XmlTranslation : TypicalXmlTranslation<ushort>
    {
        public readonly static UInt16XmlTranslation Instance = new UInt16XmlTranslation();

        protected override TryGet<ushort?> ParseNonNullString(string str)
        {
            if (ushort.TryParse(str, out ushort parsed))
            {
                return TryGet<ushort?>.Success(parsed);
            }
            return TryGet<ushort?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
