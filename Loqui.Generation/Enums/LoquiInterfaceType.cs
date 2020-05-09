using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public enum LoquiInterfaceType
    {
        Direct,
        ISetter,
        IGetter
    }

    public enum LoquiInterfaceDefinitionType
    {
        Direct,
        ISetter,
        IGetter,
        Dual
    }
}
