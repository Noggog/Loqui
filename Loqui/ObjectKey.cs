using System;

namespace Loqui
{
    public struct ObjectKey : IEquatable<ObjectKey>
    {
        public readonly ProtocolKey ProtocolKey;
        public readonly ushort MessageID;
        public readonly ushort Version;

        public ObjectKey(
            ProtocolKey protocolKey,
            ushort msgID,
            ushort version)
        {
            this.ProtocolKey = protocolKey;
            this.MessageID = msgID;
            this.Version = version;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectKey)) return false;
            return Equals((ObjectKey)obj);
        }

        public bool Equals(ObjectKey other)
        {
            return this.MessageID == other.MessageID
                && this.Version == other.Version
                && this.ProtocolKey.Equals(other.ProtocolKey);
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(
                this.MessageID,
                this.Version)
                .CombineHashCode(this.ProtocolKey);
        }

        public override string ToString()
        {
            return $"ObjectKey (p{this.ProtocolKey.ProtocolID}, m{this.MessageID}, v{this.Version})";
        }
    }
}
