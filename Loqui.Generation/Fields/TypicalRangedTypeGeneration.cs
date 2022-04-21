using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TypicalRangedTypeGeneration : PrimitiveType
{
    public string Min;
    public string Max;
    public bool RangeThrowException;
    public bool HasRange;

    public virtual string RangeTypeName(bool getter) => $"Range{TypeName(getter).TrimEnd("?")}";
    public string RangeMemberName => $"{Name}_Range";

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

    protected string InRangeCheckerString => $"{(TypeExt.IsNullable(GetType()) ? "?" : string.Empty)}.{(RangeThrowException ? "" : "Put")}InRange({RangeMemberName}.Min, {RangeMemberName}.Max)";

    public override string GetValueSetString(Accessor accessor)
    {
        if (HasRange)
        {
            return $"{accessor.Access}{InRangeCheckerString}";
        }
        return base.GetValueSetString(accessor);
    }

    public override async Task GenerateForClass(FileGeneration fg)
    {
        await base.GenerateForClass(fg);
        if (HasRange)
        {
            fg.AppendLine($"public static {RangeTypeName(getter: false)} {RangeMemberName} = new {RangeTypeName(getter: false)}({Min}, {Max});");
        }
    }
}