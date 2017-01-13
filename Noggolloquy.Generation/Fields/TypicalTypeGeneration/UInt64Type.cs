using System;

namespace Noggolloquy.Generation
{
    public class UInt64Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(UInt64); }
        }
    }
}
