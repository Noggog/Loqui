using System;

namespace Loqui
{
    public struct ProtocolKey : IEquatable<ProtocolKey>
    {
        public readonly string Namespace;

        public ProtocolKey(string nameSpace)
        {
            this.Namespace = nameSpace;
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is ProtocolKey rhs)) return false;
            return Equals(rhs);
        }

        public bool Equals(ProtocolKey other)
        {
            return this.Namespace == other.Namespace;
        }

        public override int GetHashCode()
        {
            return this.Namespace.GetHashCode();
        }

        public override string ToString()
        {
            return $"ProtocolKey ({Namespace})";
        }
    }
}
