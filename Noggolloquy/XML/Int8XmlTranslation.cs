using Noggog;
using System;

namespace Noggolloquy
{
    public class Int8XmlTranslation : TypicalXmlTranslation<sbyte>
    {
        public readonly static Int8XmlTranslation Instance = new Int8XmlTranslation();

        public override TryGet<sbyte?> ParseNonNullString(string str)
        {
            sbyte parsed;
            if (sbyte.TryParse(str, out parsed))
            {
                return TryGet<sbyte?>.Success(parsed);
            }
            return TryGet<sbyte?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
