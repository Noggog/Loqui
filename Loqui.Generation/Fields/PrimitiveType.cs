namespace Loqui.Generation;

public abstract class PrimitiveType : TypicalTypeGeneration
{
    public override bool IsEnumerable => false;
    public override bool IsClass => false;

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        return $"{accessor.Access} {(negate ? "!" : "=")}= {rhsAccessor.Access}";
    }

    public override string GetDefault(bool getter)
    {
        if (Nullable)
        {
            return $"default({TypeName(getter: getter)}?)";
        }
        else
        {
            return "default";
        }
    }

    public override void GenerateForCopy(FileGeneration fg, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            fg.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (new BraceWrapper(fg, doIt: !AlwaysCopy))
        {
            fg.AppendLine($"{accessor.Access} = {rhs};");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        return $"{accessor}";
    }
}