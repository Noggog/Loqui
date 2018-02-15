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
        public bool HasDefault;
        public override string ProtectedProperty => "_" + this.Name;
        public override string ProtectedName
        {
            get
            {
                if (this.Bare)
                {
                    if (this.RaisePropertyChanged)
                    {
                        return $"_{this.Name}";
                    }
                    else
                    {
                        return $"{this.Name}";
                    }
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
            HasDefault = node.TryGetAttribute("default", out DefaultValue);
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
            if (this.Notifying)
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
                        fg.AppendLine($"public {TypeName} {this.Name} {{ get; {(this.ReadOnly ? "protected " : string.Empty)}set; }}");
                    }
                }
            }
        }

        protected string GetNotifyingProperty()
        {
            string item;
            if (this.Notifying)
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
            return $"protected readonly I{item}<{TypeName}> _{this.Name}";
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
            if (this.Notifying)
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
            if (this.Notifying)
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
            if (this.Notifying)
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
            string accessorPrefix,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {
            if (!this.IntegrateField) return;
            if (this.Bare)
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)};");
                return;
            }
            if (this.HasBeenSet)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.GetName(false, true)}.SetToWithDefault"))
                {
                    args.Add($"rhs: {rhsAccessorPrefix}.{this.GetName(false, true)}");
                    args.Add($"def: {defaultFallbackAccessor}?.{this.GetName(false, true)}");
                    if (this.Notifying)
                    {
                        args.Add($"cmds: {cmdsAccessor}");
                    }
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.GetName(false, true)}.Set"))
                {
                    args.Add($"value: {rhsAccessorPrefix}.{this.GetName(false, false)}");
                    if (this.Notifying)
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
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.ProtectedProperty}.Set"))
                {
                    args.Add($"{rhsAccessorPrefix}");
                    if (this.Notifying)
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
            if (this.Notifying)
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
                        if (this.Notifying)
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

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"if ({this.Name} != {rhsAccessor}.{this.Name}) return false;");
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

        public override void GenerateToString(FileGeneration fg, string name, string accessor, string fgAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"{fgAccessor}.AppendLine($\"{name} => {{{accessor}}}\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, string accessor, string checkMaskAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.HasValue && {checkMaskAccessor}.Value != {accessor}.HasBeenSet) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, string accessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {accessor}.HasBeenSet;");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = true;");
            }
        }
    }
}
