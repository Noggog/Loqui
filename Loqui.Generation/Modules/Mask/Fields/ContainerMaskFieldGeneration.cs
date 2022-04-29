using Loqui.Internal;

namespace Loqui.Generation;

public class ContainerMaskFieldGeneration : MaskModuleField
{
    public override string IndexStr => "int";

    public virtual string GetItemString(ContainerType type, string valueStr)
    {
        var maskType = type.ObjectGen.ProtoGen.Gen.MaskModule.GetMaskModule(type.SubTypeGeneration.GetType());
        return maskType.GetMaskString(type.SubTypeGeneration, valueStr, indexed: IndexStr);
    }

    public virtual string GetListString(ContainerType listType, string valueStr)
    {
        return $"IEnumerable<{GetItemString(listType, valueStr)}>";
    }

    public virtual string GetMaskString(ContainerType listType, string valueStr)
    {
        return $"MaskItem<{valueStr}, {GetListString(listType, valueStr)}?>";
    }

    public override string GetMaskTypeStr(TypeGeneration field, string typeStr)
    {
        return GetMaskString(field as ContainerType, typeStr);
    }

    public override void GenerateForField(StructuredStringBuilder sb, TypeGeneration field, string valueStr)
    {
        sb.AppendLine($"public {GetMaskString(field as ContainerType, valueStr)}? {field.Name};");
    }

