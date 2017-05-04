using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class Int8Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(SByte);
        public override string RangeTypeName => nameof(RangeInt8);
    }
}
