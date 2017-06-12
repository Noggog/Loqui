using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Tests
{
    public partial class ObjectToRef
    {
        public static readonly ObjectToRef TYPICAL_VALUE = new ObjectToRef()
        {
            KeyField = 4,
            SomeField = true
        };
    }
}
