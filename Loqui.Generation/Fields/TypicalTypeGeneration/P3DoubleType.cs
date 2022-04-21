using Noggog;

namespace Loqui.Generation;

public class P3DoubleType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(P3Double);

    protected override string GenerateDefaultValue() => $"new {TypeName(getter: false)}({DefaultValue})";

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        return $"{(negate ? "!" : null)}{accessor.Access}.Equals({rhsAccessor.Access})";
    }
}