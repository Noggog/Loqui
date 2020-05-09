using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class P2FloatType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(P2Float);

        protected override string GenerateDefaultValue() => $"new {TypeName(getter: false)}({DefaultValue})";

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.Equals({rhsAccessor.DirectAccess})";
        }
    }
}
