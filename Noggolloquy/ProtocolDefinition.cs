using Noggog;
using System;

namespace Noggolloquy
{
    public struct ProtocolDefinition : IEquatable<ProtocolDefinition>
    {
        public readonly ProtocolKey Key;
        public readonly StringCaseAgnostic Nickname;

        public ProtocolDefinition(
            ProtocolKey key,
            string nickname)
        {
            this.Key = key;
            this.Nickname = nickname;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProtocolDefinition)) return false;
            return Equals((ProtocolDefinition)obj);
        }

        public bool Equals(ProtocolDefinition other)
        {
            return this.Key.Equals(other.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Nickname} ({Key.ProtocolID})";
        }
    }
}
