using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeUInt8XmlTranslation : PrimitiveXmlTranslation<RangeUInt8>
    {
        public readonly static RangeUInt8XmlTranslation Instance = new RangeUInt8XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeUInt8? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeUInt8 item)
        {
            throw new NotImplementedException();
        }

        protected override bool ParseNonNullString(string str, out RangeUInt8 value, ErrorMaskBuilder errorMask)
        {
            throw new NotImplementedException();
        }

        protected override bool ParseValue(XElement root, out RangeUInt8? value, ErrorMaskBuilder errorMask)
        {
            byte? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (byte.TryParse(val.Value, out var i))
                {
                    min = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Min value was malformed: " + val.Value));
                    value = null;
                    return false;
                }
            }
            else
            {
                min = null;
            }
            if (root.TryGetAttribute(MAX, out val))
            {
                if (byte.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Min value was malformed: " + val.Value));
                    value = null;
                    return false;
                }
            }
            else
            {
                max = null;
            }
            if (!min.HasValue && !max.HasValue)
            {
                value = null;
                return true;
            }
            value = new RangeUInt8(min, max);
            return true;
        }
    }
}
