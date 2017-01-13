using System;

namespace Noggolloquy.Generation
{
    public class UInt64NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(UInt64?); }
        }
    }
}
