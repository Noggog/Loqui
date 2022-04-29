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

    public override void GenerateForCopy(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            sb.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (sb.CurlyBrace(doIt: !AlwaysCopy))
        {
            sb.AppendLine($"{accessor.Access} = {rhs};");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        return $"{accessor}";
    }
}