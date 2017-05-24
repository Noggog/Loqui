using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public static class EnumerableExt
    {
        public static IEnumerable<R> SelectAgainst<T, R>(this IEnumerable<T> lhs, IEnumerable<T> rhs, Func<T, T, R> selector, out bool? countEqual)
        {
            var ret = System.EnumerableExt.SelectAgainst<T, R>(lhs, rhs, selector, out bool countEquals);
            countEqual = countEquals;
            return ret;
        }
    }
}
