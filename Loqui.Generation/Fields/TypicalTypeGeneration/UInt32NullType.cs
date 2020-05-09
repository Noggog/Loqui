using System;

namespace Loqui.Generation
{
    public class UInt32NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UInt32?);
    }
}
