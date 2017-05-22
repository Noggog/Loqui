﻿using Loqui.Xml;
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
    public class LoquiXmlTranslation_Direct_Test
    {
        public static readonly TestObject_HasBeenSet TYPICAL_VALUE;
        public static readonly LoquiXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask> Translator = new LoquiXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask>();

        static LoquiXmlTranslation_Direct_Test()
        {
            TYPICAL_VALUE = new TestObject_HasBeenSet()
            {
                Bool = BoolXmlTranslation_Test.TYPICAL_VALUE,
                BoolN = BoolNullableXmlTranslation_Test.TYPICAL_VALUE,
                Char = CharXmlTranslation_Test.TYPICAL_VALUE,
                CharN = CharNullableXmlTranslation_Test.TYPICAL_VALUE,
                DateTime = DateTimeXmlTranslation_Test.TYPICAL_VALUE,
                DateTimeNull = DateTimeNullableXmlTranslation_Test.TYPICAL_VALUE,
                Double = DoubleXmlTranslation_Test.TYPICAL_VALUE,
                DoubleN = DoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
                Enum = EnumXmlTranslation_Tests.TYPICAL_VALUE,
                DoubleN_Ranged = DoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
                Double_Ranged = DoubleXmlTranslation_Test.TYPICAL_VALUE,
                Float = FloatXmlTranslation_Test.TYPICAL_VALUE,
                FloatN = FloatNullableXmlTranslation_Test.TYPICAL_VALUE,
                FloatN_Ranged = FloatNullableXmlTranslation_Test.TYPICAL_VALUE,
                Float_Ranged = FloatXmlTranslation_Test.TYPICAL_VALUE,
                Int16 = Int16XmlTranslation_Test.TYPICAL_VALUE,
                Int16N = Int16NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int16N_Ranged = Int16NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int16_Ranged = Int16XmlTranslation_Test.TYPICAL_VALUE,
                Int32 = Int32XmlTranslation_Test.TYPICAL_VALUE,
                Int32N = Int32NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int32N_Ranged = Int32NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int32_Ranged = Int32XmlTranslation_Test.TYPICAL_VALUE,
                Int64 = Int64XmlTranslation_Test.TYPICAL_VALUE,
                Int64N = Int64NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int64N_Ranged = Int64NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int64_Ranged = Int64XmlTranslation_Test.TYPICAL_VALUE,
                Int8 = Int8XmlTranslation_Test.TYPICAL_VALUE,
                Int8N = Int8NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int8N_Ranged = Int8NullableXmlTranslation_Test.TYPICAL_VALUE,
                Int8_Ranged = Int8XmlTranslation_Test.TYPICAL_VALUE,
                P2Int = P2IntXmlTranslation_Test.TYPICAL_VALUE,
                P2IntN = P2IntNullableXmlTranslation_Test.TYPICAL_VALUE,
                P3Int = P3IntXmlTranslation_Test.TYPICAL_VALUE,
                P3IntN = P3IntNullableXmlTranslation_Test.TYPICAL_VALUE,
                Percent = PercentXmlTranslation_Test.TYPICAL_VALUE,
                PercentN = PercentNullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeInt16 = RangeInt16XmlTranslation_Test.TYPICAL_VALUE,
                RangeInt16N = RangeInt16NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeInt32 = RangeInt32XmlTranslation_Test.TYPICAL_VALUE,
                RangeInt32N = RangeInt32NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeInt64 = RangeInt64XmlTranslation_Test.TYPICAL_VALUE,
                RangeInt64N = RangeInt64NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeInt8 = RangeInt8XmlTranslation_Test.TYPICAL_VALUE,
                RangeInt8N = RangeInt8NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt16 = RangeUInt16XmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt16N = RangeUInt16NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt32 = RangeUInt32XmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt32N = RangeUInt32NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt64 = RangeUInt64XmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt64N = RangeUInt64NullableXmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt8 = RangeUInt8XmlTranslation_Test.TYPICAL_VALUE,
                RangeUInt8N = RangeUInt8NullableXmlTranslation_Test.TYPICAL_VALUE,
                Ref = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                RefGetter = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                RefGetter_NotNull = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                RefSetter_NotNull = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                RefSetter = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                Ref_NotNull = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                String = StringXmlTranslation_Test.TYPICAL_VALUE,
                UDouble = UDoubleXmlTranslation_Test.TYPICAL_VALUE,
                UDoubleN = UDoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
                UDoubleN_Ranged = UDoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
                UDouble_Ranged = UDoubleXmlTranslation_Test.TYPICAL_VALUE,
                UInt16 = UInt16XmlTranslation_Test.TYPICAL_VALUE,
                UInt16N = UInt16NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt16N_Ranged = UInt16NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt16_Ranged = UInt16XmlTranslation_Test.TYPICAL_VALUE,
                UInt32 = UInt32XmlTranslation_Test.TYPICAL_VALUE,
                UInt32N = UInt32NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt32N_Ranged = UInt32NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt32_Ranged = UInt32XmlTranslation_Test.TYPICAL_VALUE,
                UInt64 = UInt64XmlTranslation_Test.TYPICAL_VALUE,
                UInt64N = UInt64NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt64N_Ranged = UInt64NullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt64_Ranged = UInt64XmlTranslation_Test.TYPICAL_VALUE,
                UInt8 = ByteXmlTranslation_Test.TYPICAL_VALUE,
                UInt8N = ByteNullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt8N_Ranged = ByteNullableXmlTranslation_Test.TYPICAL_VALUE,
                UInt8_Ranged = ByteXmlTranslation_Test.TYPICAL_VALUE,
                Unsafe = true,
                WildCard = true
            };

        }

        public string ExpectedName => "Loqui.Tests.TestObject";

        public XElement GetTypicalElement(TestObject_HasBeenSet value, string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(ExpectedName, name);
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Bool)));
            elem.Add(BoolNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.BoolN)));
            elem.Add(CharXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Char)));
            elem.Add(CharNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.CharN)));
            elem.Add(DateTimeXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DateTime)));
            elem.Add(DateTimeNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DateTimeNull)));
            elem.Add(DoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Double)));
            elem.Add(DoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DoubleN)));
            elem.Add(EnumXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Enum)));
            elem.Add(DoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DoubleN)));
            elem.Add(DoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Double)));
            elem.Add(FloatXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Float)));
            elem.Add(FloatNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.FloatN)));
            elem.Add(FloatXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Float_Ranged)));
            elem.Add(FloatNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.FloatN_Ranged)));
            elem.Add(Int16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int16)));
            elem.Add(Int16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int16N)));
            elem.Add(Int16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int16_Ranged)));
            elem.Add(Int16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int16N_Ranged)));
            elem.Add(Int32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int32)));
            elem.Add(Int32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int32N)));
            elem.Add(Int32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int32_Ranged)));
            elem.Add(Int32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int32N_Ranged)));
            elem.Add(Int64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int64)));
            elem.Add(Int64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int64N)));
            elem.Add(Int64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int64_Ranged)));
            elem.Add(Int64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int64N_Ranged)));
            elem.Add(Int8XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int8)));
            elem.Add(Int8NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int8N)));
            elem.Add(Int8XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int8_Ranged)));
            elem.Add(Int8NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Int8N_Ranged)));
            elem.Add(P2IntXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P2Int)));
            elem.Add(P2IntNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P2IntN)));
            elem.Add(P3IntXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P3Int)));
            elem.Add(P2IntNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P3IntN)));
            elem.Add(PercentXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Percent)));
            elem.Add(PercentNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.PercentN)));
            elem.Add(RangeInt16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt16)));
            elem.Add(RangeInt16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt16N)));
            elem.Add(RangeInt32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt32)));
            elem.Add(RangeInt32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt32N)));
            elem.Add(RangeInt64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt64)));
            elem.Add(RangeInt64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt64N)));
            elem.Add(RangeInt8XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt8)));
            elem.Add(RangeInt8NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeInt8N)));
            elem.Add(RangeUInt16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt16)));
            elem.Add(RangeUInt16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt16N)));
            elem.Add(RangeUInt32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt32)));
            elem.Add(RangeUInt32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt32N)));
            elem.Add(RangeUInt64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt64)));
            elem.Add(RangeUInt64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt64N)));
            elem.Add(RangeUInt8XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt8)));
            elem.Add(RangeUInt8NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt8N)));
            elem.Add(RangeUInt8NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RangeUInt8N)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Ref)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefGetter)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefGetter_NotNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefSetter_NotNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefSetter)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Ref_NotNull)));
            elem.Add(StringXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.String)));
            elem.Add(UDoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UDouble)));
            elem.Add(UDoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UDoubleN)));
            elem.Add(UDoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UDouble_Ranged)));
            elem.Add(UDoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UDoubleN_Ranged)));
            elem.Add(UInt16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt16)));
            elem.Add(UInt16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt16N)));
            elem.Add(UInt16XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt16_Ranged)));
            elem.Add(UInt16NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt16N_Ranged)));
            elem.Add(UInt32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt32)));
            elem.Add(UInt32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt32N)));
            elem.Add(UInt32XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt32_Ranged)));
            elem.Add(UInt32NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt32N_Ranged)));
            elem.Add(UInt64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt64)));
            elem.Add(UInt64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt64N)));
            elem.Add(UInt64XmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt64_Ranged)));
            elem.Add(UInt64NullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt64N_Ranged)));
            elem.Add(ByteXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt8)));
            elem.Add(ByteNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt8N)));
            elem.Add(ByteXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt8_Ranged)));
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Unsafe)));
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.WildCard)));
            return elem;
        }

        public XElement GetElementNoValue()
        {
            return XmlUtility.GetElementNoValue(this.ExpectedName);
        }

        #region Parse - Typical
        [Fact]
        public void Parse_NoMask()
        {
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = TestObject_HasBeenSet.Create_XML(
                elem);
            Assert.Equal(TYPICAL_VALUE, ret);
        }

        [Fact]
        public void Parse_Mask()
        {
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = TestObject_HasBeenSet.Create_XML(
                elem,
                errorMask: out var maskObj);
            Assert.Null(maskObj);
            Assert.Equal(TYPICAL_VALUE, ret);
        }
        #endregion

        #region Parse - Bad Element Name
        [Fact]
        public void Parse_BadElementName_Mask()
        {
            var elem = XmlUtility.GetBadlyNamedElement();
            var ret = TestObject_HasBeenSet.Create_XML(
                elem,
                errorMask: out var maskObj);
            Assert.Null(ret);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }

        [Fact]
        public void Parse_BadElementName_NoMask()
        {
            var elem = XmlUtility.GetBadlyNamedElement();
            Assert.Throws(
                typeof(ArgumentException),
                () => TestObject_HasBeenSet.Create_XML(
                    elem));
        }
        #endregion

        #region Parse - No Value
        [Fact]
        public void Parse_NoValue_NoMask()
        {
            var elem = GetElementNoValue();
            Assert.Throws(
                typeof(ArgumentException),
                () => TestObject_HasBeenSet.Create_XML(
                    elem));
        }

        [Fact]
        public void Parse_NoValue_Mask()
        {
            var elem = GetElementNoValue();
            var ret = TestObject_HasBeenSet.Create_XML(
                elem,
                errorMask: out var maskObj);
            Assert.Equal(TYPICAL_VALUE, ret);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }
        #endregion

        #region Parse - Empty Value
        [Fact]
        public void Parse_EmptyValue_NoMask()
        {
            var elem = GetElementNoValue();
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            Assert.Throws(
                typeof(ArgumentException),
                () => TestObject_HasBeenSet.Create_XML(
                    elem));
        }

        [Fact]
        public void Parse_EmptyValue_Mask()
        {
            var elem = GetElementNoValue();
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), string.Empty);
            var ret = TestObject_HasBeenSet.Create_XML(
                elem,
                errorMask: out var maskObj);
            Assert.Null(ret);
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
            Assert.Equal(9, elem.Elements().Count());
        }

        [Fact]
        public void Write_Mask()
        {
            var writer = XmlUtility.GetWriteBundle();
            TYPICAL_VALUE.Write_XML(
                writer.Writer,
                name: XmlUtility.TYPICAL_NAME,
                errorMask: out var maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)).Value);
            Assert.Equal(9, elem.Elements().Count());
        }
        #endregion

        #region Reimport
        [Fact]
        public void Reimport_Typical()
        {
            var writer = XmlUtility.GetWriteBundle();
            TYPICAL_VALUE.Write_XML(
                writer.Writer,
                name: null);
            var readResp = TestObject_HasBeenSet.Create_XML(
                writer.Resolve());
            Assert.Equal(TYPICAL_VALUE, readResp);
        }
        #endregion
    }
}