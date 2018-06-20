using Loqui.Internal;
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

        protected override bool ParseNonNullString(string str, out float value, ErrorMaskBuilder errorMask)
        {
            if (float.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {NullableName}"));
            return false;
        }
    }
}
