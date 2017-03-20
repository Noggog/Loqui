using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class Int32XmlTranslation : TypicalXmlTranslation<int>
    {
        public readonly static Int32XmlTranslation Instance = new Int32XmlTranslation();

        protected override TryGet<int?> ParseNonNullString(string str)
        {
            if (int.TryParse(str, out int parsed))
            {
                return TryGet<int?>.Success(parsed);
            }
            return TryGet<int?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
