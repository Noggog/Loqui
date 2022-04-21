using System.Reflection;

namespace Loqui.Internal;

public static class GenericCommonInstanceGetter
{
    private static readonly Dictionary<Type, object> _instanceTracker = new Dictionary<Type, object>();

    public static object Get(object common, Type containedCommonType, Type desiredType)
    {
        if (containedCommonType == desiredType) return common;
        if (_instanceTracker.TryGetValue(desiredType, out var obj)) return obj;
        var genType = common.GetType().GetGenericTypeDefinition();
        var t = genType.MakeGenericType(desiredType).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.Name == "Instance")
            .First();
        obj = t.GetValue(null)!;
        _instanceTracker[desiredType] = obj;
        return obj;
    }
}