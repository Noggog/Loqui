namespace Loqui.Generation;

public class TimeOnlyNullType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(TimeOnly?);
}