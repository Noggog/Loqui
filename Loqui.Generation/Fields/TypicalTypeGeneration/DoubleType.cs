using System;

namespace Loqui.Generation
{
    public class DoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type => typeof(double);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})";
        }
        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if ({GenerateEqualsSnippet(accessor, rhsAccessor, negate: true)}) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)} && {GenerateEqualsSnippet(accessor, rhsAccessor)};");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => {GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))});");
                }
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {GenerateEqualsSnippet(accessor, rhsAccessor)};");
            }
        }
    }
}
