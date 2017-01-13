using System;

namespace Noggolloquy.Generation
{
    public class CharNullType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(char?); }
        }
    }
}
