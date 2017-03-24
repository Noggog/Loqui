using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class DoubleXmlTranslation : TypicalXmlTranslation<double>
    {
        public readonly static DoubleXmlTranslation Instance = new DoubleXmlTranslation();

        protected override double ParseNonNullString(string str)
        {
            if (double.TryParse(str, out double parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
