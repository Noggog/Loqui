using System;

namespace Loqui.Generation
{
    public class CharNullType : TypicalTypeGeneration
    {
        public override Type Type => typeof(char?);
    }
}
