using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public static class IHasBeenSetExt
    {
        public static bool Equals<T>(this IHasBeenSetItemGetter<T> lhs, IHasBeenSetItemGetter<T> rhs, Func<T, T, bool> equalCheck)
        {
            if (lhs.HasBeenSet == rhs.HasBeenSet)
            {
                if (lhs.HasBeenSet)
                {
                    var r = equalCheck(lhs.Item, rhs.Item);
                    return r;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public static MaskItem<bool, M> LoquiEqualsHelper<T, M>(
            this IHasBeenSetItemGetter<T> lhs,
            IHasBeenSetItemGetter<T> rhs,
            Func<T, T, M> maskGetter)
            where M : IMask<bool>
        {
            var ret = new MaskItem<bool, M>();
            if (lhs.HasBeenSet == lhs.HasBeenSet)
            {
                if (lhs.HasBeenSet)
                {
                    ret.Specific = maskGetter(lhs.Item, rhs.Item);
                    ret.Overall = ret.Specific.AllEqual((b) => b);
                }
                else
                {
                    ret.Overall = true;
                }
            }
            else
            {
                ret.Overall = false;
            }
            return ret;
        }

        public static MaskItem<bool, M> LoquiEqualsHelper<T, M>(
            this T lhs,
            T rhs,
            Func<T, T, M> maskGetter)
            where M : IMask<bool>
        {
            var ret = new MaskItem<bool, M>();
            ret.Specific = maskGetter(lhs, rhs);
            ret.Overall = ret.Specific.AllEqual((b) => b);
            return ret;
        }
    }
}
