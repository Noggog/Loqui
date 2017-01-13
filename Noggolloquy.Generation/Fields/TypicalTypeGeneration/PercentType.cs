using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class PercentType : FloatType
    {
        public override Type Type
        {
            get
            {
                return typeof(Percent);
            }
        }
    }
}
