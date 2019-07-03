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

        public virtual string RangeTypeName(bool getter) => $"Range{this.TypeName(getter).TrimEnd("?")}";
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
            if (this.HasBeenSet)
            {
                if (!this.TrueReadOnly)
                {
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"protected readonly IHasBeenSetItem<{base.TypeName(getter: false)}> _{this.Name};");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public IHasBeenSetItem<{this.TypeName(getter: false)}> {this.Property} => _{this.Name};");
                    fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this._{ this.Name}.Item;");
                        fg.AppendLine($"{SetPermission}set => this._{this.Name}.Set(value{InRangeCheckerString});");
                    }
                    fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: false)}> {this.ObjectGen.Interface(getter: true)}.{this.Property} => this.{this.Property};");
                }
                else
                {
                    fg.AppendLine($"public readonly {this.TypeName(getter: false)} {this.Name};");
                    fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: false)}> {this.ObjectGen.Interface(getter: true)}.{this.Property} => HasBeenSetGetter.NotBeenSet_Instance;");
                }
            }
            else
            {
                fg.AppendLine($"private {base.TypeName(getter: false)} _{this.Name};");
                fg.AppendLine($"public {base.TypeName(getter: false)} {this.Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"get => _{this.Name};");
                    fg.AppendLine($"{SetPermissionStr}set");
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

            if (this.HasRange)
            {
                fg.AppendLine($"public static {this.RangeTypeName(getter: false)} {RangeMemberName} = new {this.RangeTypeName(getter: false)}({Min}, {Max});");
            }
        }
    }
}
