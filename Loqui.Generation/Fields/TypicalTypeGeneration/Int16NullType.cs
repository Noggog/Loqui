using System;

namespace Loqui.Generation
{
    public class Int16NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(Int16?);
    }
}
