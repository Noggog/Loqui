using Noggog;
using System;

namespace Loqui.Xml
{
    public class UInt16XmlTranslation : PrimitiveXmlTranslation<ushort>
    {
        public readonly static UInt16XmlTranslation Instance = new UInt16XmlTranslation();

        protected override ushort ParseNonNullString(string str)
        {
            if (ushort.TryParse(str, out ushort parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
