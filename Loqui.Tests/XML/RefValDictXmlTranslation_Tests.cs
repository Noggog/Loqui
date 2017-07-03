using Loqui.Tests.Internals;
using Loqui.Xml;
using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests.XML
{
    public class RefValDictXmlTranslation_Tests
    {
        public static readonly RefValDictXmlTranslation_Tests Instance = new RefValDictXmlTranslation_Tests();
        public string ExpectedName => "Dict";

        public IEnumerable<KeyValuePair<string, ObjectToRef>> GetTypicalContents()
        {
            yield return new KeyValuePair<string, ObjectToRef>("Hello", ObjectToRef.TYPICAL_VALUE);
            yield return new KeyValuePair<string, ObjectToRef>("Test", ObjectToRef.TYPICAL_VALUE_2);
            yield return new KeyValuePair<string, ObjectToRef>("Test2", ObjectToRef.TYPICAL_VALUE_3);
        }

        public DictXmlTranslation<string, ObjectToRef, Exception, ObjectToRef_ErrorMask> GetTranslation()
        {
            return new DictXmlTranslation<string, ObjectToRef, Exception, ObjectToRef_ErrorMask>();
        }

        public virtual XElement GetTypicalElement(string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(name);
            foreach (var item in GetTypicalContents())
            {
                var itemElem = new XElement("Item");
                itemElem.SetAttributeValue("type", "Loqui.Tests.ObjectToRef");
                var keyElem = new XElement("Key");
                var stringElem = new XElement("String");
                stringElem.SetAttributeValue("value", item.Key);
                keyElem.Add(stringElem);
                itemElem.Add(keyElem);
                var valElem = new XElement("Value");
                valElem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(item.Value));
                itemElem.Add(valElem);
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
            var val = new KeyValuePair<string, ObjectToRef>(
                "Hello",
                ObjectToRef.TYPICAL_VALUE);
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.WriteSingleItem(
                keyTransl: (string item, bool doMasks, out Exception subErrorMask) =>
                {
                    StringXmlTranslation.Instance.Write(
                        writer.Writer,
                        name: "Item",
                        item: item,
                        doMasks: doMasks,
                        errorMask: out subErrorMask);
                },
                valTransl: (ObjectToRef item, bool doMasks, out ObjectToRef_ErrorMask subErrorMask) =>
                {
                    ObjectToRefCommon.Write_XML(item: item, writer: writer.Writer, name: "Item", doMasks: doMasks, errorMask: out subErrorMask);
                },
                writer: writer.Writer,
                item: val,
                doMasks: false,
                keymaskItem: out var keyMaskObj,
                valmaskItem: out var valMaskObj);
            var readResp = transl.ParseSingleItem(
                writer.Resolve(),
                keyTransl: (XElement root, bool doMasks, out Exception subErrorMask) =>
                {
                    return StringXmlTranslation.Instance.Parse(
                        root,
                        doMasks,
                        out subErrorMask);
                },
                valTransl: (XElement root, bool doMasks, out ObjectToRef_ErrorMask subErrorMask) =>
                {
                    return TryGet<ObjectToRef>.Succeed(
                        ObjectToRef.Create_XML(
                            root,
                            doMasks,
                            out subErrorMask));
                },
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(val, readResp.Value);
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
                maskObj: out var maskObj);
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
                maskObj: out var maskObj);
            Assert.True(ret.Succeeded);
            Assert.Null(maskObj);
            Assert.Equal(GetTypicalContents(), ret.Value);
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
                maskObj: out var maskObj);
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
                maskObj: out var maskObj);
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
                maskObj: out var maskObj);
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
                maskObj: out var maskObj);
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
                name: XmlUtility.TYPICAL_NAME,
                items: GetTypicalContents(),
                doMasks: false,
                maskObj: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
            Assert.Equal(GetTypicalContents().Count(), elem.Elements().Count());
        }

        [Fact]
        public void Write_Mask()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            transl.Write(
                writer: writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                items: GetTypicalContents(),
                doMasks: true,
                maskObj: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Name.LocalName);
            Assert.Equal(GetTypicalContents().Count(), elem.Elements().Count());
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
                items: GetTypicalContents(),
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
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
                items: new KeyValuePair<string, ObjectToRef>[] { },
                doMasks: false,
                maskObj: out var maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out var readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Empty(readResp.Value);
        }
        #endregion
    }
}
