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
            this ISetList<TItem> not,
            IReadOnlySetList<TGetter> rhs,
            IReadOnlySetList<TGetter> def,
            Func<TGetter, TGetter, TItem> converter)
            where TItem : TGetter
        {
            if (rhs.HasBeenSet)
            {
                if (def == null)
                {
                    not.SetTo(
                        rhs.Select((t) => converter(t, default)));
                }
                else
                {
                    int i = 0;
                    not.SetTo(
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
                not.SetTo(
                    def.Select((t) => converter(t, default)));
            }
            else
            {
                not.Unset();
            }
        }

        public static void SetToWithDefault<TItem, TGetter>(
            this IList<TItem> not,
            IReadOnlyList<TGetter> rhs,
            IReadOnlyList<TGetter> def,
            Func<TGetter, TGetter, TItem> converter)
        {
            if (def == null)
            {
                not.SetTo(
                    rhs.Select((t) => converter(t, default)));
            }
            else
            {
                int i = 0;
                not.SetTo(
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
