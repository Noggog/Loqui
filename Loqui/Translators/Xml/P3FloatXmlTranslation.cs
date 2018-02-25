using Noggog;
using System;

namespace Loqui.Xml
{
    public class P3FloatXmlTranslation : PrimitiveXmlTranslation<P3Float>
    {
        public readonly static P3FloatXmlTranslation Instance = new P3FloatXmlTranslation();

        protected override string GetItemStr(P3Float item)
        {
            return $"{item.X}, {item.Y}, {item.Z}";
        }

        protected override P3Float ParseNonNullString(string str)
        {
            if (P3Float.TryParse(str, out P3Float parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
