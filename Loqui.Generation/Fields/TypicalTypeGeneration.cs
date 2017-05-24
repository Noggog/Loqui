using System;

namespace Loqui.Generation
{
    public abstract class TypicalTypeGeneration : PrimitiveGeneration
    {
        public abstract Type Type { get; }
        public override string TypeName => Type.GetName();
    }
}
