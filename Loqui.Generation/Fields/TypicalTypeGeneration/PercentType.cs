using Noggog;
using System;

namespace Loqui.Generation
{
    public class PercentType : DoubleType
    {
        public override Type Type(bool getter) => typeof(Percent);

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.Equals({rhsAccessor.DirectAccess})";
        }
    }
}
