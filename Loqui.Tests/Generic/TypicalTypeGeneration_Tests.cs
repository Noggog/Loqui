using Loqui.Generation;

namespace Loqui.Tests;

public abstract class TypicalTypeGeneration_Tests<T> : TypeGeneration_Tests<T>
    where T : TypicalTypeGeneration, new()
{

}