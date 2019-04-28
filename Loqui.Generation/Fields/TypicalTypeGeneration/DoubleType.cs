using System;

namespace Loqui.Generation
{
    public class DoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type => typeof(double);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})";
        }
    }
}
