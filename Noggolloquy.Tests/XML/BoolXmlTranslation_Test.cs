using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Noggolloquy.Tests.XML
{
    public class BoolXmlTranslation_Test : TypicalXmlTranslation_Test<bool>
    {
        public static readonly bool TYPICAL_VALUE = true;

        public override string ExpectedName => "Boolean";

        public override IXmlTranslation<bool> GetTranslation()
        {
            return new BooleanXmlTranslation();
        }

        #region Parse - Typical
        [Fact]
        public void Parse_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(true);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(true, ret.Value);
        }

        [Fact]
        public void Parse_Mask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(true);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(true, ret.Value);
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
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: null,
                item: true,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
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
                item: true,
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
        public void Reimport_True()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: true,
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(true, readResp.Value);
        }

        [Fact]
        public void Reimport_False()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: false,
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(false, readResp.Value);
        }
        #endregion
    }
}
