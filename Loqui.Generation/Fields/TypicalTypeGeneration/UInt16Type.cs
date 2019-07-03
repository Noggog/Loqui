using System;

namespace Loqui.Generation
{
    public class UInt16Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UInt16);
    }
}
