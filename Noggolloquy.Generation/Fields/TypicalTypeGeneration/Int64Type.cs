using System;

namespace Noggolloquy.Generation
{
    public class Int64Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Int64); }
        }
    }
}
