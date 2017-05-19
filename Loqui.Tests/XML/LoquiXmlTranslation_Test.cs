using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Loqui.Tests.Internals;
using Xunit;

namespace Loqui.Tests.XML
{
    public class LoquiXmlTranslation_Test : TypicalXmlTranslation_Test<TestObject, LoquiXmlTranslation_Test>
    {
        public static readonly TestObject TYPICAL_VALUE;
        public override TestObject TypicalValue => TYPICAL_VALUE;
        public static readonly LoquiXmlTranslation<TestObject, TestObject_ErrorMask> Translator = new LoquiXmlTranslation<TestObject, TestObject_ErrorMask>();

        static LoquiXmlTranslation_Test()
        {
            TYPICAL_VALUE = new TestObject()
            {
                Bool = BoolXmlTranslation_Test.TYPICAL_VALUE,
                BoolN = BoolNullableXmlTranslation_Test.TYPICAL_VALUE,
                Char = CharXmlTranslation_Test.TYPICAL_VALUE,
                CharN = CharNullableXmlTranslation_Test.TYPICAL_VALUE,
                DateTime = DateTimeXmlTranslation_Test.TYPICAL_VALUE,
                DateTimeNull = DateTimeNullableXmlTranslation_Test.TYPICAL_VALUE,
                Double = DoubleXmlTranslation_Test.TYPICAL_VALUE,
                DoubleN = DoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
                Enum = EnumXmlTranslation_Tests.TYPICAL_VALUE
            };
        }

        public override string ExpectedName => "Loqui.Tests.TestObject";

        public override XElement GetTypicalElement(TestObject value, string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(ExpectedName, name);
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(BoolNullableXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(CharXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(CharNullableXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(DateTimeXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(DateTimeNullableXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(DoubleXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(DoubleNullableXmlTranslation_Test.Instance.GetTypicalElement());
            elem.Add(EnumXmlTranslation_Tests.Instance.GetTypicalElement());
            return elem;
        }

        public override IXmlTranslation<TestObject> GetTranslation()
        {
            return new LoquiXmlTranslation<TestObject, TestObject_ErrorMask>();
        }

        #region Parse - Typical
        [Fact]
        public void Parse_NoMask()
        {
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = TestObject.Create_XML(
                elem);
            Assert.Equal(TYPICAL_VALUE, ret);
        }

        [Fact]
        public void Parse_Mask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(TYPICAL_VALUE, ret.Value);
        }
        #endregion

        #region Parse - Bad Element Name
        [Fact]
        public void Parse_BadElementName_Mask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetBadlyNamedElement();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Failed);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }

        [Fact]
        public void Parse_BadElementName_NoMask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetBadlyNamedElement();
            Assert.Throws(
                typeof(ArgumentException),
                () => transl.Parse(
                    elem,
                    doMasks: false,
                    maskObj: out object maskObj));
        }
        #endregion

        #region Parse - No Value
        [Fact]
        public void Parse_NoValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            Assert.Throws(
                typeof(ArgumentException),
                () => transl.Parse(
                    elem,
                    doMasks: false,
                    maskObj: out object maskObj));
        }

        [Fact]
        public void Parse_NoValue_Mask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Failed);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }
        #endregion

        #region Parse - Empty Value
        [Fact]
        public void Parse_EmptyValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            Assert.Throws(
                typeof(ArgumentException),
                () => transl.Parse(
                    elem,
                    doMasks: false,
                    maskObj: out object maskObj));
        }

        [Fact]
        public void Parse_EmptyValue_Mask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Failed);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }
        #endregion

        #region Write - Typical
        [Fact]
        public void Write_NoMask()
        {
            var writer = XmlUtility.GetWriteBundle();
            TYPICAL_VALUE.Write_XML(
                writer.Writer,
                name: null);
            XElement elem = writer.Resolve();
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
            var valAttr = elem.Attribute(XName.Get(XmlConstants.VALUE_ATTRIBUTE));
            Assert.NotNull(valAttr);
            Assert.Equal(StringConverter(TYPICAL_VALUE), valAttr.Value);
        }

        [Fact]
        public void Write_Mask()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)).Value);
            var valAttr = elem.Attribute(XName.Get(XmlConstants.VALUE_ATTRIBUTE));
            Assert.NotNull(valAttr);
            Assert.Equal(StringConverter(TYPICAL_VALUE), valAttr.Value);
        }
        #endregion

        #region Reimport
        [Fact]
        public void Reimport_Typical()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(TYPICAL_VALUE, readResp.Value);
        }
        #endregion
    }
}
