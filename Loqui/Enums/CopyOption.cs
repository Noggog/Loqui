using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public enum CopyOption
    {
        // Replace target with reference from the copy source
        Reference,
        // Skip field and do nothing
        Skip,
        // Copy fields into target
        CopyIn,
        // Make a copy and replace target
        MakeCopy
    }
}
