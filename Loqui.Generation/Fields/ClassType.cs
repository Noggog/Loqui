using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class ClassType : TypicalTypeGeneration
    {
        public SingletonLevel Singleton;
        public bool Readonly;
        public override bool Copy => base.Copy && this.Singleton != SingletonLevel.Singleton;
        public override string ProtectedName => this.NotifyingType == NotifyingType.NotifyingItem ? $"{this.ProtectedProperty}.Item" : $"_{this.Name}";
        public override bool IsClass => true;

        public abstract string GetNewForNonNullable();

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Singleton = node.GetAttribute<SingletonLevel>(Constants.SINGLETON, this.Singleton);
            this.ReadOnly = this.ReadOnly || this.Singleton == SingletonLevel.Singleton;
        }

        protected override string GenerateDefaultValue()
        {
            if (this.Singleton != SingletonLevel.None
                && string.IsNullOrWhiteSpace(this.DefaultValue))
            {
                return GetNewForNonNullable();
            }
            return base.GenerateDefaultValue();
        }

        protected override IEnumerable<string> GenerateNotifyingConstructionParameters()
        {
            foreach (var arg in base.GenerateNotifyingConstructionParameters())
            {
                if (arg.Contains("markAsSet")) continue;
                yield return arg;
            }
            if (this.HasBeenSet)
            {
                yield return $"markAsSet: {(this.Singleton == SingletonLevel.Singleton ? "true" : "false")}";
            }
            if (this.Singleton != SingletonLevel.None)
            {
                yield return $"noNullFallback: () => {GetNewForNonNullable()}";
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            if (this.NotifyingType == NotifyingType.NotifyingItem)
            {
                if (this.ObjectCentralized)
                {
                    if (this.Singleton == SingletonLevel.None)
                    {
                        fg.AppendLine($"protected {base.TypeName} _{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"protected {base.TypeName} _{this.Name} = {GetNewForNonNullable()};");
                    }
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
                        if (this.Singleton == SingletonLevel.NotNull)
                        {
                            fg.AppendLine("if (item == null)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"item = {GetNewForNonNullable()};");
                            }
                        }
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
                        using (var args = new ArgsWrapper(fg,
                            $"Set{this.Name}"))
                        {
                            args.Add($"item: {(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName})")}");
                            args.Add($"hasBeenSet: false");
                        }
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
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                        }
                        if (!this.ReadOnly)
                        {
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"INotifyingSetItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
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
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                        }
                        if (!this.ReadOnly)
                        {
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
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
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName} {this.Name};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => HasBeenSetGetter.NotBeenSet_Instance;");
                    }
                }
                else
                {
                    fg.AppendLine($"private {base.TypeName} _{this.Name}{(this.Singleton == SingletonLevel.None ? string.Empty : $" = {GetNewForNonNullable()}")};");
                    fg.AppendLine($"public {base.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => {this.ProtectedName};");
                        if (this.Singleton == SingletonLevel.None)
                        {
                            fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set {{ this._{this.Name} = value;{(this.RaisePropertyChanged ? $" OnPropertyChanged(nameof({this.Name}));" : string.Empty)} }}");
                        }
                        else
                        {
                            fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"this.{this.ProtectedName} = value;");
                                fg.AppendLine("if (value == null)");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine($"this.{this.ProtectedName} = {this.GetNewForNonNullable()};");
                                }
                                if (this.RaisePropertyChanged)
                                {
                                    fg.AppendLine($"OnPropertyChanged(nameof({this.Name}));");
                                }
                            }
                        }
                    }
                }
            }
        }

        public override bool IsNullable()
        {
            return this.Singleton == SingletonLevel.None;
        }
    }
}
