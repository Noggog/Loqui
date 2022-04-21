using System.Xml.Linq;

namespace Loqui.Generation;

public class ListType : ContainerType
{
    public override string TypeName(bool getter, bool needsCovariance = false) => ListTypeName(getter, internalInterface: true);
    public override bool CopyNeedsTryCatch => true;
    public override bool HasDefault => false;

    public virtual string ListTypeName(bool getter, bool internalInterface)
    {
        string itemTypeName = ItemTypeName(getter: getter);
        if (SubTypeGeneration is LoquiType loqui)
        {
            itemTypeName = loqui.TypeNameInternal(getter: getter, internalInterface: internalInterface);
        }
        if (ReadOnly || getter)
        {
            if (Notifying)
            {
                return $"IObservableList<{itemTypeName}{SubTypeGeneration.NullChar}>";
            }
            else
            {
                return $"IReadOnlyList<{itemTypeName}{SubTypeGeneration.NullChar}>";
            }
        }
        else
        {
            if (Notifying)
            {
                return $"SourceList<{itemTypeName}{SubTypeGeneration.NullChar}>";
            }
            else if (SubTypeGeneration is ByteArrayType)
            {
                return $"SliceList<byte>";
            }
            else
            {
                return $"ExtendedList<{itemTypeName}{SubTypeGeneration.NullChar}>";
            }
        }
    }

