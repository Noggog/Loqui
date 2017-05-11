using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class UInt64XmlTranslation : PrimitiveXmlTranslation<ulong>
    {
        public readonly static UInt64XmlTranslation Instance = new UInt64XmlTranslation();

        protected override ulong ParseNonNullString(string str)
        {
            if (ulong.TryParse(str, out ulong parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
