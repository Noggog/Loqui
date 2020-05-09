using Noggog;
using System;

namespace Loqui.Generation
{
    public class UDoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(UDouble);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})";
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})) return false;");
        }
    }
}
