using Loqui.Tests.Internals;
using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests.XML
{
    public class WildcardLoquiXmlTranslation_Test
    {
        public static readonly WildcardLoquiXmlTranslation_Test Instance = new WildcardLoquiXmlTranslation_Test();
        public static readonly ObjectToRef TYPICAL_VALUE = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE;
        public ObjectToRefXmlTranslation_Test subTest = new ObjectToRefXmlTranslation_Test();

        public IXmlTranslation<Object> GetTranslation()
        {
            return new WildcardXmlTranslation();
        }

        public ObjectToRef GetDefault()
        {
            return null;
        }

        public XElement GetTypicalElement(string name = null)
        {
            return subTest.GetTypicalElement(name);
        }

        public XElement GetElementNoValue()
        {
            return subTest.GetElementNoValue();
        }

        public string GetTypicalString()
        {
            return subTest.StringConverter(ObjectToRefXmlTranslation_Test.TYPICAL_VALUE);
        }

        [Fact]
        public void ElementName()
        {
            var transl = GetTranslation();
            Assert.Null(transl.ElementName);
        }

        [Fact]
        public void Write_NodeName()
        {
            var name = "AName";
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: name,
                item: GetDefault(),
                doMasks: false,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            var nameAttr = elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE));
            Assert.NotNull(nameAttr);
            Assert.Equal(name, nameAttr.Value);
        }

        #region Parse - Typical
        [Fact]
        public void Parse_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement();
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(TYPICAL_VALUE, ret.Value);
        }

        [Fact]
        public void Parse_Mask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement();
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

        #region Write - Typical
        [Fact]
        public void Write_NoMask()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: null,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
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
