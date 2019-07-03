using System;

namespace Loqui.Generation
{
    public class DateTimeType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(DateTime);
    }
}
