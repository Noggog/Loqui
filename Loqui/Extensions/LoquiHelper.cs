using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Internal
{
    public static class LoquiHelper
    {
        public static object Combine(object lhs, object rhs)
        {
            if (lhs == null && rhs == null) return null;
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            return new object[] { lhs, rhs };
        }
    }
}
