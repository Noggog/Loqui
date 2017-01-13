using System;

namespace Noggolloquy.Generation
{
    public class UInt32Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(UInt32); }
        }
    }
}
