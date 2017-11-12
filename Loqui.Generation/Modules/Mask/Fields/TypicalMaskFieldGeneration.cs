using System;

namespace Loqui.Generation
{
    public class TypicalMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            if (!field.GenerateTypicalItems) return;
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
            if (!field.GenerateTypicalItems) return;
            fg.AppendLine($"public bool {field.Name};");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            if (!field.GenerateTypicalItems) return;
            fg.AppendLine($"if (!eval(this.{field.Name})) return false;");
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor)
        {
            if (!field.GenerateTypicalItems) return;
            fg.AppendLine($"{retAccessor} = eval({rhsAccessor});");
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            fg.AppendLine($"{retAccessor} = {accessor}.Combine({rhsAccessor});");
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor)
        {
             return $"{maskAccessor}?.{field.Name} ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            if (!field.GenerateTypicalItems) return;
            fg.AppendLine($"this.{field.Name} = {valueStr};");
        }

        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            return "Exception";
        }

        public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
        {
        }
    }
}
