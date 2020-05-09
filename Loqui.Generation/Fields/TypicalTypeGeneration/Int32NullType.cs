using System;

namespace Loqui.Generation
{
    public class Int32NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(Int32?);
    }
}
