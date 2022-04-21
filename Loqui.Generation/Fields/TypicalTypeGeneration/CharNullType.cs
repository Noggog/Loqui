namespace Loqui.Generation;

public class CharNullType : PrimitiveType
{
    public override Type Type(bool getter) => typeof(char?);
}