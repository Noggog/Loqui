using System;

namespace Noggolloquy.Generation
{
    public class UInt16Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(UInt16); }
        }
    }
}
