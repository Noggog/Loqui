using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui
{
    public static class SourceListExt
    {
        public static void SetToWithDefault<TItem, TGetter>(
            this ISetList<TItem> lhs,
            IReadOnlySetList<TGetter> rhs,
            IReadOnlySetList<TGetter> def,
            Func<TGetter, TGetter, TItem> converter)
            where TItem : TGetter
        {
            if (rhs.HasBeenSet)
            {
                if (def == null)
                {
                    lhs.SetTo(
                        rhs.Select((t) => converter(t, default)));
                }
                else
                {
                    int i = 0;
                    lhs.SetTo(
                        rhs.Select((t) =>
                        {
                            TGetter defVal = default;
                            if (def.Count > i)
                            {
                                defVal = def[i];
                            }
                            return converter(t, defVal);
                        }));
                }
            }
            else if (def?.HasBeenSet ?? false)
            {
                lhs.SetTo(
                    def.Select((t) => converter(t, default)));
            }
            else
            {
                lhs.Unset();
            }
        }

        public static void SetToWithDefault<TItem, TGetter>(
            this IList<TItem> lhs,
            IReadOnlyList<TGetter> rhs,
            IReadOnlyList<TGetter> def,
            Func<TGetter, TGetter, TItem> converter)
        {
            if (def == null)
            {
                lhs.SetTo(
                    rhs.Select((t) => converter(t, default)));
            }
            else
            {
                int i = 0;
                lhs.SetTo(
                    rhs.Select((t) =>
                    {
                        TGetter defVal = default;
                        if (def.Count > i)
                        {
                            defVal = def[i];
                        }
                        return converter(t, defVal);
                    }));
            }
        }
    }
}
