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
                if (this.Notifying)
                {
                    return $"{this.ProtectedProperty}.Value";
                }
                else
                {
                    return $"{this.ProtectedProperty}.Item";
                }
            }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            HasDefault = node.TryGetAttribute("default", out DefaultValue);
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.Notifying)
            {
                this.GenerateNotifyingCtor(fg);
                fg.AppendLine($"public {(Protected ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                fg.AppendLine($"public {TypeName} {this.Name} {{ get {{ return _{this.Name}.Value; }} {(Protected ? "protected " : string.Empty)}set {{ _{this.Name}.Value = value; }} }}");
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                }
                fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
            }
            else
            {
                if (!this.TrueReadOnly)
                {
                    fg.AppendLine($"private readonly HasBeenSetItem<{this.TypeName}> _{this.Name} = new HasBeenSetItem<{this.TypeName}>();");
                    fg.AppendLine($"public IHasBeenSet<{this.TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name} {{ get {{ return this._{this.Name}.Item; }} {(Protected ? "protected " : string.Empty)}set {{ this._{this.Name}.Set(value); }} }}");
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"IHasBeenSetGetter {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                }
                else
                {
                    fg.AppendLine($"public readonly {this.TypeName} {this.Name};");
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"IHasBeenSetGetter {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => HasBeenSetGetter.NotBeenSet_Instance;");
                }
            }
        }

        protected virtual void GenerateNotifyingCtor(FileGeneration fg)
        {
            fg.AppendLine($"protected readonly INotifyingItem<{TypeName}> _{this.Name} = new NotifyingItem<{TypeName}>(");
            using (new DepthWrapper(fg))
            {
                if (HasDefault)
                {
                    fg.AppendLine($"defaultVal: {GenerateDefaultValue()},");
                    fg.AppendLine("markAsSet: false");
                }
                else
                {
                    fg.AppendLine($"default({this.TypeName}),");
                    fg.AppendLine("markAsSet: false");
                }
            }
            fg.AppendLine(");");
        }

        protected virtual string GenerateDefaultValue()
        {
            return this.DefaultValue;
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (this.ReadOnly) return;
            fg.AppendLine($"new {TypeName} {this.Name} {{ get; {(Protected ? string.Empty : "set; ")}}}");
            if (this.Notifying)
            {
                fg.AppendLine($"new INotifyingItem{(this.Protected ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get; }}");
            }
            else
            {
                fg.AppendLine($"new IHasBeenSet{(this.Protected ? "Getter" : $"<{TypeName}>")} {this.Property} {{ get; }}");
            }
            fg.AppendLine();
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"{TypeName} {this.Name} {{ get; }}");
            if (this.Notifying)
            {
                fg.AppendLine($"INotifyingItemGetter<{TypeName}> {this.Property} {{ get; }}");
            }
            else
            {
                fg.AppendLine($"IHasBeenSetGetter {this.Property} {{ get; }}");
            }
            fg.AppendLine();
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{accessorPrefix}.{this.Property}.Set(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"{rhsAccessorPrefix}.{this.Name},");
                    fg.AppendLine($"{cmdsAccessor});");
                }
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if ({defaultFallbackAccessor} == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Property}.Unset({cmdsAccessor}.ToUnsetParams());");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Property}.Set(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"{defaultFallbackAccessor}.{this.Name},");
                        fg.AppendLine($"{cmdsAccessor});");
                    }
                }
            }
            fg.AppendLine();
        }

        public override void SetMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskAccessor} = {exception};");
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return rhsAccessor;
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.Notifying)
            {
                fg.AppendLine($"{accessorPrefix}.{this.Property}.Unset({cmdAccessor}.ToUnsetParams());");
            }
            else
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = default({this.TypeName});");
            }
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine($"return {identifier}.{this.Name};");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetPropertyString(internalUse)}.SetHasBeenSet({onIdentifier});");
            }
        }
    }
}
