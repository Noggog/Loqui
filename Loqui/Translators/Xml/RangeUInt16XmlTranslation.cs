using Loqui.Internal;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeUInt16XmlTranslation : PrimitiveXmlTranslation<RangeUInt16>
    {
        public readonly static RangeUInt16XmlTranslation Instance = new RangeUInt16XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeUInt16? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeUInt16 item)
        {
            throw new NotImplementedException();
        }

        protected override bool ParseNonNullString(string str, out RangeUInt16 value, ErrorMaskBuilder errorMask)
        {
            throw new NotImplementedException();
        }

        protected override bool ParseValue(XElement root, out RangeUInt16? value, ErrorMaskBuilder errorMask)
        {
            ushort? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (ushort.TryParse(val.Value, out var i))
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
                if (ushort.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException("Max value was malformed: " + val.Value));
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
            value = new RangeUInt16(min, max);
            return true;
        }
    }
}
