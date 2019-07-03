using System;

namespace Loqui.Generation
{
    public class UInt64Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UInt64);
    }
}
