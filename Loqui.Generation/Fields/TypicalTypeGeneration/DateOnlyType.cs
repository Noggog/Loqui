namespace Loqui.Generation;

public class DateOnlyType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(DateOnly);
}