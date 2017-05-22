﻿using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Loqui.Tests.Internals;
using Xunit;

namespace Loqui.Tests.XML
{
    public class RefListXmlTranslation_Tests
    {
        public string ExpectedName => "List";

        public static ObjectToRef Item1 = new ObjectToRef()
        {
            KeyField = 5,
            SomeField = true
        };
        public static ObjectToRef Item2 = new ObjectToRef()
        {
            KeyField = 2,
            SomeField = false
        };
        public static ObjectToRef[] TypicalContents = new ObjectToRef[]
        {
            Item1,
            Item2
        };
        public IEnumerable<ObjectToRef> GetTypicalContents() => TypicalContents;

        public ListXmlTranslation<ObjectToRef> GetTranslation()
        {
            return new ListXmlTranslation<ObjectToRef>();
        }

        public virtual XElement GetTypicalElement(string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(ExpectedName, name);
            foreach (var item in GetTypicalContents())
            {
                var itemElem = new XElement("ObjectToRef");
                itemElem.SetAttributeValue("name", "RefList");
                var keyField = new XElement("Int32");
                keyField.SetAttributeValue("name", "KeyField");
                keyField.SetAttributeValue("value", item.KeyField);
                itemElem.Add(keyField);
                var boolElem = new XElement("Boolean");
                boolElem.SetAttributeValue("name", "SomeField");
                boolElem.SetAttributeValue("value", item.SomeField);
                itemElem.Add(boolElem);
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
                transl: (ObjectToRef item, out object suberrorMask) =>
                {
                    ObjectToRefCommon.Write_XML(
                        writer: writer.Writer,
                        item: item,
                        name: null,
                        doMasks: false,
                        errorMask: out ObjectToRef_ErrorMask subsuberrorMask);
                    suberrorMask = subsuberrorMask;
                },
                writer: writer.Writer,
                item: Item1,
                doMasks: false,
                maskObj: out object maskObj);
            var readResp = transl.ParseSingleItem(
                writer.Resolve(),
                LoquiXmlTranslation<ObjectToRef, ObjectToRef_ErrorMask>.Instance,
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(Item1, readResp.Value);
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
            Assert.Equal(GetTypicalContents().Count(), elem.Descendants().Where((d) => object.ReferenceEquals(d.Parent, elem)).Count());
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
            Assert.Equal(GetTypicalContents().Count(), elem.Descendants().Where((d) => object.ReferenceEquals(d.Parent, elem)).Count());
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
                item: new ObjectToRef[] { },
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