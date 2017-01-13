using System;

namespace Noggolloquy.Generation
{
    public class Int16NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Int16?); }
        }
    }
}
