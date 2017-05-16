using Noggog;
using System;
using System.Xml;

namespace Loqui.Xml
{
    public class DateTimeXmlTranslation : PrimitiveXmlTranslation<DateTime>
    {
        public readonly static DateTimeXmlTranslation Instance = new DateTimeXmlTranslation();

        protected override DateTime ParseNonNullString(string str)
        {
            if (DateTime.TryParse(str, out var parsed))
            {
                return parsed;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }

        protected override string GetItemStr(DateTime item)
        {
            return item.ToString(@"MM/dd/yyyy HH:mm:ss.fffffff");
        }
    }
}
