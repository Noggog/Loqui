using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Noggolloquy.Tests.XML
{
    public class ListXmlTranslation_Tests
    {
        public string ExpectedName => "List";

        public IEnumerable<bool> GetTypicalContents()
        {
            yield return true;
            yield return false;
            yield return false;
            yield return true;
        }

        public ListXmlTranslation<bool> GetTranslation()
        {
            return new ListXmlTranslation<bool>();
        }

        public virtual XElement GetTypicalElement(string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(ExpectedName, name);
            foreach (var item in GetTypicalContents())
            {
                var itemElem = new XElement("Boolean");
                itemElem.SetAttributeValue("value", item);
                elem.Add(itemElem);
            }
            return elem;
        }

        #region Element Name
        [Fact]
        public void ElementName()
        {
            var transl = GetTranslation();
            Assert.Equal(ExpectedName, transl.ElementName);
        }
        #endregion

        #region Single Items
        [Fact]
        public void ReimportSingleItem()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.WriteSingleItem(
                transl: (bool item, out object subErrorMask) =>
                {
                    BooleanXmlTranslation.Instance.Write(
                        writer.Writer,
                        null,
                        item);
                    subErrorMask = null;
                },
                writer: writer.Writer,
                item: true,
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.ParseSingleItem(
                writer.Resolve(),
                BooleanXmlTranslation.Instance,
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(true, readResp.Value);
        }
        #endregion

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
            Assert.Equal(GetTypicalContents(), ret.Value);
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
            Assert.Equal(GetTypicalContents(), ret.Value);
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
            var elem = XmlUtility.GetElementNoValue(ExpectedName);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Empty(ret.Value);
        }

        [Fact]
        public void Parse_NoValue_Mask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetElementNoValue(ExpectedName);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Empty(ret.Value);
        }
        #endregion

        #region Parse - Empty Value
        [Fact]
        public void Parse_EmptyValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetElementNoValue(ExpectedName);
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Empty(ret.Value);
        }

        [Fact]
        public void Parse_EmptyValue_Mask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetElementNoValue(ExpectedName);
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Empty(ret.Value);
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
                item: GetTypicalContents(),
                doMasks: false,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
            Assert.Equal(GetTypicalContents().Count(), elem.Descendants().Count());
        }

        [Fact]
        public void Write_Mask()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: GetTypicalContents(),
                doMasks: true,
                maskObj: out object maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)).Value);
            Assert.Equal(GetTypicalContents().Count(), elem.Descendants().Count());
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
                item: GetTypicalContents(),
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(GetTypicalContents(), readResp.Value);
        }

        [Fact]
        public void Reimport_Empty()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                item: new bool[] { },
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Empty(readResp.Value);
        }
        #endregion
    }
}
