using Loqui.Generation;

namespace Loqui.Tests
{
    public abstract class ClassType_Tests<T> : TypeGeneration_Tests<T>
        where T : ClassType, new()
    {
    }

}
