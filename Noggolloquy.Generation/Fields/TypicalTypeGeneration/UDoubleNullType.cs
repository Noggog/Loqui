using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class UDoubleNullType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type => typeof(UDouble?);
    }
}
