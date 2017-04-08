using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class P3DoubleNullType : P3DoubleType
    {
        public override string TypeName => $"{base.TypeName}?";
    }
}
