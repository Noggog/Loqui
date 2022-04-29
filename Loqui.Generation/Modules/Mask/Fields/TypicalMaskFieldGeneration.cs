namespace Loqui.Generation;

public class TypicalMaskFieldGeneration : MaskModuleField
{
    public override void GenerateForField(StructuredStringBuilder sb, TypeGeneration field, string typeStr)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"public {GetMaskTypeStr(field, typeStr)} {field.Name};");
    }

    public override void GenerateSetException(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"this.{field.Name} = ex;");
    }

    public override void GenerateSetMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"this.{field.Name} = ({GetErrorMaskTypeStr(field)}?)obj;");
    }

    public override void GenerateForCopyMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"public bool {field.Name};");
    }

    public override void GenerateForCopyMaskCtor(StructuredStringBuilder sb, TypeGeneration field, string basicValueStr, string deepCopyStr)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"this.{field.Name} = {basicValueStr};");
    }

    public override void GenerateForTranslationMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"public bool {field.Name};");
    }

    public override void GenerateForAll(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"if (!eval({accessor.Access}{(indexed ? ".Value" : null)})) return false;");
    }

    public override void GenerateForAny(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"if (eval({accessor.Access}{(indexed ? ".Value" : null)})) return true;");
    }

    public override void GenerateForTranslate(StructuredStringBuilder sb, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"{retAccessor} = eval({rhsAccessor}{(indexed ? ".Value" : null)});");
    }

    public override void GenerateForErrorMaskCombine(StructuredStringBuilder sb, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"{retAccessor} = {accessor}.Combine({rhsAccessor});");
    }

    public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
    {
        return $"{boolMaskAccessor}?.{field.Name} ?? true";
    }

    public override void GenerateForCtor(StructuredStringBuilder sb, TypeGeneration field, string typeStr, string valueStr)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"this.{field.Name} = {valueStr};");
    }

    public override string GetErrorMaskTypeStr(TypeGeneration field)
    {
        return "Exception";
    }

    public override string GetTranslationMaskTypeStr(TypeGeneration field)
    {
        return "bool";
    }

    public override void GenerateForClearEnumerable(StructuredStringBuilder sb, TypeGeneration field)
    {
    }

    public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
    {
        return $"({field.Name}, null)";
    }

    public override void GenerateForTranslationMaskSet(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, string onAccessor)
    {
        sb.AppendLine($"{accessor.Access} = {onAccessor};");
    }

    public override string GetMaskTypeStr(TypeGeneration field, string typeStr) => typeStr;
}