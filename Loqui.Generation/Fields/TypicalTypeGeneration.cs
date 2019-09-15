using Noggog;
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
        public abstract Type Type(bool getter);
        public override string TypeName(bool getter) => Type(getter).GetName();
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
        public event Action<FileGeneration> PreSetEvent;
        public event Action<FileGeneration> PostSetEvent;

        public override bool CopyNeedsTryCatch => !this.Bare;

        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name} ?? true";

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            node.TryGetAttribute("default", out DefaultValue);
        }

        public override async Task GenerateForCtor(FileGeneration fg)
        {
            await base.GenerateForCtor(fg);

            if (!this.Bare
                && !this.TrueReadOnly
                && this.RaisePropertyChanged
                && this.NotifyingType != NotifyingType.ReactiveUI)
            {
                GenerateNotifyingConstruction(fg, $"_{this.Name}");
            }
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
            if (subFg.Strings.Count > 0
                && string.IsNullOrWhiteSpace(subFg.Strings[subFg.Strings.Count - 1]))
            {
                subFg.Strings.RemoveAt(subFg.Strings.Count - 1);
            }
            if (subFg.Strings.Count > 1)
            {
                fg.AppendLine(linePrefix);
                using (new BraceWrapper(fg))
                {
                    fg.AppendLines(subFg.Strings);
                }
            }
            else if (subFg.Strings.Count > 0)
            {
                fg.AppendLine($"{linePrefix} => {subFg.Strings[0]}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual string GetValueSetString(Accessor accessor) => accessor.DirectAccess;

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
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
                                WrapSetAccessor(fg,
                                    linePrefix: $"{SetPermissionStr}set",
                                    toDo: (subFg) => subFg.AppendLine($"this.RaiseAndSetIfChanged(_hasBeenSetTracker, {GetValueSetString("value")}, (int){this.ObjectCentralizationEnumName}, nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));"));
                            }
                        }
                        fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                        fg.AppendLine($"private {TypeName(getter: false)} _{this.Name};");
                        if (HasDefault)
                        {
                            fg.AppendLine($"public readonly static {TypeName(getter: false)} _{this.Name}_Default = {this.DefaultValue};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name};");
                            fg.AppendLine($"{(ReadOnly ? "protected " : string.Empty)}set => {this.Name}_Set({GetValueSetString("value")});");
                        }
                        fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName(getter: false)} {this.Name};");
                        fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Set"))
                    {
                        args.Add($"{this.TypeName(getter: false)} value");
                        args.Add($"bool markSet = true");
                    }
                    using (new BraceWrapper(fg))
                    {
                        WrapSetCode(fg,
                            subGen =>
                            {
                                subGen.AppendLine($"this.RaiseAndSetIf{(ReferenceChanged ? "Reference" : null)}Changed(ref _{this.Name}, {GetValueSetString("value")}, _hasBeenSetTracker, markSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                            });
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Unset"))
                    {
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"this.{this.Name}_Set({(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName(getter: false)})")}, false);");
                    }
                }
                else
                {
                    if (HasDefault)
                    {
                        fg.AppendLine($"private {TypeName(getter: false)} _{this.Name} = _{this.Name}_Default;");
                        fg.AppendLine($"public readonly static {TypeName(getter: false)} _{this.Name}_Default = {this.DefaultValue};");
                    }
                    else
                    {
                        fg.AppendLine($"private {TypeName(getter: false)} _{this.Name};");
                    }
                    fg.AppendLine($"public {TypeName(getter: false)} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{this.Name};");
                        WrapSetAccessor(fg,
                            linePrefix: $"{SetPermissionStr}set",
                            toDo: subGen => subGen.AppendLine($"this.RaiseAndSetIf{(ReferenceChanged ? "Reference" : null)}Changed(ref this._{this.Name}, {GetValueSetString("value")}, nameof({this.Name}));"));
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
                            fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName(getter: false)}> _{this.Name};");
                        }
                        else
                        {
                            GenerateNotifyingCtor(fg);
                        }
                        fg.AppendLine($"public IHasBeenSetItem<{this.TypeName(getter: false)}> {this.Property} => _{this.Name};");
                        if (HasDefault)
                        {
                            fg.AppendLine($"public readonly static {TypeName(getter: false)} _{this.Name}_Default = {this.DefaultValue};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name}.Item;");
                            WrapSetAccessor(fg,
                                linePrefix: $"{SetPermissionStr}set",
                                toDo: subGen => subGen.AppendLine($"this._{this.Name}.Set({GetValueSetString("value")});"));
                        }
                        fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName(getter: false)} {this.Name};");
                        fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                    }
                }
                else
                {
                    if (HasDefault)
                    {
                        fg.AppendLine($"public readonly static {TypeName(getter: false)} _{this.Name}_Default = {this.DefaultValue};");
                    }
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"private {TypeName(getter: false)} _{this.Name};");
                        fg.AppendLine($"public {TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{this.Name};");
                            WrapSetAccessor(fg,
                                linePrefix: $"{SetPermissionStr}set",
                                toDo: subGen =>
                                {
                                    subGen.AppendLine($"this._{this.Name} = {GetValueSetString("value")};");
                                    subGen.AppendLine($"OnPropertyChanged(nameof({this.Name}));");
                                });
                        }
                    }
                    else
                    {
                        fg.AppendLine($"public {TypeName(getter: false)} {this.Name} {{ get; {SetPermissionStr}set; }}");
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
                    fg.AppendLine($"{this.Name} = {(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName(getter: false)})")};");
                }
            }
            if (this.HasInternalInterface)
            {
                if (this.InternalSetInterface)
                {
                    fg.AppendLine($"{TypeName(getter: false)} {this.ObjectGen.Interface(getter: false, internalInterface: true)}.{this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this.{this.Name};");
                        fg.AppendLine($"set => this.{this.Name} = {GetValueSetString("value")};");
                    }
                }
                if (this.InternalGetInterface)
                {
                    fg.AppendLine($"{TypeName(getter: false)} {this.ObjectGen.Interface(getter: true, internalInterface: true)}.{this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this.{this.Name};");
                    }
                }
            }
        }

        protected string GetNotifyingProperty()
        {
            string item;
            if (this.HasBeenSet)
            {
                item = "HasBeenSetItem";
            }
            else
            {
                throw new NotImplementedException();
            }
            return $"protected I{item}<{TypeName(getter: false)}> _{this.Name}";
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
            if (this.HasBeenSet)
            {
                item = "HasBeenSetItem";
            }
            else
            {
                throw new NotImplementedException();
            }
            using (var args = new ArgsWrapper(fg,
                $"{prepend} = {item}.Factory<{TypeName(getter: false)}>"))
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

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (getter)
            {
                if (!ApplicableInterfaceField(getter, internalInterface)) return;
                fg.AppendLine($"{TypeName(getter: true)} {this.Name} {{ get; }}");
                if (this.NotifyingType == NotifyingType.ReactiveUI)
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
                        fg.AppendLine($"IHasBeenSetItemGetter<{TypeName(getter: false)}> {this.Property} {{ get; }}");
                    }
                }
                fg.AppendLine();
            }
            else
            {
                if (!ApplicableInterfaceField(getter, internalInterface)) return;
                fg.AppendLine($"new {TypeName(getter: false)} {this.Name} {{ get; set; }}");

                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    if (this.HasBeenSet)
                    {
                        fg.AppendLine($"new bool {this.HasBeenSetAccessor(new Accessor(this.Name))} {{ get; set; }}");
                        fg.AppendLine($"void {this.Name}_Set({this.TypeName(getter: false)} value, bool hasBeenSet = true);");
                        fg.AppendLine($"void {this.Name}_Unset();");
                    }
                }
                else if (this.NotifyingType == NotifyingType.None)
                {
                    if (this.HasBeenSet)
                    {
                        fg.AppendLine($"new IHasBeenSetItem{(this.ReadOnly ? "Getter" : string.Empty)}<{TypeName(getter: false)}> {this.Property} {{ get; }}");
                    }
                }
                fg.AppendLine();
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
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
                    }
                }
                else
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor.PropertyAccess}.Set"))
                    {
                        args.Add($"value: {rhsAccessorPrefix}.{this.GetName(false, false)}");
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
                        args.Add($"rhsHasBeenSet: {this.HasBeenSetAccessor(new Accessor(this, $"{rhsAccessorPrefix}."))}");
                        args.Add($"defItem: {defaultFallbackAccessor}?.{this.Name} ?? default({this.TypeName(getter: false)})");
                        args.Add($"defHasBeenSet: {this.HasBeenSetAccessor(new Accessor(this, $"{defaultFallbackAccessor}?."))} ?? false");
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

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
            if (!this.IntegrateField) return;
            if (this.Bare)
            {
                fg.AppendLine($"{accessorPrefix}.{this.ProtectedName} = {rhsAccessorPrefix};");
            }
            else if (this.ObjectCentralized && this.Notifying)
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix};");
            }
            else if (this.HasProperty)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.ProtectedProperty}.Set"))
                {
                    args.Add($"{rhsAccessorPrefix}");
                }
            }
            else
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix};");
            }
            fg.AppendLine($"break;");
        }

        public override void GenerateClear(FileGeneration fg, Accessor identifier)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            // ToDo
            // Add internal interface support
            if (this.HasInternalInterface) return;
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"{identifier.DirectAccess}_Unset();");
                }
                else
                {
                    fg.AppendLine($"{identifier.DirectAccess} = {(this.HasDefault ? $"{this.ObjectGen.Name}._{this.Name}_Default" : $"default({this.TypeName(getter: false)})")};");
                }
                return;
            }
            if (!this.Bare)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.Unset();");
            }
            else
            {
                fg.AppendLine($"{identifier.DirectAccess} = {(this.HasDefault ? $"{this.ObjectGen.Name}._{this.Name}_Default" : $"default({this.TypeName(getter: false)})")};");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            GenerateClear(fg, identifier);
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
            fg.AppendLine($"if ({GenerateEqualsSnippet(accessor, rhsAccessor, negate: true)}) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            // ToDo
            // Add Internal interface support
            if (this.HasInternalInterface) return;
            if (this.HasBeenSet)
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    fg.AppendLine($"{retAccessor} = {this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)} && {GenerateEqualsSnippet(accessor.DirectAccess, rhsAccessor.DirectAccess)};");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => {GenerateEqualsSnippet("l", "r")});");
                }
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {GenerateEqualsSnippet(accessor.DirectAccess, rhsAccessor.DirectAccess)};");
            }
        }

        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({accessor}).CombineHashCode({hashResultAccessor});");
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            if (!this.IntegrateField) return;
            // ToDo
            // Add Internal interface support
            if (this.HasInternalInterface) return;
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
