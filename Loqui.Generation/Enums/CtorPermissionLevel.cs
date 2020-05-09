using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public enum CtorPermissionLevel
    {
        @public,
        @private,
        @protected,
        @internal,
        noGeneration
    }
}
