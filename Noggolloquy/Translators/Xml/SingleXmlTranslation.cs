using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class SingleXmlTranslation : TypicalXmlTranslation<float>
    {
        public readonly static SingleXmlTranslation Instance = new SingleXmlTranslation();

        protected override float ParseNonNullString(string str)
        {
            if (float.TryParse(str, out float parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {ElementName}");
        }
    }
}
