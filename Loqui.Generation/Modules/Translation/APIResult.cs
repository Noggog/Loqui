namespace Loqui.Generation;

public class APIResult : IAPIItem
{
    public string NicknameKey { get; }
    public string Result { get; }

    public APIResult(
        IAPIItem sourceLine,
        string result)
    {
        NicknameKey = sourceLine.NicknameKey;
        Result = result;
    }

    public APIResult(
        string nicknameKey,
        string result)
    {
        NicknameKey = nicknameKey;
        Result = result;
    }

    public APIResult Resolve(ObjectGeneration obj) => this;

    public override string ToString()
    {
        return Result;
    }
}