using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int64XmlTranslation : TypicalXmlTranslation<long>
    {
        public readonly static Int64XmlTranslation Instance = new Int64XmlTranslation();

        protected override TryGet<long?> ParseNonNullString(string str)
        {
            if (long.TryParse(str, out long parsed))
            {
                return TryGet<long?>.Success(parsed);
            }
            return TryGet<long?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
