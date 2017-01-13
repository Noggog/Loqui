using Noggog;
using System;

namespace Noggolloquy
{
    public class Int64XmlTranslation : TypicalXmlTranslation<long>
    {
        public readonly static Int64XmlTranslation Instance = new Int64XmlTranslation();

        public override TryGet<long?> ParseNonNullString(string str)
        {
            long parsed;
            if (long.TryParse(str, out parsed))
            {
                return TryGet<long?>.Success(parsed);
            }
            return TryGet<long?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
