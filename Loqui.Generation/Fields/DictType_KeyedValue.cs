using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class DictType_KeyedValue : TypeGeneration, IDictType
{
    public LoquiType ValueTypeGen;
    TypeGeneration IDictType.ValueTypeGen => ValueTypeGen;
    public TypeGeneration KeyTypeGen;
    TypeGeneration IDictType.KeyTypeGen => KeyTypeGen;
    public string KeyAccessorString { get; protected set; }
    public DictMode Mode => DictMode.KeyedValue;

    public override bool IsClass => true;
    public override bool HasDefault => false;
    public override bool IsEnumerable => true;
    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        if (deepCopy)
        {
            return $"{copyMaskAccessor}?.{Name}.Overall ?? true";
        }
        else
        {
            return $"{copyMaskAccessor}?.{Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
        }
    }
        
    public override bool CopyNeedsTryCatch => true;

    public override string TypeName(bool getter, bool needsCovariance = false) => $"ICache<{BackwardsTypeTuple(getter, needsCovariance)}>";

    public string TypeTuple(bool getter) => $"{KeyTypeGen.TypeName(getter)}, {ValueTypeGen.TypeName(getter)}";
    public string BackwardsTypeTuple(bool getter, bool needsCovariance = false) => $"{ValueTypeGen.TypeName(getter, needsCovariance)}, {KeyTypeGen.TypeName(getter, needsCovariance)}";

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        var keyedValNode = node.Element(XName.Get(Constants.KEYED_VALUE, LoquiGenerator.Namespace));
        if (keyedValNode == null)
        {
            throw new ArgumentException("Dict had no keyed value element.");
        }

        var valType = await ObjectGen.LoadField(
            keyedValNode.Elements().FirstOrDefault(),
            requireName: false,
            setDefaults: false);
        if (valType.Succeeded
            && valType.Value is LoquiType)
        {
            ValueTypeGen = valType.Value as LoquiType;
        }
        else
        {
            throw new NotImplementedException();
        }

        var keyAccessorAttr = keyedValNode.Attribute(XName.Get(Constants.KEY_ACCESSOR));
        if (keyAccessorAttr == null)
        {
            throw new ArgumentException("Dict had no key accessor attribute.");
        }

        KeyAccessorString = keyAccessorAttr.Value;
        if (ValueTypeGen.GenericDef == null)
        {
            await ValueTypeGen.TargetObjectGeneration.LoadingCompleteTask.Task;
            KeyTypeGen = ValueTypeGen.TargetObjectGeneration.IterateFields(includeBaseClass: true).FirstOrDefault((f) => f.Name.Equals(keyAccessorAttr.Value));
            if (KeyTypeGen == null)
            {
                throw new ArgumentException($"Dict had a key accessor attribute that didn't correspond to a field: {keyAccessorAttr.Value}");
            }
        }
        else
        {
            if (!keyedValNode.TryGetAttribute<string>(Constants.KEY_TYPE, out var keyTypeName))
            {
                throw new ArgumentException("Cannot have a generic keyed reference without manually specifying keyType");
            }
            if (!ObjectGen.ProtoGen.Gen.TryGetTypeGeneration(keyTypeName, out var keyTypeGen))
            {
                throw new ArgumentException($"Generic keyed type specification did not link to a known field type: {keyTypeName}");
            }
            KeyTypeGen = keyTypeGen;
        }
        await base.Resolve();

        if (KeyTypeGen is ContainerType
            || KeyTypeGen is DictType)
        {
            throw new NotImplementedException();
        }
    }

    public void AddMaskException(StructuredStringBuilder sb, string errorMaskAccessor, string exception, bool key)
    {
        LoquiType valueLoquiType = ValueTypeGen as LoquiType;
        var valStr = valueLoquiType == null ? "Exception" : $"Tuple<Exception, {valueLoquiType.TargetObjectGeneration.GetMaskString("Exception")}>";

        sb.AppendLine($"{errorMaskAccessor}?.{Name}.Value.Add({(key ? "null" : exception)});");
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        if (!ReadOnly)
        {
            sb.AppendLine($"{identifier}.Unset();");
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

    protected virtual string GetActualItemClass(bool getter)
    {
        if (Nullable)
        {
            return $"SetCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{KeyAccessorString})";
        }
        else
        {
            return $"Cache<{BackwardsTypeTuple(getter: false)}>((item) => item.{KeyAccessorString})";
        }
    }

    public string DictInterface(bool getter)
    {
        if (ReadOnly || getter)
        {
            if (Nullable)
            {
                throw new NotImplementedException();
            }
            else
            {
                return $"IReadOnlyCache<{BackwardsTypeTuple(getter)}>";
            }
        }
        else
        {
            if (Nullable)
            {
                return $"ISetCache<{BackwardsTypeTuple(getter)}>";
            }
            else
            {
                return $"ICache<{BackwardsTypeTuple(getter)}>";
            }
        }
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        sb.AppendLine($"private readonly {DictInterface(getter: false)} _{Name} = new {GetActualItemClass(getter: false)};");
        Comments?.Apply(sb, LoquiInterfaceType.Direct);
        sb.AppendLine($"public {DictInterface(getter: false)} {Name} => _{Name};");

        var member = $"_{Name}";
        using (sb.Region("Interface Members"))
        {
            if (!ReadOnly)
            {
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                sb.AppendLine($"{DictInterface(getter: false)} {ObjectGen.Interface(internalInterface: false)}.{Name} => {member};");
            }
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"{DictInterface(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: false)}.{Name} => {member};");
        }
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
        Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
        sb.AppendLine($"{(getter ? null : "new ")}{DictInterface(getter: getter)} {Name} {{ get; }}");
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
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                sb,
                () =>
                {
                    var loqui = ValueTypeGen as LoquiType;
                    if (Nullable)
                    {
                        using (var args = sb.Args(
                                   $"{accessor}.SetTo"))
                        {
                            args.Add($"rhs.{Name}");
                            args.Add((gen) =>
                            {
                                gen.AppendLine("(r) =>");
                                using (new CurlyBrace(gen))
                                {
                                    gen.AppendLine($"switch (copyMask?.{Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                    using (new CurlyBrace(gen))
                                    {
                                        gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                        using (gen.IncreaseDepth())
                                        {
                                            gen.AppendLine("return r;");
                                        }
                                        gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                        using (gen.IncreaseDepth())
                                        {
                                            loqui.GenerateTypicalMakeCopy(
                                                gen,
                                                retAccessor: $"return ",
                                                rhsAccessor: new Accessor("r"),
                                                copyMaskAccessor: copyMaskAccessor,
                                                deepCopy: deepCopy,
                                                doTranslationMask: false);
                                        }
                                        gen.AppendLine($"default:");
                                        using (gen.IncreaseDepth())
                                        {
                                            gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{Name}.Overall}}. Cannot execute copy.\");");
                                        }
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        using (var args = sb.Args(
                                   $"{accessor}.SetTo"))
                        {
                            args.Add((gen) =>
                            {
                                gen.AppendLine($"rhs.{Name}.Items");
                                using (gen.IncreaseDepth())
                                {
                                    gen.AppendLine(".Select((r) =>");
                                    using (new CurlyBrace(gen) { AppendParenthesis = true })
                                    {
                                        if (deepCopy)
                                        {
                                            loqui.GenerateTypicalMakeCopy(
                                                gen,
                                                retAccessor: $"return ",
                                                rhsAccessor: new Accessor("r"),
                                                copyMaskAccessor: copyMaskAccessor,
                                                deepCopy: deepCopy,
                                                doTranslationMask: false);
                                        }
                                        else
                                        {
                                            gen.AppendLine($"switch (copyMask?.{Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                            using (new CurlyBrace(gen))
                                            {
                                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                                using (gen.IncreaseDepth())
                                                {
                                                    gen.AppendLine("return r;");
                                                }
                                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                                using (gen.IncreaseDepth())
                                                {
                                                    loqui.GenerateTypicalMakeCopy(
                                                        gen,
                                                        retAccessor: $"return ",
                                                        rhsAccessor: new Accessor("r"),
                                                        copyMaskAccessor: copyMaskAccessor,
                                                        deepCopy: deepCopy,
                                                        doTranslationMask: false);
                                                }
                                                gen.AppendLine($"default:");
                                                using (gen.IncreaseDepth())
                                                {
                                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{Name}.Overall}}. Cannot execute copy.\");");
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    private void GenerateCopy(StructuredStringBuilder sb, string accessorPrefix, string rhsAccessorPrefix, bool protectedUse)
    {
        using (var args = sb.Args(
                   $"{accessorPrefix}.{GetName(protectedUse)}.SetTo"))
        {
            args.Add($"(IEnumerable<{ValueTypeGen.TypeName(getter: true)}>){rhsAccessorPrefix}.{GetName(false)}).Select((i) => i.Copy())");
        }
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        using (var args = sb.Args(
                   $"{accessor}.SetTo"))
        {
            args.Add($"(IEnumerable<{ValueTypeGen.TypeName(getter: true)}>){rhs}");
        }
        sb.AppendLine("break;");
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        sb.AppendLine($"return {identifier.Access};");
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessorPrefix}.Unset();");
        }
        else
        {
            sb.AppendLine($"{accessorPrefix}.Clear();");
        }
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
        if (!Nullable)
        {
            GenerateForEqualsMaskCheck(sb, $"item.{Name}", $"rhs.{Name}", $"ret.{Name}");
        }
        else
        {
            sb.AppendLine($"if ({NullableAccessor(getter: true, accessor: accessor)} == {NullableAccessor(getter: true, accessor: rhsAccessor)})");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"if ({NullableAccessor(getter: true, accessor: accessor)})");
                using (sb.CurlyBrace())
                {
                    GenerateForEqualsMaskCheck(sb, $"item.{Name}", $"rhs.{Name}", $"ret.{Name}");
                }
                sb.AppendLine($"else");
                using (sb.CurlyBrace())
                {
                    GenerateForEqualsMask(sb, $"ret.{Name}", true);
                }
            }
            sb.AppendLine($"else");
            using (sb.CurlyBrace())
            {
                GenerateForEqualsMask(sb, $"ret.{Name}", false);
            }
        }
    }

    public void GenerateForEqualsMaskCheck(StructuredStringBuilder sb, string accessor, string rhsAccessor, string retAccessor)
    {
        using (var args = sb.Args(
                   $"{retAccessor} = EqualsMaskHelper.CacheEqualsHelper"))
        {
            args.Add($"lhs: {accessor}");
            args.Add($"rhs: {rhsAccessor}");
            args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
            args.Add("include: include");
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
        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
        sb.AppendLine($"using (sb.IncreaseDepth())");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"foreach (var subItem in {accessor})");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"{sbAccessor}.{nameof(StructuredStringBuilder.AppendLine)}(\"[\");");
                sb.AppendLine($"using ({sbAccessor}.IncreaseDepth())");
                using (sb.CurlyBrace())
                {
                    ValueTypeGen.GenerateToString(sb, "Item", new Accessor("subItem.Value"), sbAccessor);
                }
                sb.AppendLine($"{sbAccessor}.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
            }
        }
        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendLine)}(\"]\");");
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        if (Nullable)
        {
            sb.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor}.HasBeenSet) return false;");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}