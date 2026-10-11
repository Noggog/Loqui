using Loqui.Generation;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Xunit;

namespace Loqui.Tests;

public class TypicalTypeGenerationDefault_Tests
{
    [Fact]
    public async Task BackingFieldInitializedToDefault()
    {
        var field = new FloatType();
        await field.Load(new XElement("Float",
            new XAttribute(Constants.NAME, "Distance"),
            new XAttribute("default", "10000f")));
        field.PreSetEvent += hook => hook.AppendLine("this.State = 0;");
        var sb = new StructuredStringBuilder();

        await field.GenerateForClass(sb);

        Assert.Contains(sb, line => line.Trim() == "private Single _Distance = DistanceDefault;");
    }
}
