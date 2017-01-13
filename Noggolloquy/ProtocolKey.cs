using System;

namespace Noggolloquy
{
    public struct ProtocolKey : IEquatable<ProtocolKey>
    {
        public readonly ushort ProtocolID;

        public ProtocolKey(ushort id)
        {
            this.ProtocolID = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProtocolKey)) return false;
            return Equals((ProtocolKey)obj);
        }

        public bool Equals(ProtocolKey other)
        {
            return this.ProtocolID == other.ProtocolID;
        }

        public override int GetHashCode()
        {
            return this.ProtocolID.GetHashCode();
        }

        public override string ToString()
        {
            return $"ProtocolKey ({ProtocolID})";
        }
    }
}
