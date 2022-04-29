using static Loqui.Generation.LoquiType;

namespace Loqui.Generation;

public class LoquiMaskFieldGeneration : MaskModuleField
{
    public override string GetErrorMaskTypeStr(TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        return $"MaskItem<Exception, {loqui.Mask(MaskType.Error)}?>";
    }

    public override string GetTranslationMaskTypeStr(TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        return $"MaskItem<bool, {loqui.Mask(MaskType.Translation)}?>";
    }

    public static string GetObjectErrorMask(LoquiType loqui, string accessor)
    {
        return $"new MaskItem<Exception, {loqui.Mask(MaskType.Error)}>(null, {accessor})";
    }

    public override void GenerateForField(StructuredStringBuilder sb, TypeGeneration field, string typeStr)
    {
        LoquiType loqui = field as LoquiType;
        sb.AppendLine($"public MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>? {field.Name} {{ get; set; }}");
    }

    public override void GenerateForErrorMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        sb.AppendLine($"public MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>? {field.Name};");
    }

    public override void GenerateSetException(StructuredStringBuilder sb, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        sb.AppendLine($"this.{field.Name} = new MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>(ex, null);");
    }

    public override void GenerateSetMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        sb.AppendLine($"this.{field.Name} = (MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>?)obj;");
    }

    public override void GenerateForCopyMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            if (loqui.Singleton)
            {
                if (loqui.SetterInterfaceType == LoquiInterfaceType.IGetter) return;
                sb.AppendLine($"public MaskItem<bool, {loqui.Mask(MaskType.Copy)}> {field.Name};");
            }
            else
            {
                sb.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}> {field.Name};");
            }
        }
        else
        {
            sb.AppendLine($"public {nameof(CopyOption)} {field.Name};");
        }
    }

    public override void GenerateForTranslationMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            sb.AppendLine($"public {loqui.Mask(MaskType.Translation)}? {field.Name};");
        }
        else
        {
            sb.AppendLine($"public bool {field.Name};");
        }
    }

    public bool IsUnknownGeneric(LoquiType type)
    {
        return type.RefType != LoquiRefType.Direct
               && type.TargetObjectGeneration == null;
    }

    public override void GenerateMaskToString(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
    {
        if (!field.IntegrateField) return;
        using (var ifArg = sb.If(ANDs: true))
        {
            if (printMask)
            {
                ifArg.Add(GenerateBoolMaskCheck(field, "printMask"), wrapInParens: true);
            }
            ifArg.Body = subSb => subSb.AppendLine($"{accessor}?.ToString(sb);");
        }
    }

    public override void GenerateForAll(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        LoquiType loqui = field as LoquiType;

        if (nullCheck)
        {
            sb.AppendLine($"if ({field.Name} != null)");
        }
        using (sb.CurlyBrace(doIt: nullCheck))
        {
            sb.AppendLine($"if (!eval({accessor.Access}.Overall)) return false;");
            if (!IsUnknownGeneric(loqui))
            {
                sb.AppendLine($"if ({accessor.Access}.Specific != null && !{accessor.Access}.Specific.All(eval)) return false;");
            }
            else
            {
                sb.AppendLine($"if (!({accessor.Access}.Specific?.All(eval) ?? true)) return false;");
            }
        }
    }

    public override void GenerateForAny(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        LoquiType loqui = field as LoquiType;

        if (nullCheck)
        {
            sb.AppendLine($"if ({field.Name} != null)");
        }
        using (sb.CurlyBrace(doIt: nullCheck))
        {
            sb.AppendLine($"if (eval({accessor.Access}.Overall)) return true;");
            if (!IsUnknownGeneric(loqui))
            {
                sb.AppendLine($"if ({accessor.Access}.Specific != null && {accessor.Access}.Specific.Any(eval)) return true;");
            }
            else
            {
                sb.AppendLine($"if (({accessor.Access}.Specific?.Any(eval) ?? false)) return true;");
            }
        }
    }

    public override void GenerateForTranslate(StructuredStringBuilder sb, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
    {
        LoquiType loqui = field as LoquiType;
        if (IsUnknownGeneric(loqui))
        {
            sb.AppendLine($"{retAccessor};");
            sb.AppendLine($"throw new {nameof(NotImplementedException)}();");
        }
        else
        {
            sb.AppendLine($"{retAccessor} = {rhsAccessor} == null ? null : new MaskItem{(indexed ? "Indexed" : null)}<R, {loqui.GenerateMaskString("R")}?>({(indexed ? $"{rhsAccessor}.Index, " : null)}eval({rhsAccessor}.Overall), {rhsAccessor}.Specific?.Translate(eval));");
        }
    }

    public override void GenerateForErrorMaskCombine(StructuredStringBuilder sb, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
    {
        LoquiType loqui = field as LoquiType;
        if (!IsUnknownGeneric(loqui))
        {
            sb.AppendLine($"{retAccessor} = {accessor}.Combine({rhsAccessor}, (l, r) => l.Combine(r));");
        }
        else
        {
            sb.AppendLine($"{retAccessor} = new MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>(ExceptionExt.Combine({accessor}.Overall, {rhsAccessor}.Overall), Loqui.Internal.LoquiHelper.Combine({accessor}.Specific, {rhsAccessor}.Specific));");
        }
    }

    public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
    {
        return $"{boolMaskAccessor}?.{field.Name}?.Overall ?? true";
    }

    public override void GenerateForCtor(StructuredStringBuilder sb, TypeGeneration field, string typeStr, string valueStr)
    {
        LoquiType loqui = field as LoquiType;
        sb.AppendLine($"this.{field.Name} = new MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>({valueStr}, {(loqui.TargetObjectGeneration == null ? "null" : $"new {loqui.TargetObjectGeneration.GetMaskString(typeStr)}({valueStr})")});");
    }

    public override void GenerateForClearEnumerable(StructuredStringBuilder sb, TypeGeneration field)
    {
    }

    public override string GetMaskString(TypeGeneration field, string valueStr, string? indexed)
    {
        var loqui = field as LoquiType;
        return $"MaskItem{(indexed != null ? "Indexed" : null)}<{valueStr}, {(loqui.TargetObjectGeneration?.GetMaskString(valueStr) ?? $"IMask<{valueStr}>")}?>";
    }

    public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
    {
        return $"({field.Name} != null ? {field.Name}.OnOverall : DefaultOn, {field.Name}?.GetCrystal())";
    }

    public override void GenerateForCopyMaskCtor(StructuredStringBuilder sb, TypeGeneration field, string basicValueStr, string deepCopyStr)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            if (loqui.Singleton)
            {
                if (loqui.SetterInterfaceType == LoquiInterfaceType.IGetter) return;
                sb.AppendLine($"this.{field.Name} = new MaskItem<bool, {loqui.Mask(MaskType.Copy)}>({basicValueStr}, default);");
            }
            else
            {
                sb.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}>({deepCopyStr}, default);");
            }
        }
        else
        {
            sb.AppendLine($"this.{field.Name} = {deepCopyStr};");
        }
    }

    public override void GenerateForTranslationMaskSet(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, string onAccessor)
    {
        // Nothing
    }

    public override string GetMaskTypeStr(TypeGeneration field, string typeStr)
    {
        LoquiType loqui = field as LoquiType;
        return $"MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>?";
    }
}