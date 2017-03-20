using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UInt32XmlTranslation : TypicalXmlTranslation<uint>
    {
        public readonly static UInt32XmlTranslation Instance = new UInt32XmlTranslation();

        protected override TryGet<uint?> ParseNonNullString(string str)
        {
            if (uint.TryParse(str, out uint parsed))
            {
                return TryGet<uint?>.Success(parsed);
            }
            return TryGet<uint?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
