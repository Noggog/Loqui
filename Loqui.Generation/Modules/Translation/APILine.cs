namespace Loqui.Generation;

public delegate bool ApiWhen(ObjectGeneration objGen, TranslationDirection dir);
public class APILine : IEquatable<APILine>, IAPIItem
{
    public string NicknameKey { get; }
    public Func<ObjectGeneration, Context, APIResult> Resolver { get; }
    public ApiWhen When { get; }
    public Func<ObjectGeneration, Context, Context, string?> PassthroughConverter { get; }

    public APILine(
        string nicknameKey,
        Func<ObjectGeneration, Context, string> resolver,
        ApiWhen when = null,
        Func<ObjectGeneration, Context, Context, string> passthroughConverter = null)
    {
        NicknameKey = nicknameKey;
        Resolver = (obj, context) => new APIResult(this, resolver(obj, context));
        When = when ?? ((obj, input) => true);
        PassthroughConverter = passthroughConverter ?? new Func<ObjectGeneration, Context, Context, string>(
            (o, l, r) => null);
    }

    public APILine(
        string nicknameKey,
        string resolutionString,
        ApiWhen when = null,
        Func<ObjectGeneration, Context, Context, string> passthroughConverter = null)
    {
        NicknameKey = nicknameKey;
        Resolver = (obj, context) => new APIResult(this, resolutionString);
        When = when ?? ((obj, input) => true);
        PassthroughConverter = passthroughConverter ?? new Func<ObjectGeneration, Context, Context, string>(
            (o, l, r) => null);
    }

    public bool TryResolve(ObjectGeneration obj, TranslationDirection dir, Context context, out APIResult line)
    {
        var get = When(obj, dir);
        if (!get)
        {
            line = default;
            return false;
        }
        line = Resolver(obj, context);
        return true;
    }

    public bool TryGetParameterName(ObjectGeneration obj, TranslationDirection dir, Context context, out APIResult result)
    {
        var get = When(obj, dir);
        if (!get)
        {
            result = default;
            return false;
        }
        result = this.GetParameterName(obj, context);
        return true;
    }

    public bool TryGetPassthrough(ObjectGeneration obj, TranslationDirection dir, Context context, out string result)
    {
        var get = When(obj, dir);
        if (!get)
        {
            result = default;
            return false;
        }
        var name = this.GetParameterName(obj, context);
        result = $"{name}: {name}";
        return true;
    }

    public APIResult Resolve(ObjectGeneration obj, Context context) => Resolver(obj, context);

    public override bool Equals(object obj)
    {
        if (!(obj is APILine rhs)) return false;
        return obj.Equals(rhs);
    }

    public bool Equals(APILine other)
    {
        return string.Equals(NicknameKey, other.NicknameKey);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NicknameKey);
    }
}