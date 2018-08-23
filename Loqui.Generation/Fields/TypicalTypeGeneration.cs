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
                if (this.ObjectCentralized
                    || this.Bare)
                {
                    return $"{this.Name}";
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
                    if (!this.ReadOnly)
                    {
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"INotifying{(this.HasBeenSet ? "Set" : null)}Item<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"INotifying{(this.HasBeenSet ? "Set" : null)}ItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                }
            }
            else if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    if (!this.TrueReadOnly)
                    {
                        if (!this.ObjectCentralized)
                        {
                            throw new NotImplementedException();
                        }
                        fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))}");
                        using (new BraceWrapper(fg))
                        {
                            if (this.ObjectCentralized)
                            {
                                fg.AppendLine($"get => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                                fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this.RaiseAndSetIfChanged(_hasBeenSetTracker, value, (int){this.ObjectCentralizationEnumName}, nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                            }
                        }
                        fg.AppendLine($"bool {this.ObjectGen.Getter_InterfaceStr}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                        fg.AppendLine($"private {TypeName} _{this.Name};");
                        if (HasDefault)
                        {
                            fg.AppendLine($"public readonly static {TypeName} _{this.Name}_Default = {this.DefaultValue};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name};");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => {this.Name}_Set(value);");
                        }
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName} {this.Name};");
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Set"))
                    {
                        args.Add($"{this.TypeName} value");
                        args.Add($"bool markSet = true");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"this.RaiseAndSetIfChanged(ref _{this.Name}, value, _hasBeenSetTracker, markSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Unset"))
                    {
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"this.{this.Name}_Set({(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName})")}, false);");
                    }
                }
                else
                {
                    fg.AppendLine($"private {TypeName} _{this.Name};");
                    if (HasDefault)
                    {
                        fg.AppendLine($"public readonly static {TypeName} _{this.Name}_Default = {this.DefaultValue};");
                    }
                    fg.AppendLine($"public {TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{this.Name};");
                        fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set => this.RaiseAndSetIfChanged(ref this._{this.Name}, value, nameof({this.Name}));");
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
                            fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName}> _{this.Name};");
                        }
                        else
                        {
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public IHasBeenSetItem<{this.TypeName}> {this.Property} => _{this.Name};");
                        if (HasDefault)
                        {
                            fg.AppendLine($"public readonly static {TypeName} _{this.Name}_Default = {this.DefaultValue};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name}.Item;");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                        }
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName} {this.Name};");
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    }
                }
                else
                {
                    if (HasDefault)
                    {
                        fg.AppendLine($"public readonly static {TypeName} _{this.Name}_Default = {this.DefaultValue};");
                    }
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
                        fg.AppendLine($"public {TypeName} {this.Name} {{ get; {(this.ReadOnly ? "protected " : string.Empty)}set; }}");
                    }
                }
            }
            if (this.ObjectCentralized && this.NotifyingType != NotifyingType.ReactiveUI)
            {
                using (var args = new FunctionWrapper(fg,
                    $"protected void Unset{this.Name}"))
                {
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = false;");
                    fg.AppendLine($"{this.Name} = {(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName})")};");
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
            
            if (this.NotifyingType == NotifyingType.NotifyingItem)
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
            else if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"new bool {this.HasBeenSetAccessor(new Accessor(this.Name))} {{ get; set; }}");
                    fg.AppendLine($"void {this.Name}_Set({this.TypeName} item, bool hasBeenSet = true);");
                    fg.AppendLine($"void {this.Name}_Unset();");
                }
            }
            else if (this.NotifyingType == NotifyingType.None)
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
            if (this.NotifyingType == NotifyingType.NotifyingItem)
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
            else if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"bool {this.HasBeenSetAccessor(new Accessor(this.Name))} {{ get; }}");
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
            if (this.PrefersProperty)
            {
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
            else
            {
                if (this.HasBeenSet)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"if (LoquiHelper.DefaultSwitch",
                        suffixLine: ")")
                    {
                        SemiColon = false,
                    })
                    {
                        args.Add($"rhsItem: {rhsAccessorPrefix}.{this.Name}");
                        args.Add($"rhsHasBeenSet: {rhsAccessorPrefix}.{this.HasBeenSetAccessor(new Accessor(this.Name))}");
                        args.Add($"defItem: {defaultFallbackAccessor}?.{this.Name} ?? default({this.TypeName})");
                        args.Add($"defHasBeenSet: {defaultFallbackAccessor}?.{this.HasBeenSetAccessor(new Accessor(this.Name))} ?? false");
                        args.Add($"outRhsItem: out var rhs{this.Name}Item");
                        args.Add($"outDefItem: out var def{this.Name}Item");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{accessor.DirectAccess} = rhs{this.Name}Item;");
                    }
                    fg.AppendLine("else");
                    using (new BraceWrapper(fg))
                    {
                        if (this.NotifyingType == NotifyingType.ReactiveUI)
                        {
                            fg.AppendLine($"{accessor.DirectAccess}_Unset();");
                        }
                        else
                        {
                            fg.AppendLine($"{accessor.PropertyAccess}.Unset();");
                        }
                    }
                }
                else
                {
                    fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)};");
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
                if (this.NotifyingType == NotifyingType.NotifyingItem)
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
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix};");
                }
            }
            else if (this.HasProperty)
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
            else
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix};");
            }
            fg.AppendLine($"break;");
        }

        public override void GenerateClear(FileGeneration fg, Accessor identifier, string cmdAccessor)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"{identifier.DirectAccess}_Unset();");
                }
                else
                {
                    fg.AppendLine($"{identifier.DirectAccess} = {(this.HasDefault ? $"{this.ObjectGen.Name}._{this.Name}_Default" : $"default({this.TypeName})")};");
                }
                return;
            }
            if (!this.Bare)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.Unset({cmdAccessor}.ToUnsetParams());");
            }
            else
            {
                fg.AppendLine($"{identifier.DirectAccess} = {(this.HasDefault ? $"{this.ObjectGen.Name}._{this.Name}_Default" : $"default({this.TypeName})")};");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier, string cmdsAccessor)
        {
            GenerateClear(fg, identifier, cmdsAccessor);
            fg.AppendLine("break;");
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return {identifier.DirectAccess};");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
            if (!this.IntegrateField) return;
            if (!this.ReadOnly
                && this.HasBeenSet)
            {
                fg.AppendLine($"{this.HasBeenSetAccessor(identifier)} = {onIdentifier};");
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
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)} && {accessor.DirectAccess} == {rhsAccessor.DirectAccess};");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => l == r);");
                }
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
                fg.AppendLine($"if ({checkMaskAccessor}.HasValue && {checkMaskAccessor}.Value != {this.HasBeenSetAccessor(accessor)}) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)};");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = true;");
            }
        }
    }
}
