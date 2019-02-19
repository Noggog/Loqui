using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ColorType : PrimitiveType
    {
        public override Type Type => typeof(Color);

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            yield return "System.Drawing";
            yield return "Loqui.Presentation";
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"if (!{accessor.DirectAccess}.ColorOnlyEquals({rhsAccessor.DirectAccess})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)} && {accessor.DirectAccess}.ColorOnlyEquals({rhsAccessor.DirectAccess});");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => l.ColorOnlyEquals(r));");
                }
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor.DirectAccess}.ColorOnlyEquals({rhsAccessor.DirectAccess});");
            }
        }
    }
}
