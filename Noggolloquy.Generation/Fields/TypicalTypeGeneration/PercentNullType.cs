using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class PercentNullType : DoubleType
    {
        public override Type Type => typeof(Percent?);
    }
}
