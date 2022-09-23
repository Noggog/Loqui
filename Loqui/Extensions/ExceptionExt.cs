using Noggog;
using System.Diagnostics.CodeAnalysis;

namespace Loqui;

public static class ExceptionExt
{
    [return: NotNullIfNotNull("lhs")]
    [return: NotNullIfNotNull("rhs")]
    public static IEnumerable<MaskItemIndexed<TKey, TOverall, TSpecific>>? Combine<TKey, TOverall, TSpecific>(this IEnumerable<MaskItemIndexed<TKey, TOverall, TSpecific>>? lhs, IEnumerable<MaskItemIndexed<TKey, TOverall, TSpecific>>? rhs)
    {
        if (lhs == null && rhs == null) return null;
        if (rhs == null) return lhs;
        if (lhs == null) return rhs;
        return lhs.And(rhs);
    }
}