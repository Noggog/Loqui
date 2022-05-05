using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class DictType_Typical : TypeGeneration, IDictType
{
    public TypeGeneration ValueTypeGen;
    TypeGeneration IDictType.ValueTypeGen => ValueTypeGen;
    protected bool ValueIsLoqui;
    public TypeGeneration KeyTypeGen;
    TypeGeneration IDictType.KeyTypeGen => KeyTypeGen;
    protected bool KeyIsLoqui;
    public DictMode Mode => DictMode.KeyValue;
    public bool BothAreLoqui => KeyIsLoqui && ValueIsLoqui;

    public override bool CopyNeedsTryCatch => true;
    public override bool IsEnumerable => true;
    public override bool IsClass => true;
    public override bool HasDefault => false;
    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        if (KeyTypeGen is LoquiType
            || ValueTypeGen is LoquiType)
        {
            return $"{copyMaskAccessor}?.{Name}.Overall ?? true";
        }
        else
        {
            return $"{copyMaskAccessor}?.{Name} ?? true";
        }
    }

    public override string TypeName(bool getter, bool needsCovariance = false) => $"Dictionary<{KeyTypeGen.TypeName(getter, needsCovariance)}, {ValueTypeGen.TypeName(getter, needsCovariance)}>";

    public string TypeTuple(bool getter) => $"{KeyTypeGen.TypeName(getter)}, {ValueTypeGen.TypeName(getter)}";

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);

        var keyNode = node.Element(XName.Get("Key", LoquiGenerator.Namespace));
        if (keyNode == null)
        {
            throw new ArgumentException("Dict had no key element.");
        }

        var keyTypeGen = await ObjectGen.LoadField(
            keyNode.Elements().FirstOrDefault(),
            requireName: false,
            setDefaults: false);
        if (keyTypeGen.Succeeded)
        {
            KeyTypeGen = keyTypeGen.Value;
            KeyIsLoqui = keyTypeGen.Value as LoquiType != null;
        }
        else
        {
            throw new NotImplementedException();
        }

        var valNode = node.Element(XName.Get("Value", LoquiGenerator.Namespace));
        if (valNode == null)
        {
            throw new ArgumentException("Dict had no value element.");
        }

        var valueTypeGen = await ObjectGen.LoadField(
            valNode.Elements().FirstOrDefault(),
            requireName: false,
            setDefaults: false);
        if (valueTypeGen.Succeeded)
        {
            ValueTypeGen = valueTypeGen.Value;
            ValueIsLoqui = valueTypeGen.Value is LoquiType;
        }
        else
        {
            throw new NotImplementedException();
        }

        if (keyTypeGen.Value is ContainerType
            || keyTypeGen.Value is DictType)
        {
            throw new NotImplementedException();
        }
        if (valueTypeGen.Value is ContainerType
            || valueTypeGen.Value is DictType)
        {
            throw new NotImplementedException();
        }
    }

    public void AddMaskException(StructuredStringBuilder sb, string errorMaskMemberAccessor, string exception, bool key)
    {
        LoquiType keyLoquiType = KeyTypeGen as LoquiType;
        LoquiType valueLoquiType = ValueTypeGen as LoquiType;
        var item2 = $"KeyValuePair<{(keyLoquiType == null ? "Exception" : keyLoquiType.TargetObjectGeneration.GetMaskString("Exception"))}, {(valueLoquiType == null ? "Exception" : valueLoquiType.TargetObjectGeneration.GetMaskString("Exception"))}>";

        sb.AppendLine($"{errorMaskMemberAccessor}?.{Name}.Value.Add(new {item2}({(key ? exception : "null")}, {(key ? "null" : exception)}));");
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        if (!ReadOnly)
        {
            sb.AppendLine($"{identifier}.{GetName(false)}.Clear();");
        }
        sb.AppendLine("break;");
    }

    public override string GetName(bool internalUse)
    {
        if (internalUse)
        {
            return $"_{Name}";
        }
        else
        {
            return Name;
        }
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        sb.AppendLine($"private readonly Dictionary<{TypeTuple(getter: false)}> _{Name} = new Dictionary<{TypeTuple(getter: false)}>();");
        Comments?.Apply(sb, LoquiInterfaceType.Direct);
        sb.AppendLine($"public IDictionary<{TypeTuple(getter: false)}> {Name} => _{Name};");

        var member = "_" + Name;
        using (new Region(sb, "Interface Members"))
        {
            if (!ReadOnly)
            {
                sb.AppendLine($"IDictionary{(ReadOnly ? "Getter" : string.Empty)}<{TypeTuple(getter: false)}> {ObjectGen.Interface()}.{Name} => {member};");
            }
            if (ValueIsLoqui)
            {
                sb.AppendLine($"IReadOnlyDictionary<{TypeTuple(getter: true)}> {ObjectGen.Interface(getter: true)}.{Name} => {member}.Covariant<{TypeTuple(getter: false)}, {ValueTypeGen.TypeName(getter: true)}>();");
            }
            else
            {
                sb.AppendLine($"IReadOnlyDictionary<{TypeTuple(getter: true)}> {ObjectGen.Interface(getter: true)}.{Name} => {member};");
            }
        }
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        if (getter)
        {
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"IReadOnlyDictionary<{TypeTuple(getter: true)}> {Name} {{ get; }}");
        }
        else
        {
            if (!ReadOnly)
            {
                Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
                sb.AppendLine($"new IDictionary{(ReadOnly ? "Getter" : string.Empty)}<{TypeTuple(getter: false)}> {Name} {{ get; }}");
            }
        }
    }

    public override void GenerateForCopy(
        StructuredStringBuilder sb,
        Accessor accessor,
        Accessor rhs,
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            sb.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (sb.CurlyBrace(doIt: !AlwaysCopy))
        {
            if (!KeyIsLoqui && !ValueIsLoqui)
            {
                using (var args = sb.Call(
                           $"{accessor}.SetTo"))
                {
                    args.Add($"rhs.{Name}");
                }
                return;
            }
            if (deepCopy)
            {
                if (KeyIsLoqui)
                {
                    throw new NotImplementedException();
                }
                using (var args = sb.Call(
                           $"{accessor}.SetTo"))
                {
                    args.Add((gen) =>
                    {
                        gen.AppendLine($"rhs.{Name}");
                        using (gen.IncreaseDepth())
                        {
                            gen.AppendLine(".Select((r) =>");
                            using (new CurlyBrace(gen) { AppendParenthesis = true })
                            {
                                (ValueTypeGen as LoquiType).GenerateTypicalMakeCopy(
                                    gen,
                                    retAccessor: $"var value = ",
                                    rhsAccessor: new Accessor("r.Value"),
                                    copyMaskAccessor: copyMaskAccessor,
                                    deepCopy: deepCopy,
                                    doTranslationMask: false);
                                gen.AppendLine($"return new KeyValuePair<{KeyTypeGen.TypeName(getter: true)}, {ValueTypeGen.TypeName(getter: false)}>(r.Key, value);");
                            }
                        }
                    });
                }
                return;
            }
            throw new NotImplementedException();
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                sb,
                () =>
                {
                    if (!KeyIsLoqui && !ValueIsLoqui)
                    {
                        using (var args = sb.Call(
                                   $"{accessor}.SetTo"))
                        {
                            args.Add($"rhs.{Name}");
                        }
                        return;
                    }
                    using (var args = sb.Call(
                               $"{accessor}.SetTo"))
                    {
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"rhs.{Name}");
                            using (gen.IncreaseDepth())
                            {
                                gen.AppendLine(".Select((r) =>");
                                using (new CurlyBrace(gen) { AppendParenthesis = true })
                                {
                                    if (KeyIsLoqui)
                                    {
                                        throw new NotImplementedException();
                                        gen.AppendLine($"{KeyTypeGen.TypeName(getter: false)} key;");
                                        gen.AppendLine($"switch ({copyMaskAccessor}?.Specific.{(BothAreLoqui ? "Key." : string.Empty)}Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                                        using (new CurlyBrace(gen))
                                        {
                                            gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                            using (gen.IncreaseDepth())
                                            {
                                                gen.AppendLine($"key = r.Key;");
                                                gen.AppendLine($"break;");
                                            }
                                            gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.MakeCopy)}:");
                                            using (gen.IncreaseDepth())
                                            {
                                                gen.AppendLine($"key = r.Key.Copy(copyMask: {copyMaskAccessor}?.Specific.{(BothAreLoqui ? "Key." : string.Empty)}Mask);");
                                                gen.AppendLine($"break;");
                                            }
                                            gen.AppendLine($"default:");
                                            using (gen.IncreaseDepth())
                                            {
                                                gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{Name}.Overall}}. Cannot execute copy.\");");
                                            }
                                        }
                                    }
                                    if (ValueTypeGen is LoquiType valLoqui)
                                    {
                                        gen.AppendLine($"{ValueTypeGen.TypeName(getter: false)} val;");
                                        gen.AppendLine($"switch ({copyMaskAccessor}?.Specific.{(BothAreLoqui ? "Value." : string.Empty)}Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                                        using (new CurlyBrace(gen))
                                        {
                                            gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                            using (gen.IncreaseDepth())
                                            {
                                                gen.AppendLine($"val = r.Value;");
                                                gen.AppendLine($"break;");
                                            }
                                            gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.MakeCopy)}:");
                                            using (gen.IncreaseDepth())
                                            {
                                                valLoqui.GenerateTypicalMakeCopy(
                                                    gen,
                                                    retAccessor: $"val = ",
                                                    rhsAccessor: new Accessor("r.Value"),
                                                    copyMaskAccessor: copyMaskAccessor,
                                                    deepCopy: deepCopy,
                                                    doTranslationMask: false);
                                                gen.AppendLine($"break;");
                                            }
                                            gen.AppendLine($"default:");
                                            using (gen.IncreaseDepth())
                                            {
                                                gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{Name}.Overall}}. Cannot execute copy.\");");
                                            }
                                        }
                                    }

                                    gen.AppendLine($"return new KeyValuePair<{KeyTypeGen.TypeName(getter: false)}, {ValueTypeGen.TypeName(getter: false)}>({(KeyIsLoqui ? "key" : "r.Key")}, {(ValueIsLoqui ? "val" : "r.Value")});");
                                }
                            }
                        });
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    private void GenerateCopy(StructuredStringBuilder sb, string accessorPrefix, string rhsAccessorPrefix)
    {
        sb.AppendLine($"{accessorPrefix}.{Name}.SetTo(");
        using (sb.IncreaseDepth())
        {
            sb.AppendLine($"{rhsAccessorPrefix}.{Name}.Select(");
            using (sb.IncreaseDepth())
            {
                sb.AppendLine($"(i) => new KeyValuePair<{KeyTypeGen.TypeName(getter: false)}, {ValueTypeGen.TypeName(getter: false)}>(");
                using (sb.IncreaseDepth())
                {
                    sb.AppendLine($"i.Key{(KeyIsLoqui ? ".CopyFieldsFrom()" : string.Empty) },");
                    sb.AppendLine($"i.Value{(ValueIsLoqui ? ".CopyFieldsFrom()" : string.Empty)})),");
                }
            }
        }
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        sb.AppendLine($"{accessor}.SetTo(");
        using (sb.IncreaseDepth())
        {
            sb.AppendLine($"({rhs}).Select(");
            using (sb.IncreaseDepth())
            {
                sb.AppendLine($"(i) => new KeyValuePair<{KeyTypeGen.TypeName(getter: false)}, {ValueTypeGen.TypeName(getter: false)}>(");
                using (sb.IncreaseDepth())
                {
                    sb.AppendLine($"i.Key{(KeyIsLoqui ? ".Copy()" : string.Empty) },");
                    sb.AppendLine($"i.Value{(ValueIsLoqui ? ".Copy()" : string.Empty)})),");
                }
            }
        }
        sb.AppendLine($"break;");
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        sb.AppendLine($"return {identifier.Access};");
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        sb.AppendLine($"{accessorPrefix.Access}.Clear();");
    }

    public override string GenerateACopy(string rhsAccessor)
    {
        throw new NotImplementedException();
    }

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
    {
        return $"{(negate ? "!" : null)}{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})";
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
        }
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        GenerateForEqualsMaskCheck(sb, $"item.{Name}", $"rhs.{Name}", $"ret.{Name}");
    }

    public void GenerateForEqualsMaskCheck(StructuredStringBuilder sb, string accessor, string rhsAccessor, string retAccessor)
    {
        LoquiType keyLoqui = KeyTypeGen as LoquiType;
        LoquiType valLoqui = ValueTypeGen as LoquiType;
        if (keyLoqui != null
            && valLoqui != null)
        {
            throw new NotImplementedException();
        }
        else if (keyLoqui != null)
        {
            throw new NotImplementedException();
        }
        else if (valLoqui != null)
        {
            using (var args = sb.Call(
                       $"{retAccessor} = EqualsMaskHelper.DictEqualsHelper"))
            {
                args.Add($"lhs: {accessor}");
                args.Add($"rhs: {rhsAccessor}");
                args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
                args.AddPassArg("include");
            }
        }
        else
        {
            using (var args = sb.Call(
                       $"{retAccessor} = EqualsMaskHelper.DictEqualsHelper"))
            {
                args.Add($"lhs: {accessor}");
                args.Add($"rhs: {rhsAccessor}");
                args.AddPassArg("include");
            }
        }
    }

    public void GenerateForEqualsMask(StructuredStringBuilder sb, string retAccessor, bool on)
    {
        sb.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool", getter: true)}();");
        sb.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        sb.AppendLine($"{hashResultAccessor}.Add({accessor});");
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"{name} =>\");");
        sb.AppendLine($"using (sb.Brace())");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"foreach (var subItem in {accessor.Access})");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"using ({sbAccessor}.Brace())");
                using (sb.CurlyBrace())
                {
                    KeyTypeGen.GenerateToString(sb, "Key", new Accessor("subItem.Key"), sbAccessor);
                    ValueTypeGen.GenerateToString(sb, "Value", new Accessor("subItem.Value"), sbAccessor);
                }
            }
        }
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        sb.AppendLine($"if ({checkMaskAccessor}?.Overall.HasValue ?? false) return false;");
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}