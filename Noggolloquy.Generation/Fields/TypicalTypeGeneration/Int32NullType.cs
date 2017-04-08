using System;

namespace Noggolloquy.Generation
{
    public class Int32NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(Int32?);
    }
}
