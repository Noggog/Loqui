using Noggog;
using System;

namespace Loqui.Xml
{
    public class FloatXmlTranslation : PrimitiveXmlTranslation<float>
    {
        public readonly static FloatXmlTranslation Instance = new FloatXmlTranslation();
        public override string NullableName => "FloatN";
        public override string ElementName => "Float";

        protected override string GetItemStr(float item)
        {
            return item.ToString("G9");
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
