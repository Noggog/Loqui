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

        public static bool DefaultSwitch<T>(
            T rhsItem,
            bool rhsHasBeenSet,
            T defItem,
            bool defHasBeenSet,
            out T outRhsItem,
            out T outDefItem)
        {
            if (rhsHasBeenSet)
            {
                outRhsItem = rhsItem;
                outDefItem = defItem;
                return true;
            }
            else if (defHasBeenSet)
            {
                outRhsItem = defItem;
                outDefItem = default(T);
                return true;
            }
            else
            {
                outRhsItem = default(T);
                outDefItem = default(T);
                return false;
            }
        }

        public static void SetToWithDefault<T>(
            ref T item,
            ref bool itemHasBeenSet,
            ref T rhsItem,
            bool rhsHasBeenSet,
            ref T defItem,
            bool defHasBeenSet,
            Func<T, T, T> converter)
        {
            if (rhsHasBeenSet)
            {
                item = converter(rhsItem, defItem);
                itemHasBeenSet = true;
            }
            else if (defHasBeenSet)
            {
                item = converter(defItem, default(T));
                itemHasBeenSet = true;
            }
            else
            {
                itemHasBeenSet = false;
                item = default(T);
            }
        }

        public static void SetToWithDefault<T>(
            ref T item,
            ref bool itemHasBeenSet,
            T rhsItem,
            bool rhsHasBeenSet,
            T defItem,
            bool defHasBeenSet)
        {
            if (rhsHasBeenSet)
            {
                item = rhsItem;
                itemHasBeenSet = true;
            }
            else if (defHasBeenSet)
            {
                item = defItem;
                itemHasBeenSet = true;
            }
            else
            {
                itemHasBeenSet = false;
                item = default(T);
            }
        }
    }
}
