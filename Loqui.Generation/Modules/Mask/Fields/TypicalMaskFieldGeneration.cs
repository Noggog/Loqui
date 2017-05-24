using System;

namespace Loqui.Generation
{
    public class TypicalMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            fg.AppendLine($"public {typeStr} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ex;");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = (Exception)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"public bool {field.Name};");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"if (!object.Equals(this.{field.Name}, t)) return false;");
        }
    }
}
