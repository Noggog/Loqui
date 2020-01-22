using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class Int32XmlTranslation : PrimitiveXmlTranslation<int>
    {
        public readonly static Int32XmlTranslation Instance = new Int32XmlTranslation();

        protected override bool ParseNonNullString(string str, out int value, ErrorMaskBuilder errorMask)
        {
            if (int.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
