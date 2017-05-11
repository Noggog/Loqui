using Noggog;
using System;

namespace Loqui.Generation
{
    public class UInt8Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type => typeof(Byte);
        public override string RangeTypeName => nameof(RangeUInt8);
    }
}
