using System;

namespace Noggolloquy.Generation
{
    public class FloatType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(float); }
        }
    }
}
