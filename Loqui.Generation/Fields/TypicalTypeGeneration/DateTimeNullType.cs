using System;

namespace Loqui.Generation
{
    public class DateTimeNullType : PrimitiveType
    {
        public override Type Type => typeof(DateTime?);
    }
}
