namespace Loqui;

public struct ProtocolKey : IEquatable<ProtocolKey>
{
    public readonly string Namespace;

    public ProtocolKey(string nameSpace)
    {
        Namespace = nameSpace;
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is ProtocolKey rhs)) return false;
        return Equals(rhs);
    }

    public bool Equals(ProtocolKey other)
    {
        return Namespace == other.Namespace;
    }

    public override int GetHashCode()
    {
        return Namespace.GetHashCode();
    }

    public override string ToString()
    {
        return $"ProtocolKey ({Namespace})";
    }
}