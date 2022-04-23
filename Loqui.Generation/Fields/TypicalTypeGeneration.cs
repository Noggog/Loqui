using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TypicalTypeGeneration : TypeGeneration
{
    public abstract Type Type(bool getter);
    public override string TypeName(bool getter, bool needsCovariance = false) => Type(getter).GetName();
    public string DefaultValue;
    public string DefaultValueMemberName => $"_{Name}_Default";
    public override bool HasDefault => !string.IsNullOrWhiteSpace(DefaultValue);
    public override string ProtectedName => $"{Name}";
    public event Action<FileGeneration> PreSetEvent;
    public event Action<FileGeneration> PostSetEvent;
    public override bool CopyNeedsTryCatch => false;

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        if (deepCopy)
        {
            return GetTranslationIfAccessor(copyMaskAccessor);
        }
        else
        {
            return $"{copyMaskAccessor}?.{Name} ?? true";
        }
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        node.TryGetAttribute("default", out DefaultValue);
    }

    private void WrapSetCode(FileGeneration fg, Action<FileGeneration> toDo)
    {
        PreSetEvent?.Invoke(fg);
        toDo(fg);
        PostSetEvent?.Invoke(fg);
    }

    private void WrapSetAccessor(
        FileGeneration fg,
        string linePrefix,
        Action<FileGeneration> toDo)
    {
        FileGeneration subFg = new FileGeneration();
        WrapSetCode(subFg, toDo);
        if (subFg.Count > 1)
        {
            fg.AppendLine(linePrefix);
            using (new BraceWrapper(fg))
            {
                fg.AppendLines(subFg);
            }
        }
        else if (subFg.Count > 0)
        {
            fg.AppendLine($"{linePrefix} => {subFg[0]}");
        }
        else
        {
            fg.AppendLine($"{linePrefix}");
        }
    }

    public virtual string GetValueSetString(Accessor accessor) => accessor.Access;

    public override async Task GenerateForClass(FileGeneration fg)
    {
        void GenerateTypicalNullableMembers(bool notifying)
        {
            Comments?.Apply(fg, LoquiInterfaceType.Direct);
            fg.AppendLine($"public {TypeName(getter: false)}{NullChar} {Name} {{ get; {(ReadOnly ? "protected " : string.Empty)}set; }}{(HasDefault ? $" = {DefaultValueMemberName};" : null)}");
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"{TypeName(getter: true)}{NullChar} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
        }

        if (HasDefault)
        {
            Comments?.Apply(fg, LoquiInterfaceType.Direct);
            fg.AppendLine($"public readonly static {TypeName(getter: false)} {DefaultValueMemberName} = {DefaultValue};");
        }
        if (Nullable)
        {
            if (!TrueReadOnly)
            {
                GenerateTypicalNullableMembers(false);
            }
            else
            {
                Comments?.Apply(fg, LoquiInterfaceType.Direct);
                fg.AppendLine($"public readonly {TypeName(getter: false)} {Name};");
                fg.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
        else
        {
            var subFg = new FileGeneration();
            WrapSetAccessor(subFg,
                linePrefix: $"{SetPermissionStr}set",
                toDo: subGen =>
                {
                    if (subGen.Count == 0) return;
                    subGen.AppendLine($"this._{Name} = value;");
                });
            if (subFg.Count > 1)
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"private {TypeName(getter: false)} _{Name};");
                Comments?.Apply(fg, LoquiInterfaceType.Direct);
                fg.AppendLine($"public {TypeName(getter: false)} {Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"get => this._{Name};");
                    fg.AppendLines(subFg);
                }
            }
            else if (subFg.Count == 1)
            {
                Comments?.Apply(fg, LoquiInterfaceType.Direct);
                fg.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name} {{ get; {subFg[0]}; }} = {(HasDefault ? DefaultValueMemberName : GetDefault(getter: false))};");
            }
            else
            {
                throw new ArgumentException();
            }
            if (!InternalGetInterface && TypeName(getter: true) != TypeName(getter: false))
            {
                fg.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
        if (InternalSetInterface)
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"{TypeName(getter: false)}{NullChar} {ObjectGen.Interface(getter: false, internalInterface: true)}.{Name}");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"get => this.{Name};");
                fg.AppendLine($"set => this.{Name} = {GetValueSetString("value")};");
            }
        }
        if (InternalGetInterface)
        {
            if (Nullable)
            {
                if (CanBeNullable(getter: true))
                {
                    fg.AppendLine($"{TypeName(getter: false)}? {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
                }
                else
                {
                    fg.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
                    fg.AppendLine($"bool {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name}_IsSet => this.{Name} != null");
                }
            }
            else
            {
                fg.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
            }
        }
    }

    protected virtual string GenerateDefaultValue()
    {
        return DefaultValue;
    }

    public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
    {
        if (getter)
        {
            if (!ApplicableInterfaceField(getter, internalInterface)) return;
            Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            fg.AppendLine($"{TypeName(getter: true)}{(Nullable && CanBeNullable(getter) ? "?" : null)} {Name} {{ get; }}");
            if (Nullable && !CanBeNullable(getter))
            {
                fg.AppendLine($"bool {Name}_IsSet {{ get; }}");
            }
        }
        else
        {
            if (!ApplicableInterfaceField(getter, internalInterface)) return;
            Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            fg.AppendLine($"new {TypeName(getter: false)}{(Nullable && CanBeNullable(getter) ? "?" : null)} {Name} {{ get; set; }}");

            if (!CanBeNullable(false))
            {
                if (Nullable)
                {
                    fg.AppendLine($"new bool {NullableAccessor(getter: false, accessor: new Accessor(Name))} {{ get; set; }}");
                    fg.AppendLine($"void {Name}_Set({TypeName(getter: false)} value, bool hasBeenSet = true);");
                    fg.AppendLine($"void {Name}_Unset();");
                    fg.AppendLine();
                }
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
        if (!IntegrateField) return;
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
                        fg.AppendLine($"if ({rhs} is {{}} item{Name})");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{accessor.Access} = item{Name};");
                        }
                        fg.AppendLine("else");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{accessor.Access} = default;");
                        }
                    }
                    else
                    {
                        fg.AppendLine($"{accessor.Access} = {rhs};");
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    public override string GenerateACopy(string rhsAccessor)
    {
        return rhsAccessor;
    }

    public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
    {
        if (!IntegrateField) return;
        fg.AppendLine($"{accessor} = {rhs};");
        fg.AppendLine($"break;");
    }

    public override void GenerateClear(FileGeneration fg, Accessor identifier)
    {
        if (ReadOnly || !IntegrateField) return;
        // ToDo
        // Add internal interface support
        if (InternalSetInterface) return;
        if (HasDefault)
        {
            fg.AppendLine($"{identifier.Access} = {ObjectGen.ObjectName}.{DefaultValueMemberName};");
        }
        else if (Nullable)
        {
            fg.AppendLine($"{identifier.Access} = default;");
        }
        else
        {
            fg.AppendLine($"{identifier.Access} = {GetDefault(getter: false)};");
        }
    }

    public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
    {
        GenerateClear(fg, identifier);
        fg.AppendLine("break;");
    }

    public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
    {
        if (!IntegrateField) return;
        fg.AppendLine($"return {identifier.Access};");
    }

    public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        if (!IntegrateField) return;
        fg.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"if ({GenerateEqualsSnippet(accessor, rhsAccessor, negate: true)}) return false;");
        }
    }

    public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        if (!IntegrateField) return;
        // ToDo
        // Add Internal interface support
        if (InternalGetInterface) return;
        fg.AppendLine($"{retAccessor} = {GenerateEqualsSnippet(accessor.Access, rhsAccessor.Access)};");
    }

    public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
    {
        if (!IntegrateField) return;
        var doIf = Nullable && CanBeNullable(getter: true);
        if (doIf)
        {
            fg.AppendLine($"if ({accessor} is {{}} {Name}item)");
            accessor = $"{Name}item";
        }
        using (new BraceWrapper(fg, doIt: doIf))
        {
            fg.AppendLine($"{hashResultAccessor}.Add({accessor});");
        }
    }

    public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
    {
        if (!IntegrateField) return;
        // ToDo
        // Add Internal interface support
        if (InternalGetInterface) return;
        fg.AppendLine($"fg.{nameof(FileGeneration.AppendItem)}({accessor}{(string.IsNullOrWhiteSpace(Name) ? null : $", \"{Name}\"")});");
    }

    public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
    {
        if (!IntegrateField) return;
        if (Nullable)
        {
            fg.AppendLine($"if ({checkMaskAccessor}.HasValue && {checkMaskAccessor}.Value != {NullableAccessor(getter: true, accessor: accessor)}) return false;");
        }
    }
}
