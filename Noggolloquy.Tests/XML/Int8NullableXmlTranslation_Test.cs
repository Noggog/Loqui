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
    public class Int8NullableXmlTranslation_Test : TypicalXmlTranslation_Test<sbyte?>
    {
        public const sbyte TYPICAL_VALUE = 4;
        public const sbyte NEGATIVE_VALUE = -4;
        public const sbyte MIN_VALUE = sbyte.MinValue;
        public const sbyte MAX_VALUE = sbyte.MaxValue;

        public override string ExpectedName => "SByteN";

        public override IXmlTranslation<sbyte?> GetTranslation()
        {
            return new Int8XmlTranslation();
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
                maskObj: out object maskObj);
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
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(null, ret.Value);
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
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(null, ret.Value);
        }
        #endregion

        #region Parse - Empty Value
        [Fact]
        public void Parse_EmptyValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(null, ret.Value);
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
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(null, ret.Value);
        }
        #endregion

        #region Write - Typical
        [Fact]
        public void Write_NoMask()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var ret = transl.Write(
                writer: writer.Writer,
                name: null,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret);
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
            var ret = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret);
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
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(TYPICAL_VALUE, readResp.Value);
        }

        [Fact]
        public void Reimport_Zero()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: 0,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal<double?>(0, readResp.Value);
        }

        [Fact]
        public void Reimport_Null()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: null,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(null, readResp.Value);
        }

        [Fact]
        public void Reimport_Negative()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: NEGATIVE_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(NEGATIVE_VALUE, readResp.Value);
        }

        [Fact]
        public void Reimport_Min()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: MIN_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(MIN_VALUE, readResp.Value);
        }

        [Fact]
        public void Reimport_Max()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: MAX_VALUE,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(MAX_VALUE, readResp.Value);
        }
        #endregion
    }
}
