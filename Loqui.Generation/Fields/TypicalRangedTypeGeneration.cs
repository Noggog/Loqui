using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypicalRangedTypeGeneration : PrimitiveType
    {
        public string Min;
        public string Max;
        public bool RangeThrowException;
        public bool HasRange;

        public virtual string RangeTypeName => $"Range{this.TypeName.TrimEnd("?")}";
        public string RangeMemberName => $"{this.Name}_Range";

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            if (node.GetAttribute(Constants.MIN) != null)
            {
                HasRange = node.TryGetAttribute(Constants.MIN, out Min);
            }
            if (node.GetAttribute(Constants.MAX) != null)
            {
                HasRange = node.TryGetAttribute(Constants.MAX, out Max);
            }
            RangeThrowException = node.GetAttribute<bool>(Constants.RANGE_THROW_EXCEPTION, false);
        }

        protected string InRangeCheckerString => $"{(this.IsNullable() ? "?" : string.Empty)}.{(this.RangeThrowException ? "" : "Put")}InRange({RangeMemberName}.Min, {RangeMemberName}.Max)";

        public override void GenerateForClass(FileGeneration fg)
        {
            if (!this.HasRange)
            {
                base.GenerateForClass(fg);
                return;
            }
            if (this.NotifyingType == NotifyingType.NotifyingItem)
            {
                if (this.ObjectCentralized)
                {
                    fg.AppendLine($"protected {base.TypeName} _{this.Name};");
                    if (base.HasDefault)
                    {
                        fg.AppendLine($"protected readonly static {base.TypeName} _{this.Name}_Default = {this.DefaultValue};");
                    }
                    fg.AppendLine($"protected PropertyForwarder<{this.ObjectGen.ObjectName}, {base.TypeName}> _{this.Name}Forwarder;");
                    fg.AppendLine($"public {(ReadOnly ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{base.TypeName}> {this.Property} => _{this.Name}Forwarder ?? (_{this.Name}Forwarder = new PropertyForwarder<{this.ObjectGen.ObjectName}, {base.TypeName}>(this, (int){this.ObjectCentralizationEnumName}));");
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{this.Name};");
                        fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this.Set{this.Name}(value);");
                    }
                    using (var args = new FunctionWrapper(fg,
                        $"protected void Set{this.Name}"))
                    {
                        args.Add($"{this.TypeName} item");
                        args.Add($"bool hasBeenSet = true");
                        args.Add($"NotifyingFireParameters cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"item = item{InRangeCheckerString};");
                        fg.AppendLine($"var oldHasBeenSet = _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                        if (this.IsClass)
                        {
                            fg.AppendLine($"if ((cmds?.ForceFire ?? true) && oldHasBeenSet == hasBeenSet && object.Equals({this.Name}, item)) return;");
                        }
                        else
                        {
                            fg.AppendLine($"if ((cmds?.ForceFire ?? true) && oldHasBeenSet == hasBeenSet && {this.ProtectedName} == item) return;");
                        }
                        fg.AppendLine("if (oldHasBeenSet != hasBeenSet)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = hasBeenSet;");
                        }
                        fg.AppendLine($"if (_{Utility.MemberNameSafety(this.TypeName)}_subscriptions != null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"var tmp = {this.Name};");
                            fg.AppendLine($"_{this.Name} = item;");
                            using (var args = new ArgsWrapper(fg,
                                $"_{Utility.MemberNameSafety(this.TypeName)}_subscriptions.FireSubscriptions"))
                            {
                                args.Add($"index: (int){this.ObjectCentralizationEnumName}");
                                args.Add("oldHasBeenSet: oldHasBeenSet");
                                args.Add("newHasBeenSet: hasBeenSet");
                                args.Add($"oldVal: tmp");
                                args.Add($"newVal: item");
                                args.Add($"cmds: cmds");
                            }
                        }
                        fg.AppendLine("else");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"_{this.Name} = item;");
                        }
                    }
                    using (var args = new FunctionWrapper(fg,
                        $"protected void Unset{this.Name}"))
                    {
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = false;");
                        fg.AppendLine($"{this.Name} = {(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName})")};");
                    }
                    if (!this.ReadOnly)
                    {
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"INotifying{(this.HasBeenSet ? "Set" : null)}Item<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"INotifying{(this.HasBeenSet ? "Set" : null)}ItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                }
                else
                {
                    if (this.HasBeenSet)
                    {
                        if (this.RaisePropertyChanged)
                        {
                            fg.AppendLine($"protected readonly INotifyingSetItem<{TypeName}> _{this.Name};");
                        }
                        else
                        {
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public {(ReadOnly ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                        }
                        if (!this.ReadOnly)
                        {
                            fg.AppendLine($"INotifyingSetItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                        }
                        fg.AppendLine($"INotifyingSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    else
                    {
                        if (this.RaisePropertyChanged)
                        {
                            fg.AppendLine($"protected readonly INotifyingItem<{TypeName}> _{this.Name};");
                        }
                        else
                        {
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public {(ReadOnly ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                        }
                        if (!this.ReadOnly)
                        {
                            fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                        }
                        fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    if (!this.TrueReadOnly)
                    {
                        if (this.RaisePropertyChanged)
                        {
                            fg.AppendLine($"protected readonly IHasBeenSetItem<{base.TypeName}> _{this.Name};");
                        }
                        else
                        {
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public IHasBeenSetItem<{this.TypeName}> {this.Property} => _{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                        }
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName} {this.Name};");
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => HasBeenSetGetter.NotBeenSet_Instance;");
                    }
                }
                else
                {
                    fg.AppendLine($"private {base.TypeName} _{this.Name};");
                    fg.AppendLine($"public {base.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => _{this.Name};");
                        fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"this._{ this.Name} = value{InRangeCheckerString};");
                            if (this.RaisePropertyChanged)
                            {
                                fg.AppendLine($"OnPropertyChanged(nameof({this.Name}));");
                            }
                        }
                    }
                }
            }

            if (this.HasRange)
            {
                fg.AppendLine($"public static {this.RangeTypeName} {RangeMemberName} = new {this.RangeTypeName}({Min}, {Max});");
            }
        }
    }
}
