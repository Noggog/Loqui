using System;

namespace Noggolloquy.Generation
{
    public class Int32NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Int32?); }
        }
    }
}
