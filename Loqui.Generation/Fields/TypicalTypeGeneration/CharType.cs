using System;

namespace Loqui.Generation
{
    public class CharType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(char);
    }
}
