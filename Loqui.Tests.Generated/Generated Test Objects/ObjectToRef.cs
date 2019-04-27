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
        public static readonly ObjectToRef TYPICAL_VALUE_2 = new ObjectToRef()
        {
            KeyField = 16,
            SomeField = true
        };
        public static readonly ObjectToRef TYPICAL_VALUE_3 = new ObjectToRef()
        {
            KeyField = 9,
            SomeField = false
        };

    }
}
