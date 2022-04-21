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

    public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
    {
        LoquiType loqui = field as LoquiType;
        fg.AppendLine($"public MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>? {field.Name} {{ get; set; }}");
    }

    public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        fg.AppendLine($"public MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>? {field.Name};");
    }

    public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        fg.AppendLine($"this.{field.Name} = new MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>(ex, null);");
    }

    public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        fg.AppendLine($"this.{field.Name} = (MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>?)obj;");
    }

    public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            if (loqui.Singleton)
            {
                if (loqui.SetterInterfaceType == LoquiInterfaceType.IGetter) return;
                fg.AppendLine($"public MaskItem<bool, {loqui.Mask(MaskType.Copy)}> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}> {field.Name};");
            }
        }
        else
        {
            fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
        }
    }

    public override void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            fg.AppendLine($"public {loqui.Mask(MaskType.Translation)}? {field.Name};");
        }
        else
        {
            fg.AppendLine($"public bool {field.Name};");
        }
    }

    public bool IsUnknownGeneric(LoquiType type)
    {
        return type.RefType != LoquiRefType.Direct
               && type.TargetObjectGeneration == null;
    }

    public override void GenerateMaskToString(FileGeneration fg, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
    {
        if (!field.IntegrateField) return;
        using (var ifArg = new IfWrapper(fg, ANDs: true))
        {
            if (printMask)
            {
                ifArg.Add(GenerateBoolMaskCheck(field, "printMask"), wrapInParens: true);
            }
            ifArg.Body = subFg => subFg.AppendLine($"{accessor}?.ToString(fg);");
        }
    }

    public override void GenerateForAll(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        LoquiType loqui = field as LoquiType;

        if (nullCheck)
        {
            fg.AppendLine($"if ({field.Name} != null)");
        }
        using (new BraceWrapper(fg, doIt: nullCheck))
        {
            fg.AppendLine($"if (!eval({accessor.Access}.Overall)) return false;");
            if (!IsUnknownGeneric(loqui))
            {
                fg.AppendLine($"if ({accessor.Access}.Specific != null && !{accessor.Access}.Specific.All(eval)) return false;");
            }
            else
            {
                fg.AppendLine($"if (!({accessor.Access}.Specific?.All(eval) ?? true)) return false;");
            }
        }
    }

    public override void GenerateForAny(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        LoquiType loqui = field as LoquiType;

        if (nullCheck)
        {
            fg.AppendLine($"if ({field.Name} != null)");
        }
        using (new BraceWrapper(fg, doIt: nullCheck))
        {
            fg.AppendLine($"if (eval({accessor.Access}.Overall)) return true;");
            if (!IsUnknownGeneric(loqui))
            {
                fg.AppendLine($"if ({accessor.Access}.Specific != null && {accessor.Access}.Specific.Any(eval)) return true;");
            }
            else
            {
                fg.AppendLine($"if (({accessor.Access}.Specific?.Any(eval) ?? false)) return true;");
            }
        }
    }

    public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
    {
        LoquiType loqui = field as LoquiType;
        if (IsUnknownGeneric(loqui))
        {
            fg.AppendLine($"{retAccessor};");
            fg.AppendLine($"throw new {nameof(NotImplementedException)}();");
        }
        else
        {
            fg.AppendLine($"{retAccessor} = {rhsAccessor} == null ? null : new MaskItem{(indexed ? "Indexed" : null)}<R, {loqui.GenerateMaskString("R")}?>({(indexed ? $"{rhsAccessor}.Index, " : null)}eval({rhsAccessor}.Overall), {rhsAccessor}.Specific?.Translate(eval));");
        }
    }

    public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
    {
        LoquiType loqui = field as LoquiType;
        if (!IsUnknownGeneric(loqui))
        {
            fg.AppendLine($"{retAccessor} = {accessor}.Combine({rhsAccessor}, (l, r) => l.Combine(r));");
        }
        else
        {
            fg.AppendLine($"{retAccessor} = new MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>(ExceptionExt.Combine({accessor}.Overall, {rhsAccessor}.Overall), Loqui.Internal.LoquiHelper.Combine({accessor}.Specific, {rhsAccessor}.Specific));");
        }
    }

    public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
    {
        return $"{boolMaskAccessor}?.{field.Name}?.Overall ?? true";
    }

    public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string typeStr, string valueStr)
    {
        LoquiType loqui = field as LoquiType;
        fg.AppendLine($"this.{field.Name} = new MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>({valueStr}, {(loqui.TargetObjectGeneration == null ? "null" : $"new {loqui.TargetObjectGeneration.GetMaskString(typeStr)}({valueStr})")});");
    }

    public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
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

    public override void GenerateForCopyMaskCtor(FileGeneration fg, TypeGeneration field, string basicValueStr, string deepCopyStr)
    {
        LoquiType loqui = field as LoquiType;
        if (loqui.RefType == LoquiRefType.Direct)
        {
            if (loqui.Singleton)
            {
                if (loqui.SetterInterfaceType == LoquiInterfaceType.IGetter) return;
                fg.AppendLine($"this.{field.Name} = new MaskItem<bool, {loqui.Mask(MaskType.Copy)}>({basicValueStr}, default);");
            }
            else
            {
                fg.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}>({deepCopyStr}, default);");
            }
        }
        else
        {
            fg.AppendLine($"this.{field.Name} = {deepCopyStr};");
        }
    }

    public override void GenerateForTranslationMaskSet(FileGeneration fg, TypeGeneration field, Accessor accessor, string onAccessor)
    {
        // Nothing
    }

    public override string GetMaskTypeStr(TypeGeneration field, string typeStr)
    {
        LoquiType loqui = field as LoquiType;
        return $"MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}?>?";
    }
}