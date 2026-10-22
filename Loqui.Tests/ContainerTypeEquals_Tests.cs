using Loqui.Generation;
using Noggog.StructuredStrings;
using Xunit;

namespace Loqui.Tests;

public class ContainerTypeEquals_Tests
{
    [Fact]
    public void ListOfByteArraysComparesTheirBytes()
    {
        var generated = GenerateEquals(new ByteArrayType());

        Assert.Contains("(l, r) => MemoryExtensions.SequenceEqual(l.Span, r.Span)", generated);
    }

    [Fact]
    public void ListOfLoquiObjectsWithoutTargetUsesTheirEquals()
    {
        var generated = GenerateEquals(new LoquiType());

        Assert.Contains("if (!lhs.Items.SequenceEqualNullable(rhs.Items)) return false;", generated);
    }

    private static string GenerateEquals(TypeGeneration subType)
    {
        FileInfo fileInfo = new("fred");
        LoquiGenerator loquiGenerator = new();
        ProtocolGeneration protoGen = new(loquiGenerator, new ProtocolKey(), fileInfo.Directory);
        ListType list = new() { Name = "Items" };
        list.SetObjectGeneration(new ClassGeneration(loquiGenerator, protoGen, fileInfo), true);
        list.SubTypeGeneration = subType;

        var sb = new StructuredStringBuilder();
        list.GenerateForEquals(sb, new Accessor("lhs.Items"), new Accessor("rhs.Items"), new Accessor("equalsMask"));
        return sb.ToString();
    }
}
