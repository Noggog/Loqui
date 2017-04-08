using System;

namespace Noggolloquy.Generation
{
    public abstract class TypicalTypeGeneration : TypicalGeneration
    {
        public abstract Type Type { get; }
        public override string TypeName => Type.GetName();
    }
}
