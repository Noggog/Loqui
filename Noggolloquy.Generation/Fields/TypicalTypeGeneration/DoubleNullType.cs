using System;

namespace Noggolloquy.Generation
{
    public class DoubleNullType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(double?); }
        }
    }
}
