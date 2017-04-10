using System;

namespace Noggolloquy.Generation
{
    public class WildcardType : TypicalTypeGeneration
    {
        public override Type Type => typeof(object);
        
        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.Notifying)
            {
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
                fg.AppendLine($"public {TypeName} {this.Name} {{ get {{ return _{this.Name}.Value; }} {(Protected ? "protected " : string.Empty)}set {{ _{this.Name}.Value = value; }} }}");
                fg.AppendLine($"INotifyingItem{(Protected ? "Getter" : string.Empty)}<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
            }
            else
            {
                fg.AppendLine($"private {(this.ReadOnly ? "readonly" : string.Empty)} {this.TypeName} _{this.Name};");
                fg.AppendLine($"public {(this.ReadOnly ? "readonly" : string.Empty)} {this.TypeName} {this.Name} {{ get {{ return this._{this.Name}; }} {(this.ReadOnly ? string.Empty : $"set {{ this._{this.Name} = WildcardLink.Validate(value); }} ")}}}");
                fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
            }
        }
    }
}
