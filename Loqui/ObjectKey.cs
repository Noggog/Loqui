using Noggog;
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
            var hash = new HashCode();
            hash.Add(MessageID);
            hash.Add(Version);
            hash.Add(ProtocolKey);
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"ObjectKey ({this.ProtocolKey.Namespace}, m{this.MessageID}, v{this.Version})";
        }
    }
}
