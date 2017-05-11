using System;

namespace Loqui.Generation
{
    public class DateTimeNullType : TypicalTypeGeneration
    {
        public override Type Type => typeof(DateTime?);
    }
}
