using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class P3IntType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(P3Int); }
        }

        protected override string GenerateDefaultValue()
        {
            return "new " + TypeName + "(" + DefaultValue + ")";
        }
    }
}
