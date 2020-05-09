using System;

namespace Loqui.Generation
{
    public class DoubleNullType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(double?);
    }
}
