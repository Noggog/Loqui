namespace Loqui.Generation;

public class UInt16NullType : TypicalWholeNumberTypeGeneration
{
    public override Type Type(bool getter) => typeof(UInt16?);
}