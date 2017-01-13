using System;

namespace Noggolloquy.Generation
{
    public class BoolNullType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(bool?); }
        }
    }
}
