using System;

namespace Loqui.Generation
{
    public class Int16NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(Int16?);
    }
}
