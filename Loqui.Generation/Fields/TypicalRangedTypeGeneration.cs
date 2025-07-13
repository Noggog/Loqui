using System.Globalization;
using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

public abstract class TypicalRangedTypeGeneration : PrimitiveType
{
    public string Min;
    public string Max;
    public bool RangeThrowException;
    public bool HasRange;

    public virtual string RangeTypeName(bool getter) => $"Range{TypeName(getter).TrimStringFromEnd("?")}";
    public string RangeMemberName => $"{Name}_Range";

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        if (node.GetAttribute(Constants.MIN) != null)
        {
            HasRange = node.TryGetAttribute(Constants.MIN, out Min, 
                culture: CultureInfo.InvariantCulture);
        }
        if (node.GetAttribute(Constants.MAX) != null)
        {
            HasRange = node.TryGetAttribute(Constants.MAX, out Max, 
                culture: CultureInfo.InvariantCulture);
        }
        RangeThrowException = node.GetAttribute<bool>(Constants.RANGE_THROW_EXCEPTION, false);
    }

    protected string InRangeCheckerString => $"{(TypeExt.IsNullable(GetType()) ? "?" : string.Empty)}.{(RangeThrowException ? "" : "Put")}InRange({RangeMemberName}.Min, {RangeMemberName}.Max)";

    public override string GetValueSetString(Accessor accessor)
    {
        if (HasRange)
        {
            return $"{accessor.Access}{InRangeCheckerString}";
        }
        return base.GetValueSetString(accessor);
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        await base.GenerateForClass(sb);
        if (HasRange)
        {
            sb.AppendLine($"public static {RangeTypeName(getter: false)} {RangeMemberName} = new {RangeTypeName(getter: false)}({Min}, {Max});");
        }
    }
}