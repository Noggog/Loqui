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

        public override string GetValueSetString(Accessor accessor)
        {
            if (this.HasRange)
            {
                return $"{accessor.DirectAccess}{InRangeCheckerString}";
            }
            return base.GetValueSetString(accessor);
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            base.GenerateForClass(fg);
            if (this.HasRange)
            {
                fg.AppendLine($"public static {this.RangeTypeName(getter: false)} {RangeMemberName} = new {this.RangeTypeName(getter: false)}({Min}, {Max});");
            }
        }
    }
}
