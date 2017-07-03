using Noggog;
using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests.XML
{
    public class RangeInt64NullableXmlTranslation_Test : TypicalXmlTranslation_Test<RangeInt64?, Exception, RangeInt64NullableXmlTranslation_Test>
    {
        public static readonly RangeInt64 TYPICAL_VALUE = new RangeInt64(5, 3_147_483_647);
        public override RangeInt64? TypicalValue => TYPICAL_VALUE;
        public static readonly RangeInt64 ZERO_VALUE = new RangeInt64(0, 0);
        public static readonly RangeInt64 NEGATIVE_VALUE = new RangeInt64(-67, -6);
        public const string MIN = "Min";
        public const string MAX = "Max";

        public override string ExpectedName => "RangeInt64N";

        public override IXmlTranslation<RangeInt64?, Exception> GetTranslation()
        {
            return new RangeInt64XmlTranslation();
        }

        public override XElement GetTypicalElement(RangeInt64? value, string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(name);
            if (value == null) return elem;
            elem.SetAttributeValue(XName.Get(MIN), value.Value.Min);
            elem.SetAttributeValue(XName.Get(MAX), value.Value.Max);
            return elem;
        }

        public override XElement GetBadElement(string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(name);
            elem.SetAttributeValue(XName.Get(MIN), "Gibberish");
            return elem;
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

        #region Parse - No Value
        [Fact]
        public void Parse_NoValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Null(ret.Value);
        }

        [Fact]
        public void Parse_NoValue_Mask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Null(ret.Value);
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
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Null(ret.Value);
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
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Null(ret.Value);
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
                name: "Item",
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
            var minAttr = elem.Attribute(XName.Get(MIN));
            Assert.NotNull(minAttr);
            Assert.Equal(TYPICAL_VALUE.Min.ToString(), minAttr.Value);
            var maxAttr = elem.Attribute(XName.Get(MAX));
            Assert.NotNull(maxAttr);
            Assert.Equal(TYPICAL_VALUE.Max.ToString(), maxAttr.Value);
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
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Name.LocalName);
            var minAttr = elem.Attribute(XName.Get(MIN));
            Assert.NotNull(minAttr);
            Assert.Equal(TYPICAL_VALUE.Min.ToString(), minAttr.Value);
            var maxAttr = elem.Attribute(XName.Get(MAX));
            Assert.NotNull(maxAttr);
            Assert.Equal(TYPICAL_VALUE.Max.ToString(), maxAttr.Value);
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
        public void Reimport_Zero()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: ZERO_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(ZERO_VALUE, readResp.Value);
        }

        [Fact]
        public void Reimport_Null()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: null,
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Null(readResp.Value);
        }

        [Fact]
        public void Reimport_Negative()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: NEGATIVE_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(NEGATIVE_VALUE, readResp.Value);
        }
        #endregion
    }
}
