using System;

namespace Loqui.Generation
{
    public class UInt16NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(UInt16?);
    }
}
