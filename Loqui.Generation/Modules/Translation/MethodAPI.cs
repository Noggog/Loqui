using Noggog;

namespace Loqui.Generation;

public class MethodAPI
{
    public List<APILine> MajorAPI { get; private set; } = new List<APILine>();
    public List<CustomMethodAPI> CustomAPI { get; private set; } = new List<CustomMethodAPI>();
    public List<APILine> OptionalAPI { get; private set; } = new List<APILine>();

    public MethodAPI(
        APILine[] majorAPI,
        CustomMethodAPI[] customAPI,
        APILine[] optionalAPI)
    {
        MajorAPI.AddRange(majorAPI ?? EnumerableExt<APILine>.Empty);
        CustomAPI.AddRange(customAPI ?? EnumerableExt<CustomMethodAPI>.Empty);
        OptionalAPI.AddRange(optionalAPI ?? EnumerableExt<APILine>.Empty);
    }

    public MethodAPI(
        params APILine[] api)
    {
        MajorAPI.AddRange(api);
    }
        
    public IEnumerable<(APIResult API, bool Public)> IterateAPI(ObjectGeneration obj, TranslationDirection dir, params APILine[] customLines)
    {
        foreach (var item in MajorAPI)
        {
            if (item.TryResolve(obj, dir, out var line))
            {
                yield return (line, true);
            }
        }
        foreach (var item in CustomAPI)
        {
            if (item.API.TryResolve(obj, dir, out var line))
            {
                yield return (line, item.Public);
            }
        }
        foreach (var item in customLines)
        {
            if (item == null) continue;
            if (!item.When(obj, dir)) continue;
            yield return (item.Resolver(obj), true);
        }
        foreach (var item in OptionalAPI)
        {
            if (item.TryResolve(obj, dir, out var line))
            {
                yield return (line, true);
            }
        }
    }
}