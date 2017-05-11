using Noggog;
using System;

namespace Loqui.Generation
{
    public class FloatNullType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type => typeof(float?);
        public override string RangeTypeName => nameof(RangeFloat);
    }
}
