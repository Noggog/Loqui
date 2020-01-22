using Loqui.Internal;
using Noggog;
using System;

namespace Loqui.Xml
{
    public class ByteXmlTranslation : PrimitiveXmlTranslation<byte>
    {
        public readonly static ByteXmlTranslation Instance = new ByteXmlTranslation();

        protected override bool ParseNonNullString(string str, out byte value, ErrorMaskBuilder errorMask)
        {
            if (byte.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            return false;
        }
    }
}
