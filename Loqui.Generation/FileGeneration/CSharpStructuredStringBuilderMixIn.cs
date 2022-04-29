namespace Loqui.Generation;

public static class CSharpStructuredStringBuilderMixIn
{
    public static Args Args(this StructuredStringBuilder sb,
        string initialLine = null,
        string suffixLine = null,
        bool semiColon = true)
    {
        return new Args(sb,
            initialLine: initialLine,
            suffixLine: suffixLine,
            semiColon: semiColon);
    }

    public static Function Function(this StructuredStringBuilder sb, string initialLine, bool semiColon = false)
    {
        return new Function(sb,
            initialLine: initialLine)
        {
            SemiColon = semiColon
        };
    }

    public static Class Class(this StructuredStringBuilder sb, string name)
    {
        return new Class(sb, name: name);
    }

    public static Namespace Namespace(this StructuredStringBuilder sb, string str, bool fileScoped = true)
    {
        return new Namespace(sb, str: str, fileScoped: fileScoped);
    }

    public static If If(this StructuredStringBuilder sb, bool ANDs, bool first = true)
    {
        return new If(sb, ANDs: ANDs, first: first);
    }

    public static Region Region(this StructuredStringBuilder sb, string str, bool appendExtraLine = true, bool skipIfOnlyOneLine = false)
    {
        return new Region(sb, str: str, appendExtraLine: appendExtraLine, skipIfOnlyOneLine: skipIfOnlyOneLine);
    }

    public static PropertyCtor PropertyCtor(this StructuredStringBuilder sb)
    {
        return new PropertyCtor(sb);
    }
}