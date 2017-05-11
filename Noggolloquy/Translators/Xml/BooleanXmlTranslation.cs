using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class BooleanXmlTranslation : PrimitiveXmlTranslation<bool>
    {
        public readonly static BooleanXmlTranslation Instance = new BooleanXmlTranslation();

        protected override bool ParseNonNullString(string str)
        {
            if (Boolean.TryParse(str, out bool parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
