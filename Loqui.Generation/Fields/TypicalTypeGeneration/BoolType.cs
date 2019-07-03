using System;

namespace Loqui.Generation
{
    public class BoolType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(bool);
    }
}
