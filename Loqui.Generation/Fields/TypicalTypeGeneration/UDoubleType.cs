using Noggog;

namespace Loqui.Generation;

public class UDoubleType : TypicalDoubleNumberTypeGeneration
{
    public override Type Type(bool getter) => typeof(UDouble);

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        return $"{(negate ? "!" : null)}{accessor.Access}.EqualsWithin({rhsAccessor.Access})";
    }

    public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        fg.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"if (!{accessor.Access}.EqualsWithin({rhsAccessor.Access})) return false;");
        }
    }
}