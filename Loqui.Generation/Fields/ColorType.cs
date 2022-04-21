using System.Drawing;

namespace Loqui.Generation;

public class ColorType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(Color);

    public override IEnumerable<string> GetRequiredNamespaces()
    {
        yield return "System.Drawing";
    }

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        return $"{(negate ? "!" : null)}{accessor.Access}.ColorOnlyEquals({rhsAccessor.Access})";
    }
}