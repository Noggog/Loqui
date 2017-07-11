using System;

namespace Loqui.Generation
{
    public class CharNullType : PrimitiveType
    {
        public override Type Type => typeof(char?);
    }
}
