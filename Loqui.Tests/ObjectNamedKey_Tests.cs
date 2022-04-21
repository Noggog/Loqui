using Loqui.Generation;
using Xunit;

namespace Loqui.Tests;

public class ObjectNamedKey_Tests
{
    static ProtocolKey Protocol = new ProtocolKey("Protocol");
    static ProtocolKey LargeProtocol = new ProtocolKey("Protocol.SubProtocol");
    static ProtocolKey FallbackProtocol = new ProtocolKey("Fallback");
    static string ObjectName = "Test";

    [Fact]
    public void BasicEquals()
    {
        var lhs = new ObjectNamedKey(
            new ProtocolKey("Test"),
            "Object");
        var rhs = new ObjectNamedKey(
            new ProtocolKey("Test"),
            "Object");
        Assert.Equal(lhs, rhs);
    }

    [Fact]
    public void TryFactory_Empty()
    {
        Assert.False(ObjectNamedKey.TryFactory(string.Empty, FallbackProtocol, out var key));
    }

    [Fact]
    public void TryFactory_Null()
    {
        Assert.False(ObjectNamedKey.TryFactory(null, FallbackProtocol, out var key));
    }

    [Fact]
    public void TryFactory_OnlyName()
    {
        Assert.True(ObjectNamedKey.TryFactory(ObjectName, FallbackProtocol, out var key));
        Assert.Equal(key.ProtocolKey, FallbackProtocol);
        Assert.Equal(key.Name, ObjectName);
    }

    [Fact]
    public void TryFactory_Full()
    {
        Assert.True(ObjectNamedKey.TryFactory($"{Protocol.Namespace}.{ObjectName}", FallbackProtocol, out var key));
        Assert.Equal(key.ProtocolKey, Protocol);
        Assert.Equal(key.Name, ObjectName);
    }

    [Fact]
    public void TryFactory_ExtraFull()
    {
        Assert.True(ObjectNamedKey.TryFactory($"{LargeProtocol.Namespace}.{ObjectName}", FallbackProtocol, out var key));
        Assert.Equal(key.ProtocolKey, LargeProtocol);
        Assert.Equal(key.Name, ObjectName);
    }
}