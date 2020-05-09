using System;

namespace Loqui.Generation
{
    public class UInt32Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UInt32);
    }
}
