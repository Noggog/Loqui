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
}
