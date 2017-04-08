using System;

namespace Noggolloquy.Generation
{
    public class UInt8NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(Byte?);
    }
}
