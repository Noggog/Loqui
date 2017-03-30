using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class FloatXmlTranslation : TypicalXmlTranslation<float>
    {
        public readonly static FloatXmlTranslation Instance = new FloatXmlTranslation();
        public override string NullableName => "FloatN";
        public override string ElementName => "Float";

        protected override string GetItemStr(float item)
        {
            return item.ToString("G9");
            /*
              		
            a	-3.40282347E+38	float
		    b	-3.402823E+38	float

             */

        }

        protected override float ParseNonNullString(string str)
        {
            if (float.TryParse(str, out float parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }
    }
}
