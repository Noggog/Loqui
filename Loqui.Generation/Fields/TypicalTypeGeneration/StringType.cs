using System;

namespace Loqui.Generation
{
    public class StringType : PrimitiveType
    {
        public override Type Type => typeof(string);

        protected override string GenerateDefaultValue() => $"\"{this.DefaultValue}\"";

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.HasBeenSet)
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)} && object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess});");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => object.Equals(l, r));");
                }
            }
            else
            {
                fg.AppendLine($"{retAccessor} = object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess});");
            }
        }
    }
}
