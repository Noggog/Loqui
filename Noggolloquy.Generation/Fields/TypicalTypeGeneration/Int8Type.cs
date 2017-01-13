using System;

namespace Noggolloquy.Generation
{
    public class Int8Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(SByte); }
        }
    }
}
