using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ColorType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(Color);

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            yield return "System.Drawing";
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.ColorOnlyEquals({rhsAccessor.DirectAccess})";
        }
    }
}
