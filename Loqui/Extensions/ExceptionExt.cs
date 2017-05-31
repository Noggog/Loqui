using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public static class ExceptionExt
    {
        public static Exception Combine(this Exception lhs, Exception rhs)
        {
            if (lhs == null && rhs == null) return null;
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            var lhsAgg = lhs as AggregateException;
            var rhsAgg = rhs as AggregateException;
            if (lhsAgg != null && rhsAgg != null)
            {
                return new AggregateException(
                    lhsAgg.InnerExceptions.And(rhsAgg.InnerExceptions));
            }
            if (lhsAgg != null)
            {
                return new AggregateException(
                    lhsAgg.InnerExceptions.And(rhs));
            }
            if (rhsAgg != null)
            {
                return new AggregateException(
                    rhsAgg.InnerExceptions.And(lhs));
            }
            return new AggregateException(
                lhs,
                rhs);
        }
    }
}
