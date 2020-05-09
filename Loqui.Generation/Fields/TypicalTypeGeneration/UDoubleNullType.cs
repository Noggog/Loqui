using Noggog;
using System;

namespace Loqui.Generation
{
    public class UDoubleNullType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UDouble?);
    }
}
