using System;

namespace Noggolloquy.Generation
{
    public class WildcardType : TypicalTypeGeneration
    {
        public override Type Type => typeof(object);

        public override void GenerateForClass(FileGeneration fg)
        {
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    fg.AppendLine($"protected {this.TypeName} _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name};");
                        fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set => this._{ this.Name} = WildcardLink.Validate(value);");
                    }
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    break;
                case NotifyingOption.HasBeenSet:
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName}> _{this.Name};");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public {(Protected ? "IHasBeenSetItemGetter" : "IHasBeenSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    if (this.Protected)
                    {
                        fg.AppendLine($"public {this.TypeName} {this.Name} => this._{ this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name};");
                            fg.AppendLine($"set => this._{this.Name}.Item = WildcardLink.Validate(value);");
                        }
                    }
                    fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.GetName(false, true)};");
                    break;
                case NotifyingOption.Notifying:
                    fg.AppendLine($"protected readonly INotifyingItem<{TypeName}> _{this.Name} = new NotifyingItemConvertWrapper<{TypeName}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine("(change) => TryGet<Object>.Succeed(WildcardLink.Validate(change.New)),");
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
                    fg.AppendLine($"public {(Protected ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get => _{this.Name}.Item; {(Protected ? "protected " : string.Empty)}set => _{this.Name}.Item = value; }}");
                    if (!this.Protected)
                    {
                        fg.AppendLine($"INotifyingItem{(Protected ? "Getter" : string.Empty)}<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
