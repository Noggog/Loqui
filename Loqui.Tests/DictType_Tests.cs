using Loqui.Generation;
using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests
{
    // TODO public class DictType_Tests : DictType_Typical

    /*
    public class DictType_KeyedValue_Tests : TypeGeneration_Tests<DictType_KeyedValue>
    {
        public override DictType_KeyedValue Thing
        {
            get
            {
                DictType_KeyedValue dictType = new();

                FileInfo fileInfo = new("fred");
                LoquiGenerator loquiGenerator = new();
                ProtocolKey protocolKey = new();
                ProtocolGeneration protoGen = new(loquiGenerator, protocolKey, fileInfo.Directory);

                dictType.SetObjectGeneration(new ClassGeneration(loquiGenerator, protoGen, fileInfo), true);

                dictType.KeyTypeGen = new UInt32Type();
                dictType.ValueTypeGen = new();

                return dictType;
            }
        }

        public override TheoryData<XElement> ValidElements => new()
        {
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32")),
                new XElement(XName.Get(Constants.KEYED_VALUE, LoquiGenerator.Namespace),
                    new XElement("Uint32"))),
        };

#pragma warning disable xUnit1024 // Test methods cannot have overloads
        public new async void TestValidLoad(XElement valid)
#pragma warning restore xUnit1024 // Test methods cannot have overloads
        {
            // FIXME find out how to declare a valid KeyedValue Dict.
        }

        public override TheoryData<XElement> InvalidElements => new()
        {
            new XElement("Dict"),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp")),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace))),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32"))),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32")),
                new XElement(XName.Get("Value", LoquiGenerator.Namespace))),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32")),
                new XElement(XName.Get("Value", LoquiGenerator.Namespace),
                    new XElement("UInt32"))),
        };
        
    }
    */

    public class DictType_Typical_Tests : TypeGeneration_Tests<DictType_Typical>
    {
        public override DictType_Typical Thing
        {
            get
            {
                DictType_Typical dictType = new();

                FileInfo fileInfo = new("fred");
                LoquiGenerator loquiGenerator = new();
                ProtocolKey protocolKey = new();
                ProtocolGeneration protoGen = new(loquiGenerator, protocolKey, fileInfo.Directory);

                dictType.SetObjectGeneration(new ClassGeneration(loquiGenerator, protoGen, fileInfo), true);

                dictType.KeyTypeGen = new UInt32Type();
                dictType.ValueTypeGen = new UInt32Type();

                return dictType;
            }
        }

        public override TheoryData<XElement> ValidElements => new()
        {
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32")),
                new XElement(XName.Get("Value", LoquiGenerator.Namespace),
                    new XElement("UInt32"))),
        };

        public override TheoryData<XElement> InvalidElements => new()
        {
            new XElement("Dict"),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp")),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace))),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32"))),
            new XElement("Dict",
                new XAttribute(Constants.NAME, "DictTypeProp"),
                new XElement(XName.Get("Key", LoquiGenerator.Namespace),
                    new XElement("UInt32")),
                new XElement(XName.Get("Value", LoquiGenerator.Namespace))),
        };

    }
}
