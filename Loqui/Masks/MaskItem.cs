using System;
using System.Diagnostics;

namespace Loqui
{
    public class MaskItem<T, V> : IEquatable<MaskItem<T, V>>
    {
        public T Overall;
        public V Specific;

        public MaskItem()
        {
        }
        
        [DebuggerStepThrough]
        public MaskItem(
            T overall,
            V specific)
        {
            this.Overall = overall;
            this.Specific = specific;
        }

        public bool Equals(MaskItem<T, V> other)
        {
            return object.Equals(this.Overall, other.Overall)
                && object.Equals(this.Specific, other.Specific);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MaskItem<T, V>)) return false;
            return Equals((MaskItem<T, V>)obj);
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.Overall)
                .CombineHashCode(this.Specific);
        }

        public static MaskItem<T, V> WrapValue(V val)
        {
            if (val == null) return null;
            return new MaskItem<T, V>(default(T), val);
        }
    }

    public static class MaskItemExt
    {
        public static void ToString<T, V>(this MaskItem<T, V> maskItem, FileGeneration fg)
        {
            if (maskItem == null) return;
            if (maskItem.Overall != null)
            {
                if (maskItem.Overall is IPrintable printable)
                {
                    printable.ToString(fg);
                }
                else
                {
                    fg.AppendLine(maskItem.Overall.ToString());
                }
            }
            if (maskItem.Specific != null)
            {
                if (maskItem.Specific is IPrintable printable)
                {
                    printable.ToString(fg);
                }
                else
                {
                    fg.AppendLine(maskItem.Specific.ToString());
                }
            }
        }

        public static MaskItem<Exception, R> Bubble<T, R>(this MaskItem<Exception, T> item)
            where T : R
        {
            if (item == null) return null;
            return new MaskItem<Exception, R>(item.Overall, item.Specific);
        }

        public static MaskItem<bool, M> Factory<M>(M mask, bool includeOnlyFailures)
            where M : IMask<bool>
        {
            var allEqual = mask.AllEqual(b => b);
            if (!allEqual || !includeOnlyFailures)
            {
                return new MaskItem<bool, M>()
                {
                    Overall = allEqual,
                    Specific = mask
                };
            }
            else
            {
                return default;
            }
        }
    }
}
