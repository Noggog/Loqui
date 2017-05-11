using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3DoubleXmlTranslation : PrimitiveXmlTranslation<P3Double>
    {
        public static readonly P3DoubleXmlTranslation Instance = new P3DoubleXmlTranslation();

        protected override string GetItemStr(P3Double item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override P3Double ParseNonNullString(string str)
        {
            if (P3Double.TryParse(str, out P3Double parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
