using System;

namespace Loqui.Generation
{
    public class TypicalMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"public {typeStr} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"this.{field.Name} = ex;");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"this.{field.Name} = ({GetErrorMaskTypeStr(field)}?)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"public bool {field.Name};");
        }

        public override void GenerateForCopyMaskCtor(FileGeneration fg, TypeGeneration field, string basicValueStr, string deepCopyStr)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"this.{field.Name} = {basicValueStr};");
        }

        public override void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"public bool {field.Name};");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"if (!eval({accessor.DirectAccess}{(indexed ? ".Value" : null)})) return false;");
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"{retAccessor} = eval({rhsAccessor}{(indexed ? ".Value" : null)});");
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"{retAccessor} = {accessor}.Combine({rhsAccessor});");
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
        {
             return $"{boolMaskAccessor}?.{field.Name} ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string typeStr, string valueStr)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"this.{field.Name} = {valueStr};");
        }

        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            return "Exception";
        }

        public override string GetTranslationMaskTypeStr(TypeGeneration field)
        {
            return "bool";
        }

        public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
        {
        }

        public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
        {
            return $"({field.Name}, null)";
        }

        public override void GenerateForTranslationMaskSet(FileGeneration fg, TypeGeneration field, Accessor accessor, string onAccessor)
        {
            fg.AppendLine($"{accessor.DirectAccess} = {onAccessor};");
        }
    }
}
