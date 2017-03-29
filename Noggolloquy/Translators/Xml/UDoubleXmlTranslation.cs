using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UDoubleXmlTranslation : TypicalXmlTranslation<UDouble>
    {
        public readonly static UDoubleXmlTranslation Instance = new UDoubleXmlTranslation();

        protected override UDouble ParseNonNullString(string str)
        {
            if (UDouble.TryParse(str, out UDouble parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
