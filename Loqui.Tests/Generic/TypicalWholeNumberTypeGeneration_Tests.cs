using Loqui.Generation;
using System;

namespace Loqui.Tests
{
    public abstract class TypicalWholeNumberTypeGeneration_Tests<T> : TypicalRangedTypeGeneration_Tests<T>
        where T : TypicalWholeNumberTypeGeneration, new()
    {

    }
}
