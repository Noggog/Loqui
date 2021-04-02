using Loqui.Generation;
using Xunit;

namespace Loqui.Tests
{
    public abstract class PrimitiveType_Tests<T> : TypicalTypeGeneration_Tests<T>
        where T : PrimitiveType, new()
    {

    }

}
