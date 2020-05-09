using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeInt64XmlTranslation : PrimitiveXmlTranslation<RangeInt64>
    {
        public readonly static RangeInt64XmlTranslation Instance = new RangeInt64XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeInt64? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeInt64 item)
        {
            throw new NotImplementedException();
        }

        protected override bool Parse(string str, out RangeInt64 value, ErrorMaskBuilder? errorMask)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(XElement root, out RangeInt64 value, ErrorMaskBuilder? errorMask)
        {
            long? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute? val))
            {
                if (long.TryParse(val.Value, out var i))
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
                if (long.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Max value was malformed: " + val.Value));
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
            value = new RangeInt64(min, max);
            return true;
        }
    }
}
