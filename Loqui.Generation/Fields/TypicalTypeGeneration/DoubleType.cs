using System;

namespace Loqui.Generation
{
    public class DoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(double);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.Access}.EqualsWithin({rhsAccessor.Access})";
        }
    }
}
