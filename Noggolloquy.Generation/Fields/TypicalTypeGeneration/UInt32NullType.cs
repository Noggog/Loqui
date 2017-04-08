using System;

namespace Noggolloquy.Generation
{
    public class UInt32NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(UInt32?);
    }
}
