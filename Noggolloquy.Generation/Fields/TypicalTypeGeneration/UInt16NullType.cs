using System;

namespace Noggolloquy.Generation
{
    public class UInt16NullType : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(UInt16?); }
        }
    }
}
