using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class UInt8Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(Byte);
        public override string RangeTypeName => nameof(RangeUInt8);
    }
}
