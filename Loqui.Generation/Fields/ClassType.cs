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
        public bool Nullable { get; set; } = true;
        public bool Singleton;
        public bool Readonly;
        public override bool Copy => base.Copy && !this.Singleton;
        public override string ProtectedName
        {
            get
            {
                switch (this.Notifying)
                {
                    case NotifyingOption.None:
                        return $"_{this.Name}";
                    case NotifyingOption.HasBeenSet:
                    case NotifyingOption.Notifying:
                        return $"{this.ProtectedProperty}.Item";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public abstract string GetNewForNonNullable();

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Singleton = node.GetAttribute<bool>(Constants.SINGLETON, this.Singleton);
            this.Nullable = node.GetAttribute<bool>("nullable", this.Nullable && !this.Singleton);
            if (this.Singleton && this.Nullable)
            {
                throw new ArgumentException("A class type cannot be both nullable and a singleton.");
            }
            this.Protected = this.Protected || this.Singleton;
        }

        protected override string GenerateDefaultValue()
        {
            if (!this.Nullable 
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
            yield return $"markAsSet: {(this.Singleton ? "true" : "false")}";
            if (!this.Nullable)
            {
                yield return $"noNullFallback: () => {GetNewForNonNullable()}";
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    fg.AppendLine($"private {TypeName} _{this.Name}{(this.Nullable ? string.Empty : $" = {GetNewForNonNullable()}")};");
                    fg.AppendLine($"public {TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => {this.ProtectedName};");
                        if (this.Nullable)
                        {
                            fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set {{ this._{this.Name} = value;{(this.RaisePropertyChanged ? $" OnPropertyChanged(nameof({this.Name}));" : string.Empty)} }}");
                        }
                        else
                        {
                            fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set");
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
                    break;
                case NotifyingOption.HasBeenSet:
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
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
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
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"protected readonly INotifyingItem<{TypeName}> _{this.Name};");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public {(Protected ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Item;");
                        fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value);");
                    }
                    if (!this.Protected)
                    {
                        fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool IsNullable()
        {
            return this.Nullable;
        }
    }
}
