using Noggog;
using System.Xml.Linq;

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

    public override async Task GenerateForClass(FileGeneration fg)
    {
        if (!IntegrateField) return;
        if (Nullable)
        {
            if (CanBeNullable(false))
            {
                if (!TrueReadOnly)
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"protected {TypeName(getter: false)}? _{Name};");
                    Comments?.Apply(fg, LoquiInterfaceType.Direct);
                    fg.AppendLine($"public {OverrideStr}{TypeName(getter: false)}? {Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ Name};");
                        fg.AppendLine($"{SetPermissionStr}set => this._{Name} = value;");
                    }
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    if (CanBeNullable(getter: true))
                    {
                        fg.AppendLine($"{TypeName(getter: true)}? {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
                    }
                    else
                    {
                        fg.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
                        fg.AppendLine($"bool {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name}_IsSet => this.{Name} != null;");
                    }
                }
                else
                {
                    Comments?.Apply(fg, LoquiInterfaceType.Direct);
                    fg.AppendLine($"public readonly {TypeName(getter: false)}? {Name};");
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"private {TypeName(getter: false)} _{Name}{(IsNullable ? string.Empty : $" = {GetNewForNonNullable()}")};");
            Comments?.Apply(fg, LoquiInterfaceType.Direct);
            if (Singleton)
            {
                fg.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name} => {ProtectedName};");
            }
            else
            {
                fg.AppendLine($"public {OverrideStr}{TypeName(getter: false)} {Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"get => {ProtectedName};");
                    fg.AppendLine($"{SetPermissionStr}set => this._{Name} = value;");
                }
            }
            if (TypeName(getter: true) != TypeName(getter: false))
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, InternalGetInterface)}.{Name} => this.{Name};");
            }
        }
    }

    public override void GenerateClear(FileGeneration fg, Accessor identifier)
    {
        if (ReadOnly || !IntegrateField) return;
        // ToDo
        // Add internal interface support
        if (InternalSetInterface) return;
        if (Nullable)
        {
            fg.AppendLine($"{identifier.Access} = default;");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}