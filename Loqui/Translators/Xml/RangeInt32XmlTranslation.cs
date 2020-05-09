using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeInt32XmlTranslation : PrimitiveXmlTranslation<RangeInt32>
    {
        public readonly static RangeInt32XmlTranslation Instance = new RangeInt32XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeInt32? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeInt32 item)
        {
            throw new NotImplementedException();
        }

        protected override bool Parse(string str, out RangeInt32 value, ErrorMaskBuilder? errorMask)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(XElement root, out RangeInt32 item, ErrorMaskBuilder? errorMask)
        {
            int? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute? val))
            {
                if (int.TryParse(val.Value, out var i))
                {
                    min = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Min value was malformed: " + val.Value));
                    item = default;
                    return false;
                }
            }
            else
            {
                min = null;
            }
            if (root.TryGetAttribute(MAX, out val))
            {
                if (int.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Max value was malformed: " + val.Value));
                    item = default;
                    return false;
                }
            }
            else
            {
                max = null;
            }
            if (!min.HasValue && !max.HasValue)
            {
                item = default;
                return false;
            }
            item = new RangeInt32(min, max);
            return true;
        }
    }
}
