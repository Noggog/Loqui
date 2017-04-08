using System;

namespace Noggolloquy.Generation
{
    public class UInt64NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(UInt64?);
    }
}
