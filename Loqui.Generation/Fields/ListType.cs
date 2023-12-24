using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

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
            return $"IReadOnlyList<{itemTypeName}{SubTypeGeneration.NullChar}>";
        }
        else
        {
            if (SubTypeGeneration is ByteArrayType)
            {
                return $"SliceList<byte>";
            }
            else
            {
                return $"ExtendedList<{itemTypeName}{SubTypeGeneration.NullChar}>";
            }
        }
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        sb.AppendLine($"private {TypeName(getter: false)}{NullChar} _{Name}{(Nullable ? null : $" = {GetActualItemClass(ctor: true)}")};");
        Comments?.Apply(sb, LoquiInterfaceType.Direct);
        sb.AppendLine($"public {TypeName(getter: false)}{NullChar} {Name}");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"get => this._{Name};");
            sb.AppendLine($"{((ReadOnly || !Nullable) ? "init" : "set")} => this._{Name} = value;");
        }
        GenerateInterfaceMembers(sb, $"_{Name}");
    }

    protected virtual string GetActualItemClass(bool ctor = false)
    {
        if (SubTypeGeneration is ByteArrayType)
        {
            return $"new SliceList<byte>{(ctor ? "()" : null)}";
        }
        else
        {
            return $"new ExtendedList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
        }
    }

    public void GenerateInterfaceMembers(StructuredStringBuilder sb, string member)
    {
        using (sb.Region("Interface Members"))
        {
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"{ListTypeName(getter: true, internalInterface: true)}{NullChar} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => {member};");
        }
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
        if (getter)
        {
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"{ListTypeName(getter: true, internalInterface: true)}{NullChar} {Name} {{ get; }}");
        }
        else if (!ReadOnly)
        {
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"new {ListTypeName(getter: false, internalInterface: true)}{NullChar} {Name} {{ get; {(Nullable ? "set; " : null)}}}");
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

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        sb.AppendLine($"return {identifier.Access};");
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
        StructuredStringBuilder sb,
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
                    WrapSet(sb, accessor, (f) =>
                    {
                        f.AppendLine(rhs.ToString());
                        f.AppendLine(".Select(r =>");
                        using (new CurlyBrace(f) { AppendParenthesis = true })
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
                    using (var args = sb.Call(
                               $"{accessor}.SetTo<{SubTypeGeneration.TypeName(getter: false)}, {SubTypeGeneration.TypeName(getter: false)}>"))
                    {
                        args.Add($"items: {rhs}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine("converter: (r) =>");
                            using (new CurlyBrace(gen))
                            {
                                var supportsCopy = loqui.SupportsMask(MaskType.Copy);
                                var accessorStr = $"copyMask?.{Name}{(supportsCopy ? ".Overall" : string.Empty)}";
                                gen.AppendLine($"switch ({accessorStr} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                using (new CurlyBrace(gen))
                                {
                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                    using (gen.IncreaseDepth())
                                    {
                                        gen.AppendLine($"return ({loqui.TypeName()})r;");
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
                WrapSet(sb, accessor, (f) =>
                {
                    f.AppendLine($"rhs.{Name}");
                    SubTypeGeneration.GenerateCopySetToConverter(f);
                });
            }
        }

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
                    if (Nullable)
                    {
                        sb.AppendLine($"if ({NullableAccessor(getter: false, rhs)})");
                        using (sb.CurlyBrace())
                        {
                            GenerateSet();
                        }
                        sb.AppendLine("else");
                        using (sb.CurlyBrace())
                        {
                            GenerateClear(sb, accessor);
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

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        sb.AppendLine($"{accessor}.SetTo({rhs});");
        sb.AppendLine($"break;");
    }

    public virtual void WrapSet(StructuredStringBuilder sb, Accessor accessor, Action<StructuredStringBuilder> a)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessor} = ");
            using (sb.IncreaseDepth())
            {
                a(sb);
                sb.AppendLine($".ToExtendedList<{SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>();");
            }
        }
        else
        {
            using (var args = sb.Call(
                       $"{accessor}.SetTo"))
            {
                args.Add(subFg => a(subFg));
            }
        }
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessor)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessor} = null;");
        }
        else
        {
            sb.AppendLine($"{accessor}.Clear();");
        }
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        sb.AppendLine($"{sbAccessor}.{nameof(StructuredStringBuilder.AppendLine)}(\"{name} =>\");");
        sb.AppendLine($"using ({sbAccessor}.Brace())");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"foreach (var subItem in {accessor.Access})");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"using ({sbAccessor}.Brace())");
                using (sb.CurlyBrace())
                {
                    SubTypeGeneration.GenerateToString(sb, "Item", new Accessor("subItem"), sbAccessor);
                }
            }
        }
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        if (Nullable)
        {
            sb.AppendLine($"if ({checkMaskAccessor}?.Overall.HasValue ?? false && {checkMaskAccessor}!.Overall.Value != {NullableAccessor(getter: true, accessor: accessor)}) return false;");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        return $"{accessor}{NullChar}.{Name}Select(x => {SubTypeGeneration.GetDuplicate("x")}).ToExtendedList<{SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>()";
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