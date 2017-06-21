using Loqui.Tests.Internals;
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
    public class LoquiXmlTranslation_Test
    {
        public static readonly LoquiXmlTranslation_Test Instance = new LoquiXmlTranslation_Test();
        public static readonly TestObject_HasBeenSet TYPICAL_VALUE;
        public static readonly TestObject_HasBeenSet EMPTY_VALUE = new TestObject_HasBeenSet();
        public static readonly LoquiXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask> Translator = new LoquiXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask>();
        public static int NUM_FIELDS = 99;

        static LoquiXmlTranslation_Test()
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
                EnumNull = EnumNullableXmlTranslation_Tests.TYPICAL_VALUE,
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
                P3Double = P3DoubleXmlTranslation_Test.TYPICAL_VALUE,
                P3DoubleN = P3DoubleNullableXmlTranslation_Test.TYPICAL_VALUE,
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
                UnsafeLoqui = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                UnsafeNull = null,
                WildCard = true,
                WildCardLoqui = ObjectToRefXmlTranslation_Test.TYPICAL_VALUE,
                WildCardNull = null
            };
            TYPICAL_VALUE.Ref_Singleton.CopyFieldsFrom(ObjectToRefXmlTranslation_Test.TYPICAL_VALUE);
            TYPICAL_VALUE.RefSetter_Singleton.CopyFieldsFrom(ObjectToRefXmlTranslation_Test.TYPICAL_VALUE);
            TYPICAL_VALUE.List.Add(ListXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.RefList.Add(RefListXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.Dict.Add(DictXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.KeyRefDict.Add(RefKeyDictXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.ValRefDict.Add(RefValDictXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.RefDict.Add(RefDictXmlTranslation_Tests.Instance.GetTypicalContents());
            TYPICAL_VALUE.DictKeyedValue.Set(KeyedDictXmlTranslation_Tests.Instance.GetTypicalContents());
        }

        public IXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask> GetTranslation()
        {
            return new LoquiXmlTranslation<TestObject_HasBeenSet, TestObject_HasBeenSet_ErrorMask>();
        }

        public string ExpectedName => "Loqui.Tests.TestObject_HasBeenSet";

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
            elem.Add(DoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Double_Ranged)));
            elem.Add(DoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DoubleN_Ranged)));
            elem.Add(EnumXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Enum)));
            elem.Add(EnumNullableXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.EnumNull)));
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
            elem.Add(P3IntNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P3IntN)));
            elem.Add(P3DoubleXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P3Double)));
            elem.Add(P3DoubleNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.P3DoubleN)));
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
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Ref)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefGetter)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefGetter_NotNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefSetter_NotNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefSetter)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Ref_NotNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Ref_Singleton)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefGetter_Singleton)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefSetter_Singleton)));
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
            elem.Add(ByteNullableXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UInt8N_Ranged)));
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Unsafe)));
            elem.Add(NullXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UnsafeNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.UnsafeLoqui)));
            elem.Add(BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.WildCard)));
            elem.Add(NullXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.WildCardNull)));
            elem.Add(ObjectToRefXmlTranslation_Test.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.WildCardLoqui)));
            elem.Add(ListXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.List)));
            elem.Add(RefListXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefList)));
            elem.Add(DictXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.Dict)));
            elem.Add(RefKeyDictXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.KeyRefDict)));
            elem.Add(RefValDictXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.ValRefDict)));
            elem.Add(RefDictXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.RefDict)));
            elem.Add(KeyedDictXmlTranslation_Tests.Instance.GetTypicalElement(nameof(TestObject_HasBeenSet.DictKeyedValue)));
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
            var transl = GetTranslation();
            var elem = GetTypicalElement(TYPICAL_VALUE);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
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
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
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
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            Assert.False(ret.Succeeded);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(Loqui.Tests.Internals.TestObject_HasBeenSet_ErrorMask), maskObj);
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
                    maskObj: out TestObject_HasBeenSet_ErrorMask maskObj));
        }
        #endregion
        
        #region Parse - Empty Value
        [Fact]
        public void Parse_EmptyValue_NoMask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            Assert.True(ret.Succeeded);
            Assert.NotNull(ret.Value);
            Assert.Equal(EMPTY_VALUE, ret.Value);
        }

        [Fact]
        public void Parse_EmptyValue_Mask()
        {
            var transl = GetTranslation();
            var elem = GetElementNoValue();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            Assert.True(ret.Succeeded);
            Assert.NotNull(ret.Value);
            Assert.Equal(EMPTY_VALUE, ret.Value);
            Assert.Null(maskObj);
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
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(TestObject_HasBeenSet_Registration.FullName, elem.Name.LocalName);
            Assert.Null(elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)));
            Assert.Equal(NUM_FIELDS, elem.Elements().Count());
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
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            Assert.Equal(TestObject_HasBeenSet_Registration.FullName, elem.Name.LocalName);
            Assert.Equal(XmlUtility.TYPICAL_NAME, elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE)).Value);
            Assert.Equal(NUM_FIELDS, elem.Elements().Count());
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
                maskObj: out TestObject_HasBeenSet_ErrorMask maskObj);
            var readResp = transl.Parse(
                writer.Resolve(),
                doMasks: false,
                maskObj: out TestObject_HasBeenSet_ErrorMask readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(TYPICAL_VALUE, readResp.Value);
        }
        #endregion
    }
}
