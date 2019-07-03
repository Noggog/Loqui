using Noggog;
using System;

namespace Loqui.Generation
{
    public class FloatNullType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(float?);
        public override string RangeTypeName(bool getter) => nameof(RangeFloat);
    }
}
