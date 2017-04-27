using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalGeneration : TypeGeneration
    {
        public string DefaultValue;
        public bool HasDefault;
        public override string ProtectedProperty => "_" + this.Name;
        public override string ProtectedName
        {
            get
            {
                switch (this.Notifying)
                {
                    case NotifyingOption.None:
                        return $"{this.Name}";
                    case NotifyingOption.HasBeenSet:
                        return $"{this.ProtectedProperty}.Item";
                    case NotifyingOption.Notifying:
                        return $"{this.ProtectedProperty}.Value";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override bool CopyNeedsTryCatch => this.Notifying == NotifyingOption.Notifying;

        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name} ?? true";

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            HasDefault = node.TryGetAttribute("default", out DefaultValue);
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get; {(this.ReadOnly ? "protected " : string.Empty)}set; }}");
                    break;
                case NotifyingOption.HasBeenSet:
                    if (!this.TrueReadOnly)
                    {
                        GenerateNotifyingCtor(fg, notifying: false);
                        fg.AppendLine($"public IHasBeenSetItem<{this.TypeName}> {this.Property} => _{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Value;");
                            fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
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
                    break;
                case NotifyingOption.Notifying:
                    this.GenerateNotifyingCtor(fg);
                    fg.AppendLine($"public {(Protected ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Value;");
                        fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                    }
                    if (!this.ReadOnly)
                    {
                        fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual void GenerateNotifyingCtor(FileGeneration fg, bool notifying = true)
        {
            using (var args = new ArgsWrapper(fg,
                $"protected readonly {(notifying ? "INotifyingItem" : "IHasBeenSetItem")}<{TypeName}> _{this.Name} = new {(notifying ? "NotifyingItem" : "HasBeenSetItem")}<{TypeName}>"))
            {
                if (HasDefault)
                {
                    args.Add($"defaultVal: {GenerateDefaultValue()}");
                    args.Add("markAsSet: false");
                }
                else
                {
                    args.Add($"default({this.TypeName})");
                    args.Add("markAsSet: false");
                }
            }
        }

        protected virtual string GenerateDefaultValue()
        {
            return this.DefaultValue;
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (this.ReadOnly) return;
            fg.AppendLine($"new {TypeName} {this.Name} {{ get; {(Protected ? string.Empty : "set; ")}}}");
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    break;
                case NotifyingOption.HasBeenSet:
                    fg.AppendLine($"new IHasBeenSetItem{(this.Protected ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
                    break;
                case NotifyingOption.Notifying:
                    fg.AppendLine($"new INotifyingItem{(this.Protected ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
                    break;
                default:
                    throw new NotImplementedException();
            }
            fg.AppendLine();
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"{TypeName} {this.Name} {{ get; }}");
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    break;
                case NotifyingOption.HasBeenSet:
                    fg.AppendLine($"IHasBeenSetItemGetter<{TypeName}> {this.Property} {{ get; }}");
                    break;
                case NotifyingOption.Notifying:
                    fg.AppendLine($"INotifyingItemGetter<{TypeName}> {this.Property} {{ get; }}");
                    break;
                default:
                    throw new NotImplementedException();
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
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)};");
                return;
            }
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.GetName(false, true)}.SetToWithDefault"))
            {
                args.Add($"{rhsAccessorPrefix}.{this.GetName(false, true)}");
                args.Add($"{defaultFallbackAccessor}?.{this.GetName(false, true)}");
                if (this.Notifying == NotifyingOption.Notifying)
                {
                    args.Add($"{cmdsAccessor}");
                }
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return rhsAccessor;
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{accessorPrefix}.{this.ProtectedName} = {rhsAccessorPrefix};");
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.ProtectedProperty}.Set"))
                {
                    args.Add($"{rhsAccessorPrefix}");
                    if (this.Notifying == NotifyingOption.Notifying)
                    {
                        args.Add($"{cmdsAccessor}");
                    }
                }
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.ReadOnly) return;
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = default({this.TypeName});");
                    break;
                case NotifyingOption.HasBeenSet:
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = default({this.TypeName});");
                    break;
                case NotifyingOption.Notifying:
                    fg.AppendLine($"{accessorPrefix}.{this.Property}.Unset({cmdAccessor}.ToUnsetParams());");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine($"return {identifier}.{this.Name};");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            if (!this.Protected && this.Notifying != NotifyingOption.None)
            {
                fg.AppendLine($"{identifier}.{this.GetName(internalUse: false, property: true)}.HasBeenSet = {onIdentifier};");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.Protected)
            {
                if (this.Notifying == NotifyingOption.None)
                {
                    fg.AppendLine($"{identifier}.{this.Name} = default({this.TypeName});");
                }
                else
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{identifier}.{this.GetName(internalUse: false, property: true)}.Unset"))
                    {
                        if (this.Notifying == NotifyingOption.Notifying)
                        {
                            args.Add(cmdsAccessor);
                        }
                    }
                }
            }
        }
    }
}
