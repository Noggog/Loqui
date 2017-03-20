using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UInt64XmlTranslation : TypicalXmlTranslation<ulong>
    {
        public readonly static UInt64XmlTranslation Instance = new UInt64XmlTranslation();

        protected override TryGet<ulong?> ParseNonNullString(string str)
        {
            if (ulong.TryParse(str, out ulong parsed))
            {
                return TryGet<ulong?>.Success(parsed);
            }
            return TryGet<ulong?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
