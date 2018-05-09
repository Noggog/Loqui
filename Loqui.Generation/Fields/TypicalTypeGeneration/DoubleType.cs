using System;

namespace Loqui.Generation
{
    public class DoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type => typeof(double);

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})) return false;");
        }
    }
}
