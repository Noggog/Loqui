using System;

namespace Loqui.Generation
{
    public abstract class TypicalTypeGeneration : PrimitiveType
    {
        public abstract Type Type { get; }
        public override string TypeName => Type.GetName();
    }
}
