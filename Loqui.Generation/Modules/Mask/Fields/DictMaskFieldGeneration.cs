using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class DictMaskFieldGeneration : MaskModuleField
{
    public static string GetMaskString(IDictType dictType, string typeStr, bool getter)
    {
        return $"MaskItem<{typeStr}, IEnumerable<{GetSubMaskString(dictType, typeStr, getter)}>?>";
    }

    public override string GetMaskTypeStr(TypeGeneration field, string typeStr)
    {
        return GetMaskString(field as IDictType, typeStr, getter: false);
    }

    public static string GetSubMaskString(IDictType dictType, string typeStr, bool getter)
    {
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                if (valueLoquiType != null)
                {
                    return $"{(valueLoquiType == null ? $"({dictType.KeyTypeGen.TypeName(getter)} Key, {typeStr} Value)" : $"MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter)}, {typeStr}, {valueLoquiType.GetMaskString(typeStr)}?>")}";
                }
                else
                {
                    string keyStr = $"{(keyLoquiType == null ? dictType.KeyTypeGen.TypeName(getter: true) : $"MaskItem<{typeStr}, {keyLoquiType.GetMaskString(typeStr)}?>")}";
                    string valueStr = $"{(valueLoquiType == null ? typeStr : $"MaskItem<{typeStr}, {valueLoquiType.GetMaskString(typeStr)}?>")}";
                    return $"KeyValuePair<{keyStr}, {valueStr}>";
                }
            case DictMode.KeyedValue:
                return $"{(valueLoquiType == null ? $"({dictType.KeyTypeGen.TypeName(getter)} Key, {typeStr} Value)" : $"MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter)}, {typeStr}, {valueLoquiType.GetMaskString(typeStr)}?>")}";
            default:
                throw new NotImplementedException();
        };
    }

    public static string GetErrorMaskString(IDictType dictType)
    {
        return $"MaskItem<Exception?, IEnumerable<{GetSubErrorMaskString(dictType)}>?>";
    }

    public static string GetSubErrorMaskString(IDictType dictType)
    {
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
                if (keyLoquiType != null)
                {
                    throw new NotImplementedException();
                }
                if (valueLoquiType == null)
                {
                    return $"KeyValuePair<{dictType.KeyTypeGen.TypeName(getter: true)}, Exception?>";
                }
                else
                {
                    return $"MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter: true)}, Exception?, {valueLoquiType.Mask(MaskType.Error)}?>";
                }
            case DictMode.KeyedValue:
                return $"{(valueLoquiType == null ? "Exception?" : $"MaskItem<Exception?, {valueLoquiType.Mask(MaskType.Error)}?>")}";
            default:
                throw new NotImplementedException();
        }
    }

    public static string GetSubTranslationMaskString(IDictType dictType)
    {
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;
        string keyStr = $"{(keyLoquiType == null ? "bool" : $"MaskItem<bool, {keyLoquiType.Mask(MaskType.Translation)}?>")}";
        string valueStr = $"{(valueLoquiType == null ? "bool" : $"MaskItem<bool, {valueLoquiType.Mask(MaskType.Translation)}?>")}";

        string itemStr;
        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                itemStr = $"KeyValuePair<{keyStr}, {valueStr}>";
                break;
            case DictMode.KeyedValue:
                itemStr = valueStr;
                break;
            default:
                throw new NotImplementedException();
        }
        return itemStr;
    }

    public override void GenerateForField(StructuredStringBuilder sb, TypeGeneration field, string typeStr)
    {
        sb.AppendLine($"public {GetMaskString(field as IDictType, typeStr, getter: false)}? {field.Name};");
    }

    public override void GenerateSetException(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name} = new {GetErrorMaskString(field as IDictType)}(ex, null);");
    }

    public override void GenerateSetMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name} = ({GetErrorMaskString(field as IDictType)})obj;");
    }

    public override void GenerateForCopyMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        DictType dictType = field as DictType;
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                if (keyLoquiType == null && valueLoquiType == null)
                {
                    sb.AppendLine($"public bool {field.Name};");
                }
                else if (keyLoquiType != null && valueLoquiType != null)
                {
                    sb.AppendLine($"public MaskItem<bool, KeyValuePair<({nameof(RefCopyType)} Type, {keyLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask), ({nameof(RefCopyType)} Type, {valueLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)>> {field.Name};");
                }
                else
                {
                    LoquiType loqui = keyLoquiType ?? valueLoquiType;
                    sb.AppendLine($"public MaskItem<bool, ({nameof(RefCopyType)} Type, {loqui.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)> {field.Name};");
                }
                break;
            case DictMode.KeyedValue:
                sb.AppendLine($"public MaskItem<{nameof(CopyOption)}, {valueLoquiType.Mask(MaskType.Copy)}> {field.Name};");
                break;
            default:
                break;
        }
    }

    public override void GenerateForTranslationMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        DictType dictType = field as DictType;
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                if (keyLoquiType == null && valueLoquiType == null)
                {
                    sb.AppendLine($"public bool {field.Name};");
                }
                else if (keyLoquiType != null && valueLoquiType != null)
                {
                    sb.AppendLine($"public KeyValuePair<{keyLoquiType.TargetObjectGeneration.Mask(MaskType.Translation)}, {valueLoquiType.TargetObjectGeneration.Mask(MaskType.Translation)}>? {field.Name};");
                }
                else
                {
                    LoquiType loqui = keyLoquiType ?? valueLoquiType;
                    sb.AppendLine($"public {loqui.TargetObjectGeneration.Mask(MaskType.Translation)}? {field.Name};");
                }
                break;
            case DictMode.KeyedValue:
                sb.AppendLine($"public {valueLoquiType.Mask(MaskType.Translation)}? {field.Name};");
                break;
            default:
                break;
        }
    }

    public override void GenerateMaskToString(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
    {
        if (printMask)
        {
            sb.AppendLine($"if ({GenerateBoolMaskCheck(field, "printMask")})");
        }
        using (sb.CurlyBrace(printMask))
        {
            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"{field.Name} =>\");");
            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
            sb.AppendLine($"using (sb.IncreaseDepth())");
            using (sb.CurlyBrace())
            {
                DictType dictType = field as DictType;
                var valIsLoqui = dictType.ValueTypeGen is LoquiType;

                sb.AppendLine($"if ({accessor} != null)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"if ({accessor}.Overall != null)");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}({accessor}.Overall.ToString());");
                    }
                    sb.AppendLine($"if ({accessor}.Specific != null)");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"foreach (var subItem in {accessor}{(topLevel ? ".Specific" : string.Empty)})");
                        using (sb.CurlyBrace())
                        {
                            var keyFieldGen = Module.GetMaskModule(dictType.KeyTypeGen.GetType());
                            var valFieldGen = Module.GetMaskModule(dictType.ValueTypeGen.GetType());
                            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
                            sb.AppendLine($"using (sb.IncreaseDepth())");
                            using (sb.CurlyBrace())
                            {
                                switch (dictType.Mode)
                                {
                                    case DictMode.KeyValue:
                                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"Key => [\");");
                                        sb.AppendLine($"using (sb.IncreaseDepth())");
                                        using (sb.CurlyBrace())
                                        {
                                            keyFieldGen.GenerateMaskToString(sb, dictType.KeyTypeGen, $"subItem.{(valIsLoqui ? "Index" : "Key")}", topLevel: false, printMask: false);
                                        }
                                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
                                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"Value => [\");");
                                        sb.AppendLine($"using (sb.IncreaseDepth())");
                                        using (sb.CurlyBrace())
                                        {
                                            valFieldGen.GenerateMaskToString(sb, dictType.ValueTypeGen, $"subItem.{(valIsLoqui ? "Specific" : "Value")}", topLevel: false, printMask: false);
                                        }
                                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
                                        break;
                                    case DictMode.KeyedValue:
                                        keyFieldGen.GenerateMaskToString(sb, dictType.KeyTypeGen, "subItem", topLevel: false, printMask: false);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
                        }
                    }
                }
            }
            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
        }
    }

    public override void GenerateForAll(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        DictType dictType = field as DictType;

        if (nullCheck)
        {
            sb.AppendLine($"if ({accessor.Access} != null)");
        }
        using (sb.CurlyBrace(doIt: nullCheck))
        {
            sb.AppendLine($"if (!eval({accessor.Access}.Overall)) return false;");
            sb.AppendLine($"if ({accessor.Access}.Specific != null)");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"foreach (var item in {accessor.Access}.Specific)");
                using (sb.CurlyBrace())
                {
                    switch (dictType.Mode)
                    {
                        case DictMode.KeyValue:
                            if (dictType.ValueTypeGen is LoquiType loquiVal)
                            {
                                sb.AppendLine($"if (item.Specific != null)");
                                using (sb.CurlyBrace())
                                {
                                    sb.AppendLine($"if (!eval(item.Overall)) return false;");
                                    sb.AppendLine($"if (!item.Specific?.All(eval) ?? false) return false;");
                                }
                            }
                            else
                            {
                                sb.AppendLine($"if (!eval(item.Value)) return false;");
                            }
                            break;
                        case DictMode.KeyedValue:
                            sb.AppendLine($"if (!eval(item.Overall)) return false;");
                            sb.AppendLine($"if (!item.Specific?.All(eval) ?? false) return false;");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public override void GenerateForAny(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        DictType dictType = field as DictType;

        if (nullCheck)
        {
            sb.AppendLine($"if ({accessor.Access} != null)");
        }
        using (sb.CurlyBrace(doIt: nullCheck))
        {
            sb.AppendLine($"if (eval({accessor.Access}.Overall)) return true;");
            sb.AppendLine($"if ({accessor.Access}.Specific != null)");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"foreach (var item in {accessor.Access}.Specific)");
                using (sb.CurlyBrace())
                {
                    switch (dictType.Mode)
                    {
                        case DictMode.KeyValue:
                            if (dictType.ValueTypeGen is LoquiType loquiVal)
                            {
                                sb.AppendLine($"if (item.Specific != null)");
                                using (sb.CurlyBrace())
                                {
                                    sb.AppendLine($"if (eval(item.Overall)) return true;");
                                    sb.AppendLine($"if (item.Specific?.Any(eval) ?? false) return true;");
                                }
                            }
                            else
                            {
                                sb.AppendLine($"if (eval(item.Value)) return true;");
                            }
                            break;
                        case DictMode.KeyedValue:
                            sb.AppendLine($"if (eval(item.Overall)) return true;");
                            sb.AppendLine($"if (item.Specific?.Any(eval) ?? false) return true;");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public override void GenerateForTranslate(StructuredStringBuilder sb, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
    {
        DictType dictType = field as DictType;

        sb.AppendLine($"if ({field.Name} != null)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"{retAccessor} = new {GetMaskString(dictType, "R", getter: false)}(eval({rhsAccessor}.Overall), default);");
            sb.AppendLine($"if ({field.Name}.Specific != null)");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"List<{GetSubMaskString(dictType, "R", getter: false)}> l = new List<{GetSubMaskString(dictType, "R", getter: false)}>();");
                sb.AppendLine($"{retAccessor}.Specific = l;");
                sb.AppendLine($"foreach (var item in {field.Name}.Specific)");
                using (sb.CurlyBrace())
                {
                    switch (dictType.Mode)
                    {
                        case DictMode.KeyValue:
                            sb.AppendLine("throw new NotImplementedException();");
                            //if (dictType.ValueTypeGen is LoquiType loquiVal)
                            //{
                            //    sb.AppendLine($"MaskItem<R, {loquiVal.GenerateMaskString("R")}> valVal = default(MaskItem<R, {loquiVal.GenerateMaskString("R")}>);");
                            //    this.Module.GetMaskModule(loquiVal.GetType()).GenerateForTranslate(sb, loquiVal, "valVal", "item.Value", indexed);
                            //}
                            //else
                            //{
                            //    sb.AppendLine($"R valVal = eval(item.Value);");
                            //}
                            //sb.AppendLine($"l.Add(new {GetSubMaskString(dictType, "R", getter: false)}(item.Key, valVal));");
                            break;
                        case DictMode.KeyedValue:
                            sb.AppendLine("throw new NotImplementedException();");
                            //var loquiType = dictType.ValueTypeGen as LoquiType;
                            //sb.AppendLine($"MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter: false)}, R, {loquiType.GenerateMaskString("R")}?> mask = default(MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter: false)}, R, {loquiType.GenerateMaskString("R")}?>);");
                            //var fieldGen = this.Module.GetMaskModule(loquiType.GetType());
                            //fieldGen.GenerateForTranslate(sb, loquiType, "mask", "item", true);
                            //sb.AppendLine($"l.Add(mask);");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public override void GenerateForErrorMaskCombine(StructuredStringBuilder sb, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
    {
        DictType dictType = field as DictType;
        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                string str;
                if (dictType.ValueTypeGen is LoquiType valLoqui)
                {
                    str = $"MaskItemIndexed<{dictType.KeyTypeGen.TypeName(getter: true)}, Exception?, {valLoqui.Mask(MaskType.Error)}?>";
                }
                else
                {
                    if (dictType.KeyTypeGen is LoquiType keyLoqui)
                    {
                        throw new NotImplementedException();
                    }
                    str = $"KeyValuePair<{dictType.KeyTypeGen.TypeName(getter: true)}, Exception?>";
                }
                sb.AppendLine($"{retAccessor} = new MaskItem<Exception?, IEnumerable<{str}>?>(ExceptionExt.Combine({accessor}?.Overall, {rhsAccessor}?.Overall), ExceptionExt.Combine({accessor}?.Specific, {rhsAccessor}?.Specific));");
                break;
            case DictMode.KeyedValue:
                var loqui = dictType.ValueTypeGen as LoquiType;
                sb.AppendLine($"{retAccessor} = new MaskItem<Exception?, IEnumerable<MaskItem<Exception?, {loqui.Mask(MaskType.Error)}?>>?>(ExceptionExt.Combine({accessor}?.Overall, {rhsAccessor}?.Overall), ExceptionExt.Combine({accessor}?.Specific, {rhsAccessor}?.Specific));");
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
    {
        return $"{boolMaskAccessor}?.{field.Name}?.Overall ?? true";
    }

    public override void GenerateForCtor(StructuredStringBuilder sb, TypeGeneration field, string typeStr, string valueStr)
    {
        sb.AppendLine($"this.{field.Name} = new {GetMaskString(field as IDictType, typeStr, getter: false)}({valueStr}, null);");
    }

    public override string GetErrorMaskTypeStr(TypeGeneration field)
    {
        return GetErrorMaskString(field as IDictType);
    }

    public override string GetTranslationMaskTypeStr(TypeGeneration field)
    {
        return $"MaskItem<bool, IEnumerable<{GetSubTranslationMaskString(field as IDictType)}>>";
    }

    public override void GenerateForClearEnumerable(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name}.Specific = null;");
    }

    public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
    {
        var dictType = field as DictType;
        if (dictType.ValueTypeGen is LoquiType loquiType)
        {
            return $"({field.Name} != null || DefaultOn, {field.Name}?.GetCrystal())";
        }
        else
        {
            return $"({field.Name}, null)";
        }
    }

    public override void GenerateForCopyMaskCtor(StructuredStringBuilder sb, TypeGeneration field, string basicValueStr, string deepCopyStr)
    {
        DictType dictType = field as DictType;
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                if (keyLoquiType == null && valueLoquiType == null)
                {
                    sb.AppendLine($"this.{field.Name} = {basicValueStr};");
                }
                else if (keyLoquiType != null && valueLoquiType != null)
                {
                    sb.AppendLine($"this.{field.Name} = new MaskItem<bool, KeyValuePair<({nameof(RefCopyType)} Type, {keyLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask), ({nameof(RefCopyType)} Type, {valueLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)>>({basicValueStr}, default);");
                }
                else
                {
                    LoquiType loqui = keyLoquiType ?? valueLoquiType;
                    sb.AppendLine($"this.{field.Name} = new MaskItem<bool, ({nameof(RefCopyType)} Type, {loqui.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)>({basicValueStr}, default);");
                }
                break;
            case DictMode.KeyedValue:
                sb.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {valueLoquiType.Mask(MaskType.Copy)}>({deepCopyStr}, default);");
                break;
            default:
                break;
        }
    }

    public override void GenerateForTranslationMaskSet(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, string onAccessor)
    {
        DictType dictType = field as DictType;
        LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

        switch (dictType.Mode)
        {
            case DictMode.KeyValue:
                if (keyLoquiType == null && valueLoquiType == null)
                {
                    sb.AppendLine($"{accessor.Access} = {onAccessor};");
                }
                break;
            default:
                break;
        }
    }
}