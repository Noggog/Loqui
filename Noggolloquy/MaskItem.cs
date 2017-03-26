using System;

namespace Noggolloquy
{
    public struct MaskItem<T, V> : IEquatable<MaskItem<V, T>>
    {
        public readonly T Overall;
        public readonly V Specific;
        
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
