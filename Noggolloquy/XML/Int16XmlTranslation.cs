using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int16XmlTranslation : TypicalXmlTranslation<short>
    {
        public readonly static Int16XmlTranslation Instance = new Int16XmlTranslation();

        protected override TryGet<short?> ParseNonNullString(string str)
        {
            if (short.TryParse(str, out short parsed))
            {
                return TryGet<short?>.Success(parsed);
            }
            return TryGet<short?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
