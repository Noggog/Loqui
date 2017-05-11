using Noggog;
using System;

namespace Loqui.Xml
{
    public class ByteXmlTranslation : PrimitiveXmlTranslation<byte>
    {
        public readonly static ByteXmlTranslation Instance = new ByteXmlTranslation();

        protected override byte ParseNonNullString(string str)
        {
            if (byte.TryParse(str, out byte parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
