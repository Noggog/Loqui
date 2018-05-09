using Noggog;
using System;

namespace Loqui.Generation
{
    public class FloatType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type => typeof(float);
        public override string RangeTypeName => nameof(RangeFloat);

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})) return false;");
        }
    }
}
