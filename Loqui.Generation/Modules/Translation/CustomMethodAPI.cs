namespace Loqui.Generation;

public class CustomMethodAPI
{
    public bool Public { get; private set; }
    public APILine API { get; private set; }
    public APIResult DefaultFallback { get; private set; }

    public CustomMethodAPI(
        bool isPublic,
        APILine api,
        string defaultFallback = null)
    {
        Public = isPublic;
        API = api;
        DefaultFallback = new APIResult(api, defaultFallback);
    }

    public CustomMethodAPI()
    {
    }

    public bool Applies(ObjectGeneration obj, TranslationDirection dir, Context context)
    {
        return API?.TryResolve(obj, dir, context, out var line) ?? true;
    }

    public static CustomMethodAPI FactoryPublic(
        APILine api)
    {
        return new CustomMethodAPI()
        {
            Public = true,
            API = api,
            DefaultFallback = null
        };
    }

    public static CustomMethodAPI FactoryPrivate(
        APILine api,
        string defaultFallback)
    {
        return new CustomMethodAPI()
        {
            Public = false,
            API = api,
            DefaultFallback = new APIResult(api, defaultFallback)
        };
    }

    public bool TryGetPassthrough(ObjectGeneration baseGen, ObjectGeneration obj, TranslationDirection dir, Context context, out string result)
    {
        var get = API.When(obj, dir);
        if (!get)
        {
            result = default;
            return false;
        }
        var name = API.GetParameterName(obj, context);
        if (API.When(baseGen, dir))
        {
            result = $"{name}: {name}";
        }
        else
        {
            result = $"{name}: {DefaultFallback}";
        }
        return true;
    }
}