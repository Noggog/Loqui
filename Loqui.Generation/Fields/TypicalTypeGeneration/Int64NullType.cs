using System;

namespace Loqui.Generation
{
    public class Int64NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(Int64?);
    }
}
