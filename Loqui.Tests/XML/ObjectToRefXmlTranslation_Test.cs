using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Loqui.Xml;
using Loqui.Tests.Internals;
using Xunit;

namespace Loqui.Tests.XML
{
    public class ObjectToRefXmlTranslation_Test : TypicalXmlTranslation_Test<ObjectToRef, ObjectToRefXmlTranslation_Test>
    {
        public static readonly ObjectToRef TYPICAL_VALUE = ObjectToRef.TYPICAL_VALUE;
        public override string ExpectedName => "Loqui.Tests.ObjectToRef";

        public override ObjectToRef TypicalValue => TYPICAL_VALUE;

        public override IXmlTranslation<ObjectToRef> GetTranslation()
        {
            return new LoquiXmlTranslation<ObjectToRef, ObjectToRef_ErrorMask>();
        }

        public override XElement GetTypicalElement(string name = null)
        {
            var ret = XmlUtility.GetElementNoValue(ExpectedName, name);
            ret.Add(
                Int32XmlTranslation_Test.Instance.GetTypicalElement(nameof(ObjectToRef.KeyField)));
            ret.Add(
                BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(ObjectToRef.SomeField)));
            return ret;
        }

        public override void Parse_BadElement_Mask()
        {
        }

        public override void Parse_BadElement_NoMask()
        {
        }

        public override void Write_NodeName()
        {
            var name = "AName";
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var def = new ObjectToRef();
            def.Write_XML(
                writer: writer.Writer,
                name: name);
            XElement elem = writer.Resolve();
            var nameAttr = elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE));
            Assert.NotNull(nameAttr);
            Assert.Equal(name, nameAttr.Value);
        }
    }
}
