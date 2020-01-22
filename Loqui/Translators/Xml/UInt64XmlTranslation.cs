using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class UInt64XmlTranslation : PrimitiveXmlTranslation<ulong>
    {
        public readonly static UInt64XmlTranslation Instance = new UInt64XmlTranslation();

        protected override bool Parse(string str, out ulong value, ErrorMaskBuilder? errorMask)
        {
            if (ulong.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
