using Loqui.Generation;

namespace Loqui.Tests;

public abstract class WrapperType_Tests<T> : TypeGeneration_Tests<T>
    where T : WrapperType, new()
{

}