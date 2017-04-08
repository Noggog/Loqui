using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class RangeDoubleNullType : RangeDoubleType
    {
        public override Type Type => typeof(RangeDouble?);
    }
}