    public override void GenerateSetException(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name} = new {GetErrorMaskTypeStr(field)}(ex, null);");
    }

    public override void GenerateSetMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name} = ({GetErrorMaskTypeStr(field)})obj;");
    }

    public override void GenerateForCopyMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        ListType listType = field as ListType;
        if (listType.SubTypeGeneration is LoquiType loqui
            && loqui.SupportsMask(MaskType.Copy))
        {
            sb.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}?> {field.Name};");
        }
        else
        {
            sb.AppendLine($"public {nameof(CopyOption)} {field.Name};");
        }
    }

    public override void GenerateForCopyMaskCtor(StructuredStringBuilder sb, TypeGeneration field, string basicValueStr, string deepCopyStr)
    {
        ListType listType = field as ListType;
        if (listType.SubTypeGeneration is LoquiType loqui
            && loqui.SupportsMask(MaskType.Copy))
        {
            sb.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}?>({deepCopyStr}, default);");
        }
        else
        {
            sb.AppendLine($"this.{field.Name} = {deepCopyStr};");
        }
    }

    public override void GenerateForTranslationMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        ListType listType = field as ListType;
        if (listType.SubTypeGeneration is LoquiType loqui
            && loqui.SupportsMask(MaskType.Translation))
        {
            sb.AppendLine($"public {loqui.Mask(MaskType.Translation)}? {field.Name};");
        }
        else
        {
            sb.AppendLine($"public bool {field.Name};");
        }
    }

    public override void GenerateMaskToString(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
    {
        using (var args = sb.If(ANDs: true))
        {
            if (printMask)
            {
                args.Add(GenerateBoolMaskCheck(field, "printMask"), wrapInParens: true);
            }
            args.Add($"{accessor} is {{}} {field.Name}Item");
            accessor = $"{field.Name}Item";
            args.Body = (sb) =>
            {
                sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"{field.Name} =>\");");
                sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
                sb.AppendLine($"using (sb.IncreaseDepth())");
                using (sb.CurlyBrace())
                {
                    ContainerType listType = field as ContainerType;
                    if (topLevel)
                    {
                        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendItem)}({accessor}.Overall);");
                        sb.AppendLine($"if ({accessor}.Specific != null)");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"foreach (var subItem in {accessor}.Specific)");
                            using (sb.CurlyBrace())
                            {
                                sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
                                var fieldGen = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                                sb.AppendLine($"using (sb.IncreaseDepth())");
                                using (sb.CurlyBrace())
                                {
                                    fieldGen.GenerateMaskToString(sb, listType.SubTypeGeneration, "subItem", topLevel: false, printMask: false);
                                }
                                sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine($"foreach (var subItem in {accessor})");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
                            var fieldGen = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                            sb.AppendLine($"using (sb.IncreaseDepth())");
                            using (sb.CurlyBrace())
                            {
                                fieldGen.GenerateMaskToString(sb, listType.SubTypeGeneration, "subItem", topLevel: false, printMask: false);
                            }
                            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
                        }
                    }
                }
                sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
            };
        }
    }

    public override void GenerateForAll(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        ListType listType = field as ListType;

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
                    var subMask = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                    subMask.GenerateForAll(sb, listType.SubTypeGeneration, new Accessor("item"), nullCheck: false, indexed: true);
                }
            }
        }
    }

    public override void GenerateForAny(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
    {
        ListType listType = field as ListType;

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
                    var subMask = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                    subMask.GenerateForAll(sb, listType.SubTypeGeneration, new Accessor("item"), nullCheck: false, indexed: true);
                }
            }
        }
    }

    public override void GenerateForTranslate(StructuredStringBuilder sb, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
    {
        ListType listType = field as ListType;

        sb.AppendLine($"if ({field.Name} != null)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"{retAccessor} = new {GetMaskString(listType, "R")}(eval({rhsAccessor}.Overall), Enumerable.Empty<{GetItemString(listType, "R")}>());");
            sb.AppendLine($"if ({field.Name}.Specific != null)");
            using (sb.CurlyBrace())
            {
                if (listType.SubTypeGeneration is LoquiType loqui)
                {
                    var submaskString = $"MaskItemIndexed<R, {loqui.GetMaskString("R")}?>";
                    sb.AppendLine($"var l = new List<{submaskString}>();");
                    sb.AppendLine($"{retAccessor}.Specific = l;");
                    sb.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (sb.CurlyBrace())
                    {
                        var fieldGen = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                        fieldGen.GenerateForTranslate(sb, listType.SubTypeGeneration,
                            retAccessor: $"{submaskString}? mask",
                            rhsAccessor: $"item",
                            indexed: true);
                        sb.AppendLine("if (mask == null) continue;");
                        sb.AppendLine($"l.Add(mask);");
                    }
                }
                else
                {
                    sb.AppendLine($"var l = new List<({IndexStr} Index, R Item)>();");
                    sb.AppendLine($"{retAccessor}.Specific = l;");
                    sb.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (sb.CurlyBrace())
                    {
                        var fieldGen = Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                        fieldGen.GenerateForTranslate(sb, listType.SubTypeGeneration,
                            retAccessor: $"R mask",
                            rhsAccessor: $"item",
                            indexed: true);
                        sb.AppendLine($"l.Add((item.Index, mask));");
                    }
                }
            }
        }
    }
            
    public override void GenerateForErrorMaskCombine(StructuredStringBuilder sb, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
    {
        ContainerType cont = field as ContainerType;
        LoquiType loquiType = cont.SubTypeGeneration as LoquiType;
        string itemStr;
        if (loquiType == null)
        {
            itemStr = GetItemString(cont, "Exception");
        }
        else
        {
            itemStr = $"MaskItem<Exception?, {loquiType.Mask(MaskType.Error)}?>";
        }
        sb.AppendLine($"{retAccessor} = new MaskItem<Exception?, IEnumerable<{itemStr}>?>(ExceptionExt.Combine({accessor}?.Overall, {rhsAccessor}?.Overall), ExceptionExt.Combine({accessor}?.Specific, {rhsAccessor}?.Specific));");
    }

    public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
    {
        return $"{boolMaskAccessor}?.{field.Name}?.Overall ?? true";
    }

    public override void GenerateForCtor(StructuredStringBuilder sb, TypeGeneration field, string typeStr, string valueStr)
    {
        sb.AppendLine($"this.{field.Name} = new {GetMaskString(field as ContainerType, typeStr)}({valueStr}, Enumerable.Empty<{GetItemString(field as ContainerType, typeStr)}>());");
    }

    public override string GetErrorMaskTypeStr(TypeGeneration field)
    {
        var contType = field as ContainerType;
        LoquiType loquiType = contType.SubTypeGeneration as LoquiType;
        string itemStr;
        if (loquiType == null)
        {
            itemStr = GetItemString(contType, "Exception");
        }
        else
        {
            itemStr = $"MaskItem<Exception?, {loquiType.Mask(MaskType.Error)}?>";
        }
        return $"MaskItem<Exception?, IEnumerable<{itemStr}>?>";
    }

    public override string GetTranslationMaskTypeStr(TypeGeneration field)
    {
        var contType = field as ContainerType;
        LoquiType loquiType = contType.SubTypeGeneration as LoquiType;
        string itemStr;
        if (loquiType == null)
        {
            itemStr = GetItemString(contType, "bool");
        }
        else
        {
            itemStr = $"MaskItem<bool, {loquiType.Mask(MaskType.Translation)}>";
        }
        return $"MaskItem<bool, IEnumerable<{itemStr}>>";
    }

    public override void GenerateForClearEnumerable(StructuredStringBuilder sb, TypeGeneration field)
    {
        sb.AppendLine($"this.{field.Name}.Specific = null;");
    }

    public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
    {
        var contType = field as ContainerType;
        if (contType.SubTypeGeneration is LoquiType loquiType
            && loquiType.SupportsMask(MaskType.Translation))
        {
            return $"({field.Name} == null ? DefaultOn : !{field.Name}.GetCrystal().{nameof(TranslationCrystal.CopyNothing)}, {field.Name}?.GetCrystal())";
        }
        else
        {
            return $"({field.Name}, null)";
        }
    }

    public override void GenerateForTranslationMaskSet(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, string onAccessor)
    {
        ListType listType = field as ListType;
        if (listType.SubTypeGeneration is LoquiType loqui
            && loqui.SupportsMask(MaskType.Translation))
        {
            // Nothing
        }
        else
        {
            sb.AppendLine($"{accessor.Access} = {onAccessor};");
        }
    }
}