using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypicalTypeGeneration : TypeGeneration
    {
        public abstract Type Type { get; }
        public override string TypeName => Type.GetName();
        public string DefaultValue;
        public override bool HasDefault => !string.IsNullOrWhiteSpace(DefaultValue);
        public override string ProtectedProperty => "_" + this.Name;
        public override string ProtectedName
        {
            get
            {
                if (this.ObjectCentralized)
                {
                    return $"{this.Name}";
                }
                else if (this.Bare)
                {
                    return $"_{this.Name}";
                }
                else
                {
                    return $"{this.ProtectedProperty}.Item";
                }
            }
        }

        public override bool CopyNeedsTryCatch => !this.Bare;

        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name} ?? true";

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            node.TryGetAttribute("default", out DefaultValue);
        }

        public override void GenerateForCtor(FileGeneration fg)
        {
            base.GenerateForCtor(fg);

            if (!this.Bare
                && !this.TrueReadOnly
                && this.RaisePropertyChanged)
            {
                GenerateNotifyingConstruction(fg, $"_{this.Name}");
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.NotifyingType == NotifyingType.NotifyingItem)
            {
                if (!this.ObjectCentralized)
                {
                    if (this.HasBeenSet)
                    {
                        if (this.RaisePropertyChanged)
                        {
                            fg.AppendLine($"protected readonly INotifyingSetItem<{TypeName}> _{this.Name};");
                        }
                        else
                        {
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public {(ReadOnly ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name}.Item;");
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
                            fg.AppendLine($"protected INotifyingItem<{TypeName}> _{this.Name};");
                        }
                        else
                        {
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
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
                else
                {
                    fg.AppendLine($"protected {TypeName} _{this.Name};");
                    if (HasDefault)
                    {
                        fg.AppendLine($"protected readonly static {TypeName} _{this.Name}_Default = {this.DefaultValue};");
                    }
                    fg.AppendLine($"protected PropertyForwarder<{this.ObjectGen.ObjectName}, {TypeName}> _{this.Name}Forwarder;");
                    fg.AppendLine($"public {(ReadOnly ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{TypeName}> {this.Property} => _{this.Name}Forwarder ?? (_{this.Name}Forwarder = new PropertyForwarder<{this.ObjectGen.ObjectName}, {TypeName}>(this, (int){this.ObjectCentralizationEnumName}));");
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
            }
            else
            {
                if (this.HasBeenSet)
                {
                    if (this.ObjectCentralized)
                    {
                        throw new NotImplementedException();
                    }
                    if (!this.TrueReadOnly)
                    {
                        if (this.RaisePropertyChanged)
                        {
                            fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName}> _{this.Name};");
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
                            fg.AppendLine($"get => this._{this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
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
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"private {TypeName} _{this.Name};");
                        fg.AppendLine($"public {TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name};");
                            fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set {{ this._{this.Name} = value; OnPropertyChanged(nameof({this.Name})); }}");
                        }
                    }
                    else
                    {
                        fg.AppendLine($"private {TypeName} _{this.Name};");
                        fg.AppendLine($"public {TypeName} {this.Name} {{ get => _{this.Name}; {(this.ReadOnly ? "protected " : string.Empty)}set => _{this.Name} = value; }}");
                    }
                }
            }
        }

        protected string GetNotifyingProperty()
        {
            string item;
            if (this.NotifyingType == NotifyingType.NotifyingItem)
            {
                if (this.HasBeenSet)
                {
                    item = "NotifyingSetItem";
                }
                else
                {
                    item = "NotifyingItem";
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    item = "HasBeenSetItem";
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return $"protected I{item}<{TypeName}> _{this.Name}";
        }

        protected void GenerateNotifyingCtor(FileGeneration fg)
        {
            GenerateNotifyingConstruction(fg, GetNotifyingProperty());
        }

        protected virtual IEnumerable<string> GenerateNotifyingConstructionParameters()
        {
            if (this.RaisePropertyChanged)
            {
                yield return $"onSet: (i) => this.OnPropertyChanged(nameof({this.Name}))";
            }
            if (HasDefault)
            {
                yield return $"defaultVal: {GenerateDefaultValue()}";
            }
            if (this.HasBeenSet)
            {
                yield return "markAsSet: false";
            }
        }

        protected virtual void GenerateNotifyingConstruction(FileGeneration fg, string prepend)
        {
            if (!this.IntegrateField) return;
            string item;
            if (this.NotifyingType == NotifyingType.NotifyingItem)
            {
                if (this.HasBeenSet)
                {
                    item = "NotifyingSetItem";
                }
                else
                {
                    item = "NotifyingItem";
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    item = "HasBeenSetItem";
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            using (var args = new ArgsWrapper(fg,
                $"{prepend} = {item}.Factory<{TypeName}>"))
            {
                foreach (var arg in GenerateNotifyingConstructionParameters())
                {
                    args.Add(arg);
                }
            }
        }

        protected virtual string GenerateDefaultValue()
        {
            return this.DefaultValue;
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            fg.AppendLine($"new {TypeName} {this.Name} {{ get; set; }}");
            if (this.NotifyingType != NotifyingType.None)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"new INotifyingSetItem{(this.ReadOnly ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
                }
                else
                {
                    fg.AppendLine($"new INotifyingItem{(this.ReadOnly ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"new IHasBeenSetItem{(this.ReadOnly ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
                }
            }
            fg.AppendLine();
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"{TypeName} {this.Name} {{ get; }}");
            if (this.NotifyingType != NotifyingType.None)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"INotifyingSetItemGetter<{TypeName}> {this.Property} {{ get; }}");
                }
                else
                {
                    fg.AppendLine($"INotifyingItemGetter<{TypeName}> {this.Property} {{ get; }}");
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"IHasBeenSetItemGetter<{TypeName}> {this.Property} {{ get; }}");
                }
            }
            fg.AppendLine();
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {
            if (!this.IntegrateField) return;
            if (!this.HasProperty)
            {
                fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)};");
                return;
            }
            if (this.HasBeenSet)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessor.PropertyAccess}.SetToWithDefault"))
                {
                    args.Add($"rhs: {rhsAccessorPrefix}.{this.GetName(false, true)}");
                    args.Add($"def: {defaultFallbackAccessor}?.{this.GetName(false, true)}");
                    if (this.NotifyingType == NotifyingType.NotifyingItem
                        && !this.ObjectCentralized)
                    {
                        args.Add($"cmds: {cmdsAccessor}");
                    }
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessor.PropertyAccess}.Set"))
                {
                    args.Add($"value: {rhsAccessorPrefix}.{this.GetName(false, false)}");
                    if (this.NotifyingType != NotifyingType.None)
                    {
                        args.Add($"cmds: {cmdsAccessor}");
                    }
                }
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return rhsAccessor;
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            if (!this.IntegrateField) return;
            if (this.Bare)
            {
                fg.AppendLine($"{accessorPrefix}.{this.ProtectedName} = {rhsAccessorPrefix};");
            }
            else if (this.ObjectCentralized && this.Notifying)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.Set{this.Name}"))
                {
                    args.Add($"{rhsAccessorPrefix}");
                    args.Add($"cmds: {cmdsAccessor}");
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.ProtectedProperty}.Set"))
                {
                    args.Add($"{rhsAccessorPrefix}");
                    if (this.NotifyingType != NotifyingType.None)
                    {
                        args.Add($"{cmdsAccessor}");
                    }
                }
            }
            fg.AppendLine($"break;");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            if (this.NotifyingType != NotifyingType.None)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Property}.Unset({cmdAccessor}.ToUnsetParams());");
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = default({this.TypeName});");
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Property}.Unset();");
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = default({this.TypeName});");
                }
            }
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return {identifier}.{this.Name};");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
        {
            if (!this.IntegrateField) return;
            if (!this.ReadOnly
                && this.HasBeenSet)
            {
                fg.AppendLine($"{identifier}.{this.GetName(internalUse: false, property: true)}.HasBeenSet = {onIdentifier};");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.IntegrateField) return;
            if (!this.ReadOnly)
            {
                if (this.HasBeenSet)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{identifier}.{this.GetName(internalUse: false, property: true)}.Unset"))
                    {
                        if (this.NotifyingType != NotifyingType.None)
                        {
                            args.Add(cmdsAccessor);
                        }
                    }
                }
                else
                {
                    fg.AppendLine($"{identifier}.{this.Name} = default({this.TypeName});");
                }
            }
            fg.AppendLine("break;");
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"if ({accessor.DirectAccess} != {rhsAccessor.DirectAccess}) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => l == r);");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor.DirectAccess} == {rhsAccessor.DirectAccess};");
            }
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({this.Name}).CombineHashCode({hashResultAccessor});");
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"{fgAccessor}.AppendLine($\"{name} => {{{accessor.DirectAccess}}}\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.HasValue && {checkMaskAccessor}.Value != {accessor.PropertyAccess}.HasBeenSet) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.HasBeenSet;");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = true;");
            }
        }
    }
}
