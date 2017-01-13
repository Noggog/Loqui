using System;

namespace Noggolloquy.Generation
{
    public class Int16Type : TypicalWholeNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(Int16); }
        }
    }
}
