using System;

namespace Loqui.Generation
{
    public class WildcardType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(object);

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName(getter: false)}> _{this.Name};");
                fg.AppendLine($"public {(ReadOnly ? "IHasBeenSetItemGetter" : "IHasBeenSetItem")}<{TypeName(getter: false)}> {this.Property} => _{this.Name};");
                if (this.ReadOnly)
                {
                    fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name} => this._{ this.Name};");
                }
                else
                {
                    fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Item;");
                        fg.AppendLine($"set => this._{this.Name}.Item = WildcardLink.Validate(value);");
                    }
                }
                fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: false)}> {this.ObjectGen.Interface(getter: true)}.{this.Property} => this.{this.GetName(false, true)};");
            }
            else
            {
                fg.AppendLine($"protected {this.TypeName(getter: false)} _{this.Name};");
                fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"get => this._{ this.Name};");
                    fg.AppendLine($"{SetPermissionStr}set => this._{ this.Name} = WildcardLink.Validate(value);");
                }
                fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
            }
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }
    }
}
