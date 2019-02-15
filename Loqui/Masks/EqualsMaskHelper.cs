using CSharpExt.Rx;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui
{
    public static class EqualsMaskHelper
    {
        public enum Include
        {
            All,
            OnlyFailures,
        }

        public static MaskItem<bool, M> EqualsHelper<T, M>(
            this T lhs,
            T rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            var ret = new MaskItem<bool, M>();
            EqualsHelper(
                ret,
                lhs,
                rhs,
                maskGetter,
                include);
            return ret;
        }

        public static void EqualsHelper<T, M>(
            MaskItem<bool, M> maskItem,
            T lhs,
            T rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            if (lhs == null && rhs == null)
            {
                maskItem.Overall = true;
                maskItem.Specific = default;
            }
            else if (lhs == null || rhs == null)
            {
                maskItem.Overall = false;
                maskItem.Specific = default;
            }
            var mask = maskGetter(lhs, rhs);
            maskItem.Overall = mask.AllEqual((b) => b);
            if (!maskItem.Overall || include == Include.All)
            {
                maskItem.Specific = mask;
            }
        }

        public static void EqualsHelper<K, T, M>(
            MaskItemIndexed<K, bool, M> maskItem,
            T lhs,
            T rhs,
            Func<K, T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            if (lhs == null && rhs == null)
            {
                maskItem.Overall = true;
                maskItem.Specific = default;
            }
            else if (lhs == null || rhs == null)
            {
                maskItem.Overall = false;
                maskItem.Specific = default;
            }
            var mask = maskGetter(maskItem.Index, lhs, rhs);
            maskItem.Overall = mask.AllEqual((b) => b);
            if (!maskItem.Overall || include == Include.All)
            {
                maskItem.Specific = mask;
            }
        }

        public static MaskItem<bool, M> EqualsHelper<T, M>(
            bool lhsHas,
            bool rhsHas,
            T lhs,
            T rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            if (lhs == null && rhs == null)
            {
                return new MaskItem<bool, M>(true, default);
            }
            else if (lhs == null || rhs == null)
            {
                return new MaskItem<bool, M>(false, default);
            }
            var ret = new MaskItem<bool, M>();
            var mask = maskGetter(lhs, rhs);
            ret.Overall = lhsHas == rhsHas && mask.AllEqual((b) => b);
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = mask;
            }
            return ret;
        }

        #region List
        public static MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>> CollectionEqualsHelper<T, M>(
            this IObservableSetList<T> lhs,
            IObservableSetList<T> rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            if (lhs.HasBeenSet != rhs.HasBeenSet)
            {
                switch (include)
                {
                    case Include.All:
                        break;
                    case Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            var ret = new MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>>();
            int index = 0;
            var masks = lhs.SelectAgainst<T, MaskItemIndexed<bool, M>>(
                rhs,
                (l, r) =>
                {
                    MaskItemIndexed<bool, M> itemRet = new MaskItemIndexed<bool, M>(index++);
                    EqualsHelper(itemRet, l, r, maskGetter, include);
                    return itemRet;
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Overall)
                .ToArray();
            ret.Overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>> CollectionEqualsHelper<T>(
            this IObservableSetList<T> lhs,
            IObservableSetList<T> rhs,
            Func<T, T, bool> maskGetter,
            Include include)
        {
            if (lhs.HasBeenSet != rhs.HasBeenSet)
            {
                switch (include)
                {
                    case Include.All:
                        break;
                    case Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            var ret = new MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>>();
            int index = 0;
            var masks = lhs.SelectAgainst<T, (int Index, bool EqualValues)>(
                rhs,
                (l, r) =>
                {
                    return (index++, maskGetter(l, r));
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.EqualValues)
                .ToArray();
            ret.Overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.EqualValues);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>> CollectionEqualsHelper<T, M>(
            this IObservableList<T> lhs,
            IObservableSetList<T> rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            var ret = new MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>>();
            int index = 0;
            var masks = lhs.SelectAgainst<T, MaskItemIndexed<bool, M>>(
                rhs, 
                (l, r) =>
                {
                    MaskItemIndexed<bool, M> itemRet = new MaskItemIndexed<bool, M>(index++);
                    EqualsHelper(itemRet, l, r, maskGetter, include);
                    return itemRet;
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Overall)
                .ToArray();
            ret.Overall = countEqual;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>> CollectionEqualsHelper<T>(
            this IObservableList<T> lhs,
            IObservableSetList<T> rhs,
            Func<T, T, bool> maskGetter,
            Include include)
        {
            var ret = new MaskItem<bool, IEnumerable<(int, bool)>>();
            int index = 0;
            var masks = lhs.SelectAgainst<T, (int Index, bool EqualValues)>(
                rhs, 
                (l, r) =>
                {
                    return (index++, maskGetter(l, r));
                }, out var countEqual)
                .Where(i => include == Include.All || !i.EqualValues)
                .ToArray();
            ret.Overall = countEqual;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.EqualValues);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }
        #endregion

        #region Dict
        public static MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>> DictEqualsHelper<K, T, M>(
            this IObservableSetCache<T, K> lhs,
            IObservableSetCache<T, K> rhs,
            Func<K, T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            if (lhs.HasBeenSet != rhs.HasBeenSet)
            {
                switch (include)
                {
                    case Include.All:
                        break;
                    case Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            var ret = new MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>>();
            var masks = lhs.SelectAgainst<K, T, MaskItemIndexed<K, bool, M>>(
                rhs, 
                (k, l, r) =>
                {
                    MaskItemIndexed<K, bool, M> itemRet = new MaskItemIndexed<K, bool, M>(k);
                    EqualsHelper(itemRet, l, r, maskGetter, include);
                    return itemRet;
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value.Overall)
                .Select(kv => kv.Value)
                .ToArray();
            ret.Overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
                
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>> DictEqualsHelper<K, T>(
            this IObservableSetCache<T, K> lhs,
            IObservableSetCache<T, K> rhs,
            Func<K, T, T, bool> maskGetter,
            Include include)
        {
            if (lhs.HasBeenSet != rhs.HasBeenSet)
            {
                switch (include)
                {
                    case Include.All:
                        break;
                    case Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            var ret = new MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>>();
            var masks = lhs.SelectAgainst<K, T, bool>(
                rhs,
                (k, l, r) =>
                {
                    return maskGetter(k, l, r);
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value)
                .ToArray();
            ret.Overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Value);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>> DictEqualsHelper<K, T, M>(
            this IObservableCache<T, K> lhs,
            IObservableCache<T, K> rhs,
            Func<K, T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
            var ret = new MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>>();
            var masks = lhs.SelectAgainst<K, T, MaskItemIndexed<K, bool, M>>(
                rhs, 
                (k, l, r) =>
                {
                    MaskItemIndexed<K, bool, M> itemRet = new MaskItemIndexed<K, bool, M>(k);
                    EqualsHelper(itemRet, l, r, maskGetter, include);
                    return itemRet;
                },
                out var countEqual)
                .Select(kv => kv.Value)
                .Where(i => include == Include.All || !i.Overall)
                .ToArray();
            ret.Overall = countEqual;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }

        public static MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>> DictEqualsHelper<K, T>(
            this IObservableCache<T, K> lhs,
            IObservableCache<T, K> rhs,
            Func<K, T, T, bool> maskGetter,
            Include include)
        {
            var ret = new MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>>();
            var masks = lhs.SelectAgainst<K, T, bool>(
                rhs, 
                (k, l, r) =>
                {
                    return maskGetter(k, l, r);
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value)
                .ToArray();
            ret.Overall = countEqual;
            if (ret.Overall)
            {
                switch (include)
                {
                    case Include.All:
                        ret.Overall = masks.All((b) => b.Value);
                        break;
                    case Include.OnlyFailures:
                        ret.Overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!ret.Overall || include == Include.All)
            {
                ret.Specific = masks;
            }
            return ret;
        }
        #endregion
    }
}
