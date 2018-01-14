using System;

namespace Loqui.Generation
{
    public class WildcardType : PrimitiveType
    {
        public override Type Type => typeof(object);

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.Notifying)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"protected readonly INotifyingSetItem<{TypeName}> _{this.Name} = new NotifyingSetItemConvertWrapper<{TypeName}>(");
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
                    fg.AppendLine($"public {(ReadOnly ? "INotifyingSetItemGetter" : "INotifyingSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get => _{this.Name}.Item; {(ReadOnly ? "protected " : string.Empty)}set => _{this.Name}.Item = value; }}");
                    if (!this.ReadOnly)
                    {
                        fg.AppendLine($"INotifyingSetItem{(ReadOnly ? "Getter" : string.Empty)}<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                }
                else
                {
                    using (var args = new ArgsWrapper(fg, $"protected readonly INotifyingItem<{TypeName}> _{this.Name} = new NotifyingItemConvertWrapper<{TypeName}>"))
                    {
                        args.Add("(change) => TryGet<Object>.Succeed(WildcardLink.Validate(change.New))");
                        if (HasDefault)
                        {
                            args.Add($"defaultVal: {GenerateDefaultValue()}");
                            if (this.HasBeenSet)
                            {
                                args.Add("markAsSet: false");
                            }
                        }
                        else
                        {
                            args.Add($"default({this.TypeName})");
                            if (this.HasBeenSet)
                            {
                                args.Add("markAsSet: false");
                            }
                        }
                    }
                    fg.AppendLine($"public {(ReadOnly ? "INotifyingItemGetter" : "INotifyingItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get => _{this.Name}.Item; {(ReadOnly ? "protected " : string.Empty)}set => _{this.Name}.Item = value; }}");
                    if (!this.ReadOnly)
                    {
                        fg.AppendLine($"INotifyingItem{(ReadOnly ? "Getter" : string.Empty)}<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName}> _{this.Name};");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public {(ReadOnly ? "IHasBeenSetItemGetter" : "IHasBeenSetItem")}<{TypeName}> {this.Property} => _{this.Name};");
                    if (this.ReadOnly)
                    {
                        fg.AppendLine($"public {this.TypeName} {this.Name} => this._{ this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                        fg.AppendLine($"public {this.TypeName} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name}.Item;");
                            fg.AppendLine($"set => this._{this.Name}.Item = WildcardLink.Validate(value);");
                        }
                    }
                    fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.GetName(false, true)};");
                }
                else
                {
                    fg.AppendLine($"protected {this.TypeName} _{this.Name};");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name};");
                        fg.AppendLine($"{(this.ReadOnly ? "protected " : string.Empty)}set => this._{ this.Name} = WildcardLink.Validate(value);");
                    }
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                }
            }
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!object.Equals({this.Name}, {rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => object.Equals(l, r));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess});");
            }
        }
    }
}
