using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class DateTimeXmlTranslation : TypicalXmlTranslation<DateTime>
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
    }
}
