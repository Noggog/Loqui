using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class FloatType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type => typeof(float);
        public override string RangeTypeName => nameof(RangeFloat);
    }
}
