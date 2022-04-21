namespace Loqui;

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
        ProtocolKey = protocolKey;
        MessageID = msgID;
        Version = version;
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is ObjectKey)) return false;
        return Equals((ObjectKey)obj);
    }

    public bool Equals(ObjectKey other)
    {
        return MessageID == other.MessageID
               && Version == other.Version
               && ProtocolKey.Equals(other.ProtocolKey);
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
        return $"ObjectKey ({ProtocolKey.Namespace}, m{MessageID}, v{Version})";
    }
}