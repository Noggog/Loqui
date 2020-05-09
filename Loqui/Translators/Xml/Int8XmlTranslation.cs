using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class Int8XmlTranslation : PrimitiveXmlTranslation<sbyte>
    {
        public readonly static Int8XmlTranslation Instance = new Int8XmlTranslation();
        
        protected override bool Parse(string str, out sbyte value, ErrorMaskBuilder? errorMask)
        {
            if (sbyte.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
