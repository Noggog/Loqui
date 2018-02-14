using Noggog;
using System;

namespace Loqui.Xml
{
    public class P2FloatXmlTranslation : PrimitiveXmlTranslation<P2Float>
    {
        public readonly static P2FloatXmlTranslation Instance = new P2FloatXmlTranslation();

        protected override string GetItemStr(P2Float item)
        {
            return $"{item.X}, {item.Y}";
        }

        protected override P2Float ParseNonNullString(string str)
        {
            if (P2Float.TryParse(str, out P2Float parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
