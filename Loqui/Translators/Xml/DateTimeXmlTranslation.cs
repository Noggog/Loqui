using Loqui.Internal;
using Noggog;
using System;
using System.Xml;

namespace Loqui.Xml
{
    public class DateTimeXmlTranslation : PrimitiveXmlTranslation<DateTime>
    {
        public readonly static DateTimeXmlTranslation Instance = new DateTimeXmlTranslation();

        protected override bool ParseNonNullString(string str, out DateTime value, ErrorMaskBuilder errorMask)
        {
            if (DateTime.TryParse(str, out value))
            {
                return true;
            }
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {NullableName}"));
            return false;
        }

        protected override string GetItemStr(DateTime item)
        {
            return item.ToString(@"MM/dd/yyyy HH:mm:ss.fffffff");
        }
    }
}
