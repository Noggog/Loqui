using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class P2Int32NullType : P2Int32Type
    {
        public override string TypeName => $"{base.TypeName}?";
    }
}
