using System;

namespace Noggolloquy.Generation
{
    public class CharType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(char); }
        }
    }
}
