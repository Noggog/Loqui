using Noggog;
using System;

namespace Loqui.Generation
{
    public class UInt8NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(Byte?);
        public override string RangeTypeName(bool getter) => nameof(RangeUInt8);
    }
}
