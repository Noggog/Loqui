using System;

namespace Loqui.Generation
{
    public class BoolNullType : TypicalTypeGeneration
    {
        public override Type Type => typeof(bool?);
    }
}
