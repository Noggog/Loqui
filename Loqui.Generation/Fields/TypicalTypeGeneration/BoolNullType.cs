using System;

namespace Loqui.Generation
{
    public class BoolNullType : PrimitiveType
    {
        public override Type Type => typeof(bool?);
    }
}
