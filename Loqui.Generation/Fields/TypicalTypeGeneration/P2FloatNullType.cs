using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class P2FloatNullType : P2FloatType
    {
        public override string TypeName => $"{base.TypeName}?";
    }
}
