using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class PercentType : DoubleType
    {
        public override Type Type => typeof(Percent);
    }
}
