using Noggog;
using System;

namespace Loqui.Xml
{
    public class PercentXmlTranslation : PrimitiveXmlTranslation<Percent>
    {
        public readonly static PercentXmlTranslation Instance = new PercentXmlTranslation();

        protected override Percent ParseNonNullString(string str)
        {
            if (Percent.TryParse(str, out Percent parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
