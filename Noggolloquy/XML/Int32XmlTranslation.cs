using Noggog;
using System;

namespace Noggolloquy
{
    public class Int32XmlTranslation : TypicalXmlTranslation<int>
    {
        public readonly static Int32XmlTranslation Instance = new Int32XmlTranslation();

        public override TryGet<int?> ParseNonNullString(string str)
        {
            int parsed;
            if (int.TryParse(str, out parsed))
            {
                return TryGet<int?>.Success(parsed);
            }
            return TryGet<int?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
