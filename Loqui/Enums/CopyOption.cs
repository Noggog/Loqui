using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public enum CopyOption
    {
        // Skip field and do nothing
        Skip,
        // Replace target with reference from the copy source
        Reference,
        // Copy fields into target
        CopyIn,
        // Make a copy and replace target
        MakeCopy
    }
}
