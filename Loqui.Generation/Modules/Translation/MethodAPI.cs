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

    public IEnumerable<(APIResult API, bool Public)> IterateAPI(ObjectGeneration obj, TranslationDirection dir, Context context, params APILine[] customLines)
    {
        foreach (var item in IterateRawAPILines(obj, dir, customLines))
        {
            if (item.API.TryResolve(obj, dir, context, out var line))
            {
                yield return (line, item.Public);
            }
        }
    }

    public IEnumerable<(APILine API, bool Public)> IterateAPILines(ObjectGeneration obj, TranslationDirection dir, params APILine[] customLines)
    {
        foreach (var item in IterateRawAPILines(obj, dir, customLines))
        {
            if (item.API.When(obj, dir))
            {
                yield return item;
            }
        }
    }

    public IEnumerable<(APILine API, bool Public)> IterateRawAPILines(ObjectGeneration obj, TranslationDirection dir, params APILine[] customLines)
    {
        foreach (var item in MajorAPI)
        {
            yield return (item, true);
        }
        foreach (var item in CustomAPI)
        {
            yield return (item.API, item.Public);
        }
        foreach (var item in customLines)
        {
            if (item == null) continue;
            yield return (item, true);
        }
        foreach (var item in OptionalAPI)
        {
            yield return (item, true);
        }
    }
}