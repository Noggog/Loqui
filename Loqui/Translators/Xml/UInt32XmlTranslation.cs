using Noggog;
using System;

namespace Loqui.Xml
{
    public class UInt32XmlTranslation : PrimitiveXmlTranslation<uint>
    {
        public readonly static UInt32XmlTranslation Instance = new UInt32XmlTranslation();

        protected override uint ParseNonNullString(string str)
        {
            if (uint.TryParse(str, out uint parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
