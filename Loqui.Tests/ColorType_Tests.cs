using Loqui.Generation;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests;

public class ColorType_Tests : PrimitiveType_Tests<ColorType>
{
    public override ColorType Thing => new();

    public override TheoryData<XElement> ValidElements => new()
    {
        new XElement("Color",
            new XAttribute(Constants.NAME, "ColorTypeProp")),
    };

    public override TheoryData<XElement> InvalidElements => new()
    {
        new XElement("Color"),
    };
}