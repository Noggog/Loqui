using System;

namespace Noggolloquy.Generation
{
    public class FloatNullType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type => typeof(float?);
    }
}
