namespace Loqui.Generation;

public class GenericSpecification
{
    public Dictionary<string, string> Specifications = new();
    public Dictionary<string, string> Mappings = new();
}

public static class GenericSpecificationExt
{
    public static string Swap(this GenericSpecification spec, string str)
    {
        if (spec == null) return str;
        if (spec.Specifications.TryGetValue(str, out var swap)) return swap;
        return str;
    }
}