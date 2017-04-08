using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class PercentType : FloatType
    {
        public override Type Type => typeof(Percent);
    }
}
