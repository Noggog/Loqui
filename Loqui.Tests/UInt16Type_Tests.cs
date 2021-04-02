using Loqui.Generation;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests
{
    public class UInt16Type_Tests : TypicalWholeNumberTypeGeneration_Tests<UInt16Type>
    {
        public override UInt16Type Thing => new();

        public override TheoryData<XElement> InvalidElements => new()
        {
            new XElement("foo"),
            //new XElement("foo", new XAttribute(Constants.NAME, "UInt16Prop"), new XAttribute(Constants.MIN, -1)),
            //new XElement("foo", new XAttribute(Constants.NAME, "UInt16Prop"), new XAttribute(Constants.MAX, 0x10000))
            new XElement("foo",
                new XAttribute(Constants.NAME, "UInt16Prop"),
                new XAttribute(Constants.MAX, 12.5)),
        };

        public override TheoryData<XElement> ValidElements => new()
        {
            new XElement("foo", new XAttribute(Constants.NAME, "UInt16Prop")),
        };
    }

}
