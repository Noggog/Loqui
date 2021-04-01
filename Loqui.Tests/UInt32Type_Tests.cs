using Loqui.Generation;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests
{
    public class UInt32Type_Tests : TypicalWholeNumberTypeGeneration_Tests<UInt32Type>
    {
        public override UInt32Type Thing => new();

        public override TheoryData<XElement> InvalidElements => new()
        {
            new XElement("foo"),
            //new XElement("foo", new XAttribute(Constants.NAME, "UInt32Prop"), new XAttribute(Constants.MIN, -1)),
            new XElement("foo",
                new XAttribute(Constants.NAME, "UInt32Prop"),
                new XAttribute(Constants.MAX, 0x100000000L)),
            new XElement("foo",
                new XAttribute(Constants.NAME, "UInt32Prop"),
                new XAttribute(Constants.MAX, 12.5)),
        };

        public override TheoryData<XElement> ValidElements => new()
        {
            new XElement("foo", new XAttribute(Constants.NAME, "UInt32Prop")),
        };
    }

}