    public override async Task GenerateForClass(FileGeneration fg)
    {
        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        fg.AppendLine($"private {TypeName(getter: false)}{NullChar} _{Name}{(Nullable ? null : $" = {GetActualItemClass(ctor: true)}")};");
        Comments?.Apply(fg, LoquiInterfaceType.Direct);
        fg.AppendLine($"public {TypeName(getter: false)}{NullChar} {Name}");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"get => this._{Name};");
            fg.AppendLine($"{((ReadOnly || !Nullable) ? "init" : "set")} => this._{Name} = value;");
        }
        GenerateInterfaceMembers(fg, $"_{Name}");
    }

    protected virtual string GetActualItemClass(bool ctor = false)
    {
        if (NotifyingType == NotifyingType.ReactiveUI)
        {
            return $"new SourceList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
        }
        else if (SubTypeGeneration is ByteArrayType)
        {
            return $"new SliceList<byte>{(ctor ? "()" : null)}";
        }
        else
        {
            return $"new ExtendedList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
        }
    }

    public void GenerateInterfaceMembers(FileGeneration fg, string member)
    {
        using (new RegionWrapper(fg, "Interface Members"))
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"{ListTypeName(getter: true, internalInterface: true)}{NullChar} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => {member};");
        }
    }

    public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
    {
        if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
        if (getter)
        {
            Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            fg.AppendLine($"{ListTypeName(getter: true, internalInterface: true)}{NullChar} {Name} {{ get; }}");
        }
        else if (!ReadOnly)
        {
            Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            fg.AppendLine($"new {ListTypeName(getter: false, internalInterface: true)}{NullChar} {Name} {{ get; {(Nullable ? "set; " : null)}}}");
        }
    }

    public override string NullableAccessor(bool getter, Accessor accessor = null)
    {
        if (accessor == null)
        {
            return $"({Name} != null)";
        }
        else
        {
            return $"({accessor} != null)";
        }
    }

    public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
    {
        fg.AppendLine($"return {identifier.Access};");
    }

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        var loqui = SubTypeGeneration as LoquiType;
        if (!deepCopy
            && loqui != null
            && loqui.SupportsMask(MaskType.Copy))
        {
            return $"{copyMaskAccessor}?.{Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
        }
        else if (deepCopy
                 && loqui != null
                 && loqui.SupportsMask(MaskType.Translation))
        {
            return $"{copyMaskAccessor}?.{Name}.Overall ?? true";
        }
        else
        {
            if (deepCopy)
            {
                return $"{copyMaskAccessor}?.{Name} ?? true";
            }
            else
            {
                return $"{copyMaskAccessor}?.{Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            }
        }
    }

    public override void GenerateForCopy(
        FileGeneration fg,
        Accessor accessor,
        Accessor rhs,
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy)
    {
        void GenerateSet()
        {
            if (isLoquiSingle)
            {
                if (deepCopy)
                {
                    LoquiType loqui = SubTypeGeneration as LoquiType;
                    WrapSet(fg, accessor, (f) =>
                    {
                        f.AppendLine(rhs.ToString());
                        f.AppendLine(".Select(r =>");
                        using (new BraceWrapper(f) { AppendParenthesis = true })
                        {
                            loqui.GenerateTypicalMakeCopy(
                                f,
                                retAccessor: $"return ",
                                rhsAccessor: Accessor.FromType(loqui, "r"),
                                copyMaskAccessor: copyMaskAccessor,
                                deepCopy: deepCopy,
                                doTranslationMask: false);
                        }
                    });
                }
                else
                {
                    LoquiType loqui = SubTypeGeneration as LoquiType;
                    using (var args = new ArgsWrapper(fg,
                               $"{accessor}.SetTo<{SubTypeGeneration.TypeName(getter: false)}, {SubTypeGeneration.TypeName(getter: false)}>"))
                    {
                        args.Add($"items: {rhs}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine("converter: (r) =>");
                            using (new BraceWrapper(gen))
                            {
                                var supportsCopy = loqui.SupportsMask(MaskType.Copy);
                                var accessorStr = $"copyMask?.{Name}{(supportsCopy ? ".Overall" : string.Empty)}";
                                gen.AppendLine($"switch ({accessorStr} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                using (new BraceWrapper(gen))
                                {
                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                    using (new DepthWrapper(gen))
                                    {
                                        gen.AppendLine($"return ({loqui.TypeName()})r;");
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
                                        gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{accessorStr}}}. Cannot execute copy.\");");
                                    }
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                WrapSet(fg, accessor, (f) =>
                {
                    f.AppendLine($"rhs.{Name}");
                    SubTypeGeneration.GenerateCopySetToConverter(f);
                });
            }
        }

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
                    if (Nullable)
                    {
                        fg.AppendLine($"if ({NullableAccessor(getter: false, rhs)})");
                        using (new BraceWrapper(fg))
                        {
                            GenerateSet();
                        }
                        fg.AppendLine("else");
                        using (new BraceWrapper(fg))
                        {
                            GenerateClear(fg, accessor);
                        }
                    }
                    else
                    {
                        GenerateSet();
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
    {
        fg.AppendLine($"{accessor}.SetTo({rhs});");
        fg.AppendLine($"break;");
    }

    public virtual void WrapSet(FileGeneration fg, Accessor accessor, Action<FileGeneration> a)
    {
        if (Nullable)
        {
            fg.AppendLine($"{accessor} = ");
            using (new DepthWrapper(fg))
            {
                a(fg);
                fg.AppendLine($".ToExtendedList<{SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>();");
            }
        }
        else
        {
            using (var args = new ArgsWrapper(fg,
                       $"{accessor}.SetTo"))
            {
                args.Add(subFg => a(subFg));
            }
        }
    }

    public override void GenerateClear(FileGeneration fg, Accessor accessor)
    {
        if (Nullable)
        {
            fg.AppendLine($"{accessor} = null;");
        }
        else
        {
            fg.AppendLine($"{accessor}.Clear();");
        }
    }

    public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
    {
        fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"{name} =>\");");
        fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
        fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"foreach (var subItem in {accessor.Access})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                using (new BraceWrapper(fg))
                {
                    SubTypeGeneration.GenerateToString(fg, "Item", new Accessor("subItem"), fgAccessor);
                }
                fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
            }
        }
        fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
    }

    public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
    {
        if (Nullable)
        {
            fg.AppendLine($"if ({checkMaskAccessor}?.Overall.HasValue ?? false && {checkMaskAccessor}!.Overall.Value != {NullableAccessor(getter: true, accessor: accessor)}) return false;");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        return $"{accessor}{NullChar}.{Name}Select(x => {SubTypeGeneration.GetDuplicate("x")}).ToExtendedList()";
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        if (node.Name.LocalName == "RefList")
        {
            LoadTypeGenerationFromNode(node, requireName);
            SubTypeGeneration = ObjectGen.ProtoGen.Gen.GetTypeGeneration<LoquiType>();
            SubTypeGeneration.SetObjectGeneration(ObjectGen, setDefaults: false);
            await SubTypeGeneration.Load(node, false);
            NullableProperty.OnNext(SubTypeGeneration.NullableProperty.Value);
            SubTypeGeneration.NullableProperty.OnNext((false, false));
            SubTypeGeneration.Name = null;
            isLoquiSingle = SubTypeGeneration is LoquiType;
        }
        else
        {
            await base.Load(node, requireName);
        }
    }
}