using System;

namespace Loqui.Generation
{
    public class UInt64NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(UInt64?);
    }
}
