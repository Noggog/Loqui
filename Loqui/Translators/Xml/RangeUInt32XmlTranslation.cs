using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeUInt32XmlTranslation : PrimitiveXmlTranslation<RangeUInt32>
    {
        public readonly static RangeUInt32XmlTranslation Instance = new RangeUInt32XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeUInt32? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeUInt32 item)
        {
            throw new NotImplementedException();
        }

        protected override bool Parse(string str, out RangeUInt32 value, ErrorMaskBuilder? errorMask)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(XElement root, out RangeUInt32 value, ErrorMaskBuilder? errorMask)
        {
            uint? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute? val))
            {
                if (uint.TryParse(val.Value, out var i))
                {
                    min = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Min value was malformed: " + val.Value));
                    value = default;
                    return false;
                }
            }
            else
            {
                min = null;
            }
            if (root.TryGetAttribute(MAX, out val))
            {
                if (uint.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Min value was malformed: " + val.Value));
                    value = default;
                    return false;
                }
            }
            else
            {
                max = null;
            }
            if (!min.HasValue && !max.HasValue)
            {
                value = default;
                return false;
            }
            value = new RangeUInt32(min, max);
            return true;
        }
    }
}
