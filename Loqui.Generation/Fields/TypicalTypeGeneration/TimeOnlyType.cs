namespace Loqui.Generation;

public class TimeOnlyType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(TimeOnly);
}