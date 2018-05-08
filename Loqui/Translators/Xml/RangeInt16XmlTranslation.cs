using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeInt16XmlTranslation : PrimitiveXmlTranslation<RangeInt16>
    {
        public readonly static RangeInt16XmlTranslation Instance = new RangeInt16XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XElement node, RangeInt16? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(MIN, item.Value.Min.ToString());
            node.SetAttributeValue(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeInt16 item)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt16 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt16? ParseValue(XElement root)
        {
            short? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (short.TryParse(val.Value, out var i))
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
                if (short.TryParse(val.Value, out var i))
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
            return new RangeInt16(min, max);
        }
    }
}
