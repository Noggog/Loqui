using System;

namespace Noggolloquy.Generation
{
    public class DateTimeNullType : TypicalTypeGeneration
    {
        public override Type Type => typeof(DateTime?);
    }
}
