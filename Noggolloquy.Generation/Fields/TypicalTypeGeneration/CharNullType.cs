using System;

namespace Noggolloquy.Generation
{
    public class CharNullType : TypicalTypeGeneration
    {
        public override Type Type => typeof(char?);
    }
}
