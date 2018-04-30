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

        protected override RangeUInt16 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override RangeUInt16? ParseValue(XElement root)
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
                    throw new ArgumentException("Min value was malformed: " + val.Value);
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
                    throw new ArgumentException("Min value was malformed: " + val.Value);
                }
            }
            else
            {
                max = null;
            }
            if (!min.HasValue && !max.HasValue) return null;
            return new RangeUInt16(min, max);
        }
    }
}
