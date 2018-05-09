using Noggog;
using System;

namespace Loqui.Generation
{
    public class UDoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type => typeof(UDouble);

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})) return false;");
        }
    }
}
