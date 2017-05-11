using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class P2IntNullType : P2IntType
    {
        public override string TypeName => $"{base.TypeName}?";
    }
}
