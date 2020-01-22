using Loqui.Internal;
using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeDoubleXmlTranslation : PrimitiveXmlTranslation<RangeDouble>
    {
        public readonly static RangeDoubleXmlTranslation Instance = new RangeDoubleXmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeDouble? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeDouble item)
        {
            throw new NotImplementedException();
        }

        protected override bool Parse(string str, out RangeDouble value, ErrorMaskBuilder? errorMask)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(XElement root, out RangeDouble value, ErrorMaskBuilder? errorMask)
        {
            double? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute? val))
            {
                if (double.TryParse(val.Value, out var d))
                {
                    min = d;
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
                if (double.TryParse(val.Value, out var d))
                {
                    max = d;
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
            value = new RangeDouble(min, max);
            return true;
        }
    }
}
