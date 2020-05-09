using Noggog;
using System;

namespace Loqui.Generation
{
    public class Int8NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(SByte?);
        public override string RangeTypeName(bool getter) => nameof(RangeInt8);
    }
}
