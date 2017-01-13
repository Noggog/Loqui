using System;

namespace Noggolloquy.Generation
{
    public class UInt8Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Byte); }
        }
    }
}
