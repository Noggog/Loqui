using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loqui.Xml;
using Xunit;
using System.Xml.Linq;

namespace Loqui.Tests.XML
{
    public class StringXmlTranslation_Test : TypicalXmlTranslation_Test<string, Exception, StringXmlTranslation_Test>
    {
        public const string TYPICAL_VALUE = "Noggog";
        public override string TypicalValue => TYPICAL_VALUE;
        public const string EMPTY_VALUE = "";

        public override string ExpectedName => "String";

        public override IXmlTranslation<string, Exception> GetTranslation()
        {
            return new StringXmlTranslation();
        }

        #region Parse - Typical
        [Fact]
        public void Parse_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(TYPICAL_VALUE, ret.Value);
        }

        [Fact]
        public void Parse_Mask()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(TYPICAL_VALUE, ret.Value);
        }
        #endregion

        #region Parse - Bad Element
        public override void Parse_BadElement_Mask()
        {
            // N/A
            var transl = GetTranslation();
            var elem = GetBadElement();
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal("Gibberish", ret.Value);
        }

        public override void Parse_BadElement_NoMask()
        {
            // N/A
            var transl = GetTranslation();
            var elem = GetBadElement();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal("Gibberish", ret.Value);
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
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
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
                item: TYPICAL_VALUE,
                doMasks: true,
                maskObj: out var maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Name.LocalName);
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
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(TYPICAL_VALUE, readResp.Value);
        }

        [Fact]
        public void Reimport_EmptyString()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: EMPTY_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(EMPTY_VALUE, readResp.Value);
        }
        #endregion
    }
}
