using System;

namespace Noggolloquy.Generation
{
    public class Int64NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Int64?); }
        }
    }
}
