using Noggog;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

public delegate void InternalTranslation(params IAPIItem[] accessors);
public class TranslationModuleAPI
{
    public MethodAPI WriterAPI { get; private set; }
    public MethodAPI ReaderAPI { get; private set; }
    private MethodAPI Get(TranslationDirection dir) => dir == TranslationDirection.Reader ? ReaderAPI : WriterAPI;
    public IEnumerable<APILine> PublicMembers(ObjectGeneration obj, TranslationDirection dir) => Get(dir).IterateAPILines(obj, dir).Where((a) => a.Public).Select((r) => r.API);
    public string[] PassArgs(ObjectGeneration obj, TranslationDirection dir, Context lhsContext, Context rhsContext) =>
        PublicMembers(obj, dir)
            .Select(api =>
                CreatePassArgs(obj, api, lhsContext, rhsContext))
            .ToArray();
    public IEnumerable<CustomMethodAPI> InternalMembers(ObjectGeneration obj, TranslationDirection dir) => Get(dir).CustomAPI.Where((a) => !a.Public).Where(o => o.API.When(obj, dir));
    public string[] InternalFallbackArgs(ObjectGeneration obj, TranslationDirection dir, Context context) =>
        InternalMembers(obj, dir).Select(custom =>
                CombineResults(
                    custom.API.GetParameterName(obj, context),
                    custom.DefaultFallback))
            .ToArray();
    public string[] InternalPassArgs(ObjectGeneration obj, TranslationDirection dir, Context lhsContext, Context rhsContext) =>
        InternalMembers(obj, dir).Select(custom =>
                CreatePassArgs(obj, custom.API, lhsContext, rhsContext))
            .ToArray();
    public TranslationFunnel Funnel;

    public Func<ObjectGeneration, TranslationDirection, bool> When { get; set; }

    public TranslationModuleAPI(MethodAPI api, Func<ObjectGeneration, TranslationDirection, bool> when = null)
    {
        WriterAPI = api;
        ReaderAPI = api;
        When = when;
    }

    public TranslationModuleAPI(
        MethodAPI writerAPI,
        MethodAPI readerAPI,
        Func<ObjectGeneration, TranslationDirection, bool> when = null)
    {
        WriterAPI = writerAPI;
        ReaderAPI = readerAPI;
        When = when;
    }

    private IEnumerable<(T lhs, T rhs)> ZipAccessors<T>(
        IEnumerable<T> lhs,
        IEnumerable<T> rhs)
        where T : IAPIItem
    {
        if (lhs.Count() != rhs.Count())
        {
            throw new ArgumentException("Zip inputs did not have the same number of elements");
        }
        Dictionary<string, T> cache = new Dictionary<string, T>();
        foreach (var item in lhs)
        {
            cache.Add(item.NicknameKey, item);
        }

        foreach (var rhsItem in rhs.OrderBy(l => l.NicknameKey))
        {
            if (!cache.TryGetValue(rhsItem.NicknameKey, out var lhsItem))
            {
                throw new ArgumentException();
            }
            yield return (lhsItem, rhsItem);
            cache.Remove(rhsItem.NicknameKey);
        }
    }

    private string CombineResults(
        APIResult lhs,
        APIResult rhs)
    {
        return CombineResults(lhs.Result, rhs.Result);
    }

    private string CreatePassArgs(ObjectGeneration obj,
        APILine line, Context lhsContext, Context rhsContext)
    {
        return $"{line.GetParameterName(obj, lhsContext)}: {(line.PassthroughConverter(obj, lhsContext, rhsContext) ?? line.GetParameterName(obj, rhsContext).Result)}";
    }

    private string CombineResults(
        string lhs,
        string rhs)
    {
        return $"{lhs}: {rhs}";
    }

    private IEnumerable<string> WrapAccessors(
        APILine[] memberNames,
        APILine[] accessors)
    {
        if (memberNames.Length != accessors.Length)
        {
            throw new ArgumentException();
        }
        for (int i = 0; i < memberNames.Length; i++)
        {
            yield return $"{memberNames[i]}: {accessors[i]}";
        }
    }

    public IEnumerable<string> WrapAccessors(ObjectGeneration obj, TranslationDirection dir, Context context, IAPIItem[] accessors) =>
        ZipAccessors(
                Get(dir).IterateAPI(obj, dir, context).Where((a) => a.Public).Select(a => a.API),
                accessors.Select(api => api.Resolve(obj, context)))
            .Select(api =>
                CombineResults(
                    api.lhs.GetParameterName(obj, context),
                    api.rhs.GetParameterName(obj, context)));

    public IEnumerable<string> WrapFinalAccessors(ObjectGeneration obj, TranslationDirection dir, Context context, IAPIItem[] accessors) =>
        ZipAccessors(
                Get(dir).IterateAPI(obj, dir, context).Select(a => a.API),
                accessors
                    .Select(api => api.GetParameterName(obj, context))
                    .And(
                        Get(dir).CustomAPI
                            .Where(a => !a.Public)
                            .Where(a => a.API.When(obj, dir))
                            .Select(a => a.DefaultFallback)))
            .Select(api =>
                CombineResults(
                    api.lhs.GetParameterName(obj, context),
                    api.rhs));
}

public class TranslationFunnel
{
    public TranslationModuleAPI FunneledTo { get; private set; }
    public Action<ObjectGeneration, StructuredStringBuilder, InternalTranslation> OutConverter { get; private set; }
    public Action<ObjectGeneration, StructuredStringBuilder, InternalTranslation> InConverter { get; private set; }

    public TranslationFunnel(
        TranslationModuleAPI funnelTo,
        Action<ObjectGeneration, StructuredStringBuilder, InternalTranslation> outConverter,
        Action<ObjectGeneration, StructuredStringBuilder, InternalTranslation> inConverter)
    {
        FunneledTo = funnelTo;
        OutConverter = outConverter;
        InConverter = inConverter;
    }
}