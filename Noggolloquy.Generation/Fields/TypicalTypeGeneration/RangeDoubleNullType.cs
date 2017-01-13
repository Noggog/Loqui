using System;

namespace Noggolloquy.Generation
{
    public class RangeDoubleNullType : RangeDoubleType
    {
        public override Type Type
        {
            get { return typeof(RangeDouble?); }
        }
    }
}
