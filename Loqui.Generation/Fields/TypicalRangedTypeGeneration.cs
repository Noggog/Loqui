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
                    fg.AppendLine($"public {(Protected ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Item;");
                        fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                    }
                    if (!this.Protected)
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
                    fg.AppendLine($"public {(Protected ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Item;");
                        fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                    }
                    if (!this.Protected)
                    {
                        fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
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
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this._{this.Name}.Set(value{InRangeCheckerString});");
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
                    fg.AppendLine($"private {TypeName} _{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => _{this.Name};");
                        fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set");
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
