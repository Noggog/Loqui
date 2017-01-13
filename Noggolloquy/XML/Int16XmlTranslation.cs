using Noggog;
using System;

namespace Noggolloquy
{
    public class Int16XmlTranslation : TypicalXmlTranslation<short>
    {
        public readonly static Int16XmlTranslation Instance = new Int16XmlTranslation();

        public override TryGet<short?> ParseNonNullString(string str)
        {
            short parsed;
            if (short.TryParse(str, out parsed))
            {
                return TryGet<short?>.Success(parsed);
            }
            return TryGet<short?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
