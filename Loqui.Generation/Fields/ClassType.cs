using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public abstract class ClassType : TypicalTypeGeneration
{
    public bool Singleton { get; set; }
    public bool Readonly;
    public override string ProtectedName => $"_{Name}";
    public override bool IsClass => true;
    public override bool IsNullable => Nullable && !Singleton;

    public abstract string GetNewForNonNullable();

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        Singleton = node.GetAttribute(Constants.SINGLETON, Singleton);
        ReadOnly = ReadOnly || Singleton;
    }

    protected override string GenerateDefaultValue()
    {
        if (Singleton
            && string.IsNullOrWhiteSpace(DefaultValue))
        {
            return GetNewForNonNullable();
        }
        return base.GenerateDefaultValue();
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        if (!IntegrateField) return;
        if (Nullable)
        {
            if (CanBeNullable(false))
            {
                if (!TrueReadOnly)
                {
                    sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    sb.AppendLine($"protected {TypeName(getter: false)}? _{Name};");
                    Comments?.Apply(sb, LoquiInterfaceType.Direct);
                    sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)}? {Name}");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"get => this._{ Name};");
                        sb.AppendLine($"{SetPermissionStr}set => this._{Name} = value;");
                    }
                    sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    if (CanBeNullable(getter: true))
                    {
                        sb.AppendLine($"{TypeName(getter: true)}? {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
                    }
                    else
                    {
                        sb.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
                        sb.AppendLine($"bool {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name}_IsSet => this.{Name} != null;");
                    }
                }
                else
                {
                    Comments?.Apply(sb, LoquiInterfaceType.Direct);
                    sb.AppendLine($"public readonly {TypeName(getter: false)}? {Name};");
                    sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"private {TypeName(getter: false)} _{Name}{(IsNullable ? string.Empty : $" = {GetNewForNonNullable()}")};");
            Comments?.Apply(sb, LoquiInterfaceType.Direct);
            if (Singleton)
            {
                sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name} => {ProtectedName};");
            }
            else
            {
                sb.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name}");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"get => {ProtectedName};");
                    sb.AppendLine($"{SetPermissionStr}set => this._{Name} = value;");
                }
            }
            if (TypeName(getter: true) != TypeName(getter: false))
            {
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                sb.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor identifier)
    {
        if (ReadOnly || !IntegrateField) return;
        // ToDo
        // Add internal interface support
        if (InternalSetInterface) return;
        if (Nullable)
        {
            sb.AppendLine($"{identifier.Access} = default;");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}