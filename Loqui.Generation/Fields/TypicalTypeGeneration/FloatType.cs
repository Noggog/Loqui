using Noggog;
using System;

namespace Loqui.Generation
{
    public class FloatType : TypicalFloatNumberTypeGeneration
    {
        public override Type Type(bool getter) => typeof(float);
        public override string RangeTypeName(bool getter) => nameof(RangeFloat);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.EqualsWithin({rhsAccessor.DirectAccess})";
        }
    }
}
