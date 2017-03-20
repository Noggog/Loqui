using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int8XmlTranslation : TypicalXmlTranslation<sbyte>
    {
        public readonly static Int8XmlTranslation Instance = new Int8XmlTranslation();

        protected override TryGet<sbyte?> ParseNonNullString(string str)
        {
            if (sbyte.TryParse(str, out sbyte parsed))
            {
                return TryGet<sbyte?>.Success(parsed);
            }
            return TryGet<sbyte?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
