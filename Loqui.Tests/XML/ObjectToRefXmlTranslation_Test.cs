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
    public class ObjectToRefXmlTranslation_Test : TypicalXmlTranslation_Test<ObjectToRef, ObjectToRef_ErrorMask, ObjectToRefXmlTranslation_Test>
    {
        public static readonly ObjectToRef TYPICAL_VALUE = ObjectToRef.TYPICAL_VALUE;
        public override string ExpectedName => "Loqui.Tests.ObjectToRef";

        public override ObjectToRef TypicalValue => TYPICAL_VALUE;

        public override IXmlTranslation<ObjectToRef, ObjectToRef_ErrorMask> GetTranslation()
        {
            return new LoquiXmlTranslation<ObjectToRef, ObjectToRef_ErrorMask>();
        }

        public override XElement GetTypicalElement(ObjectToRef obj, string name = null)
        {
            var ret = XmlUtility.GetElementNoValue(name);
            ret.SetAttributeValue("type", "Loqui.Tests.ObjectToRef");
            ret.Add(
                Int32XmlTranslation_Test.Instance.GetTypicalElement(obj?.KeyField ?? Int32XmlTranslation_Test.TYPICAL_VALUE, nameof(ObjectToRef.KeyField)));
            ret.Add(
                BoolXmlTranslation_Test.Instance.GetTypicalElement(obj?.SomeField ?? BoolXmlTranslation_Test.TYPICAL_VALUE, nameof(ObjectToRef.SomeField)));
            return ret;
        }

        public override XElement GetTypicalElement(string name = null)
        {
            return GetTypicalElement(obj: null, name: name);
        }

        [Fact]
        public override void Parse_BadElement_Mask()
        {
        }

        [Fact]
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
            Assert.Equal(name, elem.Name.LocalName);
        }
    }
}
