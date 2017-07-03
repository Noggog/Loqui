using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
namespace Loqui.Tests.XML
{
    public class WildcardBoolXmlTranslation_Test
    {
        public static readonly WildcardBoolXmlTranslation_Test Instance = new WildcardBoolXmlTranslation_Test();
        public const bool TYPICAL_VALUE = true;
        public BoolXmlTranslation_Test subTest = new BoolXmlTranslation_Test();

        public IXmlTranslation<Object, Object> GetTranslation()
        {
            return new WildcardXmlTranslation();
        }

        public bool GetDefault()
        {
            return true;
        }

        public XElement GetTypicalElement(string name = null)
        {
            var wildElem = XmlUtility.GetElementNoValue(name);
            wildElem.SetAttributeValue(XName.Get(XmlConstants.TYPE_ATTRIBUTE), subTest.ExpectedName);
            var elem = subTest.GetTypicalElement(BoolXmlTranslation_Test.TYPICAL_VALUE, "Item");
            wildElem.Add(elem);
            return wildElem;
        }

        public XElement GetElementNoValue()
        {
            return subTest.GetElementNoValue();
        }

        public string GetTypicalString()
        {
            return subTest.StringConverter(BoolXmlTranslation_Test.TYPICAL_VALUE);
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
                maskObj: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(name, elem.Name.LocalName);
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
                maskObj: out var maskObj);
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
            Assert.Throws(
                typeof(ArgumentException),
                () => transl.Parse(
                    elem,
                    doMasks: false,
                    maskObj: out var maskObj));
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
                    maskObj: out var maskObj));
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
                name: XmlUtility.TYPICAL_NAME,
                item: TYPICAL_VALUE,
                doMasks: false,
                maskObj: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Name.LocalName);
            XElement item = elem.Element("Item");
            var valAttr = item.Attribute(XName.Get(XmlConstants.VALUE_ATTRIBUTE));
            Assert.NotNull(valAttr);
            Assert.Equal(GetTypicalString(), valAttr.Value);
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
            XElement item = elem.Element("Item");
            var valAttr = item.Attribute(XName.Get(XmlConstants.VALUE_ATTRIBUTE));
            Assert.NotNull(valAttr);
            Assert.Equal(GetTypicalString(), valAttr.Value);
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
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
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
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(false, readResp.Value);
        }
        #endregion
    }
}
#pragma warning restore xUnit2004 // Do not use equality check to test for boolean conditions
