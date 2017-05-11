using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UDoubleXmlTranslation : PrimitiveXmlTranslation<UDouble>
    {
        public readonly static UDoubleXmlTranslation Instance = new UDoubleXmlTranslation();

        protected override string GetItemStr(UDouble item)
        {
            return item.Value.ToString("R");
        }

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
