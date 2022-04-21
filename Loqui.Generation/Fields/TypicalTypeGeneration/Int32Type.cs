namespace Loqui.Generation;

public class Int32Type : TypicalWholeNumberTypeGeneration
{
    public override Type Type(bool getter) => typeof(Int32);
}