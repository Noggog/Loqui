using Loqui.Generation;
using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests
{
    public class ArrayType_Tests : ListType_Tests<ArrayType>
    {
        public override ArrayType Thing
        {
            get
            {
                ArrayType arrayType = new();

                FileInfo fileInfo = new("fred");
                LoquiGenerator loquiGenerator = new();
                ProtocolKey protocolKey = new();
                ProtocolGeneration protoGen = new(loquiGenerator, protocolKey, fileInfo.Directory);

                arrayType.SetObjectGeneration(new ClassGeneration(loquiGenerator, protoGen, fileInfo), true);
                arrayType.SubTypeGeneration = new UInt32Type();

                return arrayType;
            }
        }

        public override TheoryData<XElement> ValidElements => new() 
        {
             new XElement("Array",
                new XAttribute(Constants.NAME, "ArrayTypeProp"),
                new XElement("UInt32")),
        };

        public override TheoryData<XElement> InvalidElements => new()
        {
            new XElement("foo"),
            new XElement("foo", new XAttribute(Constants.NAME, "ArrayTypeProp")),
        };
    }

}
