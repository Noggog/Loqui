using CSharpExt.Rx;
using DynamicData;
using Noggog;
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
                return include == Include.All ? new MaskItem<bool, M>(true, default) : null;
            }
            else if (lhs == null || rhs == null)
            {
                return new MaskItem<bool, M>(false, default);
            }
            var mask = maskGetter(lhs, rhs);
            var overall = lhsHas == rhsHas && mask.AllEqual((b) => b);
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, M>()
                {
                    Overall = overall,
                    Specific = mask
                };
            }
            return null;
        }

        #region List
        public static MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>> CollectionEqualsHelper<T, M>(
            this IReadOnlySetList<T> lhs,
            IObservableSetList<T> rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
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
            var overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>> CollectionEqualsHelper<T>(
            this IReadOnlySetList<T> lhs,
            IReadOnlySetList<T> rhs,
            Func<T, T, bool> maskGetter,
            Include include)
        {
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
            var overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.EqualValues);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>> CollectionEqualsHelper<T, M>(
            this IReadOnlyList<T> lhs,
            IReadOnlyList<T> rhs,
            Func<T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
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
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<MaskItemIndexed<bool, M>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<(int Index, bool EqualValues)>> CollectionEqualsHelper<T>(
            this IReadOnlyList<T> lhs,
            IReadOnlyList<T> rhs,
            Func<T, T, bool> maskGetter,
            Include include)
        {
            int index = 0;
            var masks = lhs.SelectAgainst<T, (int Index, bool EqualValues)>(
                rhs, 
                (l, r) =>
                {
                    return (index++, maskGetter(l, r));
                }, out var countEqual)
                .Where(i => include == Include.All || !i.EqualValues)
                .ToArray();
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.EqualValues);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<(int, bool)>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }
        #endregion

        #region Dict
        public static MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>> DictEqualsHelper<K, T, M>(
            this IReadOnlySetCache<T, K> lhs,
            IReadOnlySetCache<T, K> rhs,
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
            var overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
                
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>> DictEqualsHelper<K, T>(
            this IReadOnlySetCache<T, K> lhs,
            IReadOnlySetCache<T, K> rhs,
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
            var masks = lhs.SelectAgainst<K, T, bool>(
                rhs,
                (k, l, r) =>
                {
                    return maskGetter(k, l, r);
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value)
                .ToArray();
            var overall = countEqual
                && lhs.HasBeenSet == rhs.HasBeenSet;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Value);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>> DictEqualsHelper<K, T, M>(
            this IReadOnlyDictionary<K, T> lhs,
            IReadOnlyDictionary<K, T> rhs,
            Func<K, T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
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
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>> DictEqualsHelper<K, T>(
            this IReadOnlyDictionary<K, T> lhs,
            IReadOnlyDictionary<K, T> rhs,
            Func<K, T, T, bool> maskGetter,
            Include include)
        {
            var masks = lhs.SelectAgainst<K, T, bool>(
                rhs,
                (k, l, r) =>
                {
                    return maskGetter(k, l, r);
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value)
                .ToArray();
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Value);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>> CacheEqualsHelper<K, T, M>(
            this IReadOnlyCache<T, K> lhs,
            IReadOnlyCache<T, K> rhs,
            Func<K, T, T, M> maskGetter,
            Include include)
            where M : IMask<bool>
        {
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
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Overall);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<MaskItemIndexed<K, bool, M>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }

        public static MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>> CacheEqualsHelper<K, T>(
            this IReadOnlyCache<T, K> lhs,
            IReadOnlyCache<T, K> rhs,
            Func<K, T, T, bool> maskGetter,
            Include include)
        {
            var masks = lhs.SelectAgainst<K, T, bool>(
                rhs,
                (k, l, r) =>
                {
                    return maskGetter(k, l, r);
                },
                out var countEqual)
                .Where(i => include == Include.All || !i.Value)
                .ToArray();
            var overall = countEqual;
            if (overall)
            {
                switch (include)
                {
                    case Include.All:
                        overall = masks.All((b) => b.Value);
                        break;
                    case Include.OnlyFailures:
                        overall = !masks.Any();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!overall || include == Include.All)
            {
                return new MaskItem<bool, IEnumerable<KeyValuePair<K, bool>>>()
                {
                    Overall = overall,
                    Specific = masks
                };
            }
            return null;
        }
        #endregion
    }
}
