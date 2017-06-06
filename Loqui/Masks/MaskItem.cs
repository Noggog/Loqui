using System;

namespace Loqui
{
    public class MaskItem<T, V> : IEquatable<MaskItem<V, T>>
    {
        public T Overall;
        public V Specific;

        public MaskItem()
        {
        }
        
        public MaskItem(
            T overall,
            V specific)
        {
            this.Overall = overall;
            this.Specific = specific;
        }

        public bool Equals(MaskItem<V, T> other)
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
    }

    public static class MaskItemExt
    {
        public static void ToString<T, V>(this MaskItem<T, V> maskItem, FileGeneration fg)
        {
            if (maskItem == null) return;
            if (maskItem.Overall != null)
            {
                fg.AppendLine(maskItem.Overall.ToString());
            }
            if (maskItem.Specific != null)
            {
                fg.AppendLine(maskItem.Specific.ToString());
            }
        }
    }
}
