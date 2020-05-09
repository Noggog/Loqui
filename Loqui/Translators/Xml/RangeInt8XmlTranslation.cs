using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeInt8XmlTranslation : PrimitiveXmlTranslation<RangeInt8>
    {
        public readonly static RangeInt8XmlTranslation Instance = new RangeInt8XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeInt8? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeInt8 item)
        {
            throw new NotImplementedException();
        }

        protected override bool Parse(string str, out RangeInt8 value, ErrorMaskBuilder? errorMask)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(XElement root, out RangeInt8 value, ErrorMaskBuilder? errorMask)
        {
            sbyte? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute? val))
            {
                if (sbyte.TryParse(val.Value, out var i))
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
                if (sbyte.TryParse(val.Value, out var i))
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
            value = new RangeInt8(min, max);
            return true;
        }
    }
}
