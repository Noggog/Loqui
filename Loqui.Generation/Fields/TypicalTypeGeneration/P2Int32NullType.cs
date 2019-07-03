using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class P2Int32NullType : P2Int32Type
    {
        public override string TypeName(bool getter) => $"{base.TypeName(getter)}?";

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }
    }
}
