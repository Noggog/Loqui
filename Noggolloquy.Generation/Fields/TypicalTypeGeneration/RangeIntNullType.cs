using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class RangeIntNullType : RangeIntType
    {
        public override Type Type
        {
            get
            {
                return typeof(RangeInt?);
            }
        }
    }
}
