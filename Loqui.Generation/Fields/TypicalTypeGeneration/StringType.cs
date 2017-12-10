using System;

namespace Loqui.Generation
{
    public class StringType : PrimitiveType
    {
        public override Type Type => typeof(string);

        protected override string GenerateDefaultValue() => $"\"{this.DefaultValue}\"";

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!object.Equals({this.Name}, {rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => object.Equals(l, r));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess});");
            }
        }
    }
}
