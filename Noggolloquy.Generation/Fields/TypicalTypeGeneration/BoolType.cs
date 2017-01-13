using System;

namespace Noggolloquy.Generation
{
    public class BoolType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}
