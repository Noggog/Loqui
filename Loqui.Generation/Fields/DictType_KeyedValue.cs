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

    public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception, bool key)
    {
        LoquiType valueLoquiType = ValueTypeGen as LoquiType;
        var valStr = valueLoquiType == null ? "Exception" : $"Tuple<Exception, {valueLoquiType.TargetObjectGeneration.GetMaskString("Exception")}>";

        fg.AppendLine($"{errorMaskAccessor}?.{Name}.Value.Add({(key ? "null" : exception)});");
    }

    public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
    {
        if (!ReadOnly)
        {
            fg.AppendLine($"{identifier}.Unset();");
        }
        fg.AppendLine("break;");
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
        if (NotifyingType == NotifyingType.ReactiveUI)
        {
            if (Nullable)
            {
                return $"SourceSetCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{KeyAccessorString})";
            }
            else
            {
                return $"SourceCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{KeyAccessorString})";
            }
        }
        else
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
    }

    public string DictInterface(bool getter)
    {
        if (ReadOnly || getter)
        {
            if (Notifying)
            {
                if (Nullable)
                {
                    return $"IObservableSetCache<{BackwardsTypeTuple(getter)}>";
                }
                else
                {
                    return $"IObservableCache<{BackwardsTypeTuple(getter)}>";
                }
            }
            else
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
        }
        else
        {
            if (Notifying)
            {
                if (Nullable)
                {
                    return $"ISourceSetCache<{BackwardsTypeTuple(getter)}>";
                }
                else
                {
                    return $"ISourceCache<{BackwardsTypeTuple(getter)}>";
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
    }

    public override async Task GenerateForClass(FileGeneration fg)
    {
        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        fg.AppendLine($"private readonly {DictInterface(getter: false)} _{Name} = new {GetActualItemClass(getter: false)};");
        Comments?.Apply(fg, LoquiInterfaceType.Direct);
        fg.AppendLine($"public {DictInterface(getter: false)} {Name} => _{Name};");

        var member = $"_{Name}";
        using (new RegionWrapper(fg, "Interface Members"))
        {
            if (!ReadOnly)
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"{DictInterface(getter: false)} {ObjectGen.Interface(internalInterface: false)}.{Name} => {member};");
            }
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"{DictInterface(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: false)}.{Name} => {member};");
        }
    }

    public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
    {
        if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
        Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
        fg.AppendLine($"{(getter ? null : "new ")}{DictInterface(getter: getter)} {Name} {{ get; }}");
    }

    public override void GenerateForCopy(
        FileGeneration fg,
        Accessor accessor,
        Accessor rhs, 
        Accessor copyMaskAccessor,
        bool protectedMembers, 
        bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            fg.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (new BraceWrapper(fg, doIt: !AlwaysCopy))
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                fg,
                () =>
                {
                    var loqui = ValueTypeGen as LoquiType;
                    if (Nullable)
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{accessor}.SetTo"))
                        {
                            args.Add($"rhs.{Name}");
                            args.Add((gen) =>
                            {
                                gen.AppendLine("(r) =>");
                                using (new BraceWrapper(gen))
                                {
                                    gen.AppendLine($"switch (copyMask?.{Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                    using (new BraceWrapper(gen))
                                    {
                                        gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                        using (new DepthWrapper(gen))
                                        {
                                            gen.AppendLine("return r;");
                                        }
                                        gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                        using (new DepthWrapper(gen))
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
                                        using (new DepthWrapper(gen))
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
                        using (var args = new ArgsWrapper(fg,
                                   $"{accessor}.SetTo"))
                        {
                            args.Add((gen) =>
                            {
                                gen.AppendLine($"rhs.{Name}.Items");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine(".Select((r) =>");
                                    using (new BraceWrapper(gen) { AppendParenthesis = true })
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
                                            using (new BraceWrapper(gen))
                                            {
                                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine("return r;");
                                                }
                                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                                using (new DepthWrapper(gen))
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
                                                using (new DepthWrapper(gen))
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

    private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool protectedUse)
    {
        using (var args = new ArgsWrapper(fg,
                   $"{accessorPrefix}.{GetName(protectedUse)}.SetTo"))
        {
            args.Add($"(IEnumerable<{ValueTypeGen.TypeName(getter: true)}>){rhsAccessorPrefix}.{GetName(false)}).Select((i) => i.Copy())");
        }
    }

    public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
    {
        using (var args = new ArgsWrapper(fg,
                   $"{accessor}.SetTo"))
        {
            args.Add($"(IEnumerable<{ValueTypeGen.TypeName(getter: true)}>){rhs}");
        }
        fg.AppendLine("break;");
    }

    public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
    {
        fg.AppendLine($"return {identifier.Access};");
    }

    public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
    {
        if (Nullable)
        {
            fg.AppendLine($"{accessorPrefix}.Unset();");
        }
        else
        {
            fg.AppendLine($"{accessorPrefix}.Clear();");
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

    public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        fg.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
        }
    }

    public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        if (!Nullable)
        {
            GenerateForEqualsMaskCheck(fg, $"item.{Name}", $"rhs.{Name}", $"ret.{Name}");
        }
        else
        {
            fg.AppendLine($"if ({NullableAccessor(getter: true, accessor: accessor)} == {NullableAccessor(getter: true, accessor: rhsAccessor)})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if ({NullableAccessor(getter: true, accessor: accessor)})");
                using (new BraceWrapper(fg))
                {
                    GenerateForEqualsMaskCheck(fg, $"item.{Name}", $"rhs.{Name}", $"ret.{Name}");
                }
                fg.AppendLine($"else");
                using (new BraceWrapper(fg))
                {
                    GenerateForEqualsMask(fg, $"ret.{Name}", true);
                }
            }
            fg.AppendLine($"else");
            using (new BraceWrapper(fg))
            {
                GenerateForEqualsMask(fg, $"ret.{Name}", false);
            }
        }
    }

    public void GenerateForEqualsMaskCheck(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
    {
        using (var args = new ArgsWrapper(fg,
                   $"{retAccessor} = EqualsMaskHelper.CacheEqualsHelper"))
        {
            args.Add($"lhs: {accessor}");
            args.Add($"rhs: {rhsAccessor}");
            args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
            args.Add("include: include");
        }
    }

    public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
    {
        fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool", getter: true)}();");
        fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
    }

    public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
    {
        fg.AppendLine($"{hashResultAccessor}.Add({accessor});");
    }

    public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
    {
        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"{name} =>\");");
        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
        fg.AppendLine($"using (new DepthWrapper(fg))");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"foreach (var subItem in {accessor})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                using (new BraceWrapper(fg))
                {
                    ValueTypeGen.GenerateToString(fg, "Item", new Accessor("subItem.Value"), fgAccessor);
                }
                fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
            }
        }
        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
    }

    public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
    {
        if (Nullable)
        {
            fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor}.HasBeenSet) return false;");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}