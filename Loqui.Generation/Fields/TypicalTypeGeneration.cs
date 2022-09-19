using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public abstract class TypicalTypeGeneration : TypeGeneration
{
    public abstract Type Type(bool getter);
    public override string TypeName(bool getter, bool needsCovariance = false) => Type(getter).GetName();
    public string DefaultValue;
    public string DefaultValueMemberName => $"{Name}Default";
    public override bool HasDefault => !string.IsNullOrWhiteSpace(DefaultValue);
    public override string ProtectedName => $"{Name}";
    public event Action<StructuredStringBuilder> PreSetEvent;
    public event Action<StructuredStringBuilder> PostSetEvent;
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

    private void WrapSetCode(StructuredStringBuilder sb, Action<StructuredStringBuilder> toDo)
    {
        PreSetEvent?.Invoke(sb);
        toDo(sb);
        PostSetEvent?.Invoke(sb);
    }

    private void WrapSetAccessor(
        StructuredStringBuilder sb,
        string linePrefix,
        Action<StructuredStringBuilder> toDo)
    {
        StructuredStringBuilder subFg = new StructuredStringBuilder();
        WrapSetCode(subFg, toDo);
        if (subFg.Count > 1)
        {
            sb.AppendLine(linePrefix);
            using (sb.CurlyBrace())
            {
                sb.AppendLines(subFg);
            }
        }
        else if (subFg.Count > 0)
        {
            sb.AppendLine($"{linePrefix} => {subFg[0]}");
        }
        else
        {
            sb.AppendLine($"{linePrefix}");
        }
    }

    public virtual string GetValueSetString(Accessor accessor) => accessor.Access;

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        void GenerateTypicalNullableMembers(bool notifying)
        {
            Comments?.Apply(sb, LoquiInterfaceType.Direct);
            sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)}{NullChar} {Name} {{ get; {(ReadOnly ? "protected " : string.Empty)}set; }}{(HasDefault ? $" = {DefaultValueMemberName};" : null)}");
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"{TypeName(getter: true)}{NullChar} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
        }

        if (HasDefault)
        {
            Comments?.Apply(sb, LoquiInterfaceType.Direct);
            sb.AppendLine($"public static readonly {TypeName(getter: false)} {DefaultValueMemberName} = {DefaultValue};");
        }
        if (Nullable)
        {
            if (!TrueReadOnly)
            {
                GenerateTypicalNullableMembers(false);
            }
            else
            {
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public readonly {TypeName(getter: false)} {Name};");
                sb.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
        else
        {
            var subFg = new StructuredStringBuilder();
            WrapSetAccessor(subFg,
                linePrefix: $"{SetPermissionStr}set",
                toDo: subGen =>
                {
                    if (subGen.Count == 0) return;
                    subGen.AppendLine($"this._{Name} = value;");
                });
            if (subFg.Count > 1)
            {
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                sb.AppendLine($"private {TypeName(getter: false)} _{Name};");
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name}");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"get => this._{Name};");
                    sb.AppendLines(subFg);
                }
            }
            else if (subFg.Count == 1)
            {
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name} {{ get; {subFg[0]}; }} = {(HasDefault ? DefaultValueMemberName : GetDefault(getter: false))};");
            }
            else
            {
                throw new ArgumentException();
            }
            if (!InternalGetInterface && TypeName(getter: true) != TypeName(getter: false))
            {
                sb.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
        if (InternalSetInterface)
        {
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"{TypeName(getter: false)}{NullChar} {ObjectGen.Interface(getter: false, internalInterface: true)}.{Name}");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"get => this.{Name};");
                sb.AppendLine($"set => this.{Name} = {GetValueSetString("value")};");
            }
        }
        if (InternalGetInterface)
        {
            if (Nullable)
            {
                if (CanBeNullable(getter: true))
                {
                    sb.AppendLine($"{TypeName(getter: false)}? {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
                }
                else
                {
                    sb.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
                    sb.AppendLine($"bool {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name}_IsSet => this.{Name} != null");
                }
            }
            else
            {
                sb.AppendLine($"{TypeName(getter: false)} {ObjectGen.Interface(getter: true, internalInterface: true)}.{Name} => this.{Name}");
            }
        }
    }

    protected virtual string GenerateDefaultValue()
    {
        return DefaultValue;
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        if (getter)
        {
            if (!ApplicableInterfaceField(getter, internalInterface)) return;
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"{TypeName(getter: true)}{(Nullable && CanBeNullable(getter) ? "?" : null)} {Name} {{ get; }}");
            if (Nullable && !CanBeNullable(getter))
            {
                sb.AppendLine($"bool {Name}_IsSet {{ get; }}");
            }
        }
        else
        {
            if (!ApplicableInterfaceField(getter, internalInterface)) return;
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"new {TypeName(getter: false)}{(Nullable && CanBeNullable(getter) ? "?" : null)} {Name} {{ get; set; }}");

            if (!CanBeNullable(false))
            {
                if (Nullable)
                {
                    sb.AppendLine($"new bool {NullableAccessor(getter: false, accessor: new Accessor(Name))} {{ get; set; }}");
                    sb.AppendLine($"void {Name}_Set({TypeName(getter: false)} value, bool hasBeenSet = true);");
                    sb.AppendLine($"void {Name}_Unset();");
                    sb.AppendLine();
                }
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
        if (!IntegrateField) return;
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
                        sb.AppendLine($"if ({rhs} is {{}} item{Name})");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"{accessor.Access} = item{Name};");
                        }
                        sb.AppendLine("else");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"{accessor.Access} = default;");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{accessor.Access} = {rhs};");
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

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        if (!IntegrateField) return;
        sb.AppendLine($"{accessor} = {rhs};");
        sb.AppendLine($"break;");
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor identifier)
    {
        if (ReadOnly || !IntegrateField) return;
        // ToDo
        // Add internal interface support
        if (InternalSetInterface) return;
        if (HasDefault)
        {
            sb.AppendLine($"{identifier.Access} = {ObjectGen.ObjectName}.{DefaultValueMemberName};");
        }
        else if (Nullable)
        {
            sb.AppendLine($"{identifier.Access} = default;");
        }
        else
        {
            sb.AppendLine($"{identifier.Access} = {GetDefault(getter: false)};");
        }
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        GenerateClear(sb, identifier);
        sb.AppendLine("break;");
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        if (!IntegrateField) return;
        sb.AppendLine($"return {identifier.Access};");
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        if (!IntegrateField) return;
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"if ({GenerateEqualsSnippet(accessor, rhsAccessor, negate: true)}) return false;");
        }
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        if (!IntegrateField) return;
        // ToDo
        // Add Internal interface support
        if (InternalGetInterface) return;
        sb.AppendLine($"{retAccessor} = {GenerateEqualsSnippet(accessor.Access, rhsAccessor.Access)};");
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        if (!IntegrateField) return;
        var doIf = Nullable && CanBeNullable(getter: true);
        if (doIf)
        {
            sb.AppendLine($"if ({accessor} is {{}} {Name}item)");
            accessor = $"{Name}item";
        }
        using (sb.CurlyBrace(doIt: doIf))
        {
            sb.AppendLine($"{hashResultAccessor}.Add({accessor});");
        }
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        if (!IntegrateField) return;
        // ToDo
        // Add Internal interface support
        if (InternalGetInterface) return;
        sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendItem)}({accessor}{(string.IsNullOrWhiteSpace(Name) ? null : $", \"{Name}\"")});");
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        if (!IntegrateField) return;
        if (Nullable)
        {
            sb.AppendLine($"if ({checkMaskAccessor}.HasValue && {checkMaskAccessor}.Value != {NullableAccessor(getter: true, accessor: accessor)}) return false;");
        }
    }
}
