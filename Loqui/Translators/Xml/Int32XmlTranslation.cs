using Noggog;
using System;

namespace Loqui.Xml
{
    public class Int32XmlTranslation : PrimitiveXmlTranslation<int>
    {
        public readonly static Int32XmlTranslation Instance = new Int32XmlTranslation();

        protected override int ParseNonNullString(string str)
        {
            if (int.TryParse(str, out int parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
