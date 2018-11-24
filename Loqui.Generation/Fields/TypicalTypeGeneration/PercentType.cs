using Noggog;
using System;

namespace Loqui.Generation
{
    public class PercentType : DoubleType
    {
        public override Type Type => typeof(Percent);

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.Equals({rhsAccessor.DirectAccess})) return false;");
        }
    }
}
