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

        protected override RangeInt64 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt64? ParseValue(XElement root)
        {
            long? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (long.TryParse(val.Value, out var i))
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
                if (long.TryParse(val.Value, out var i))
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
            return new RangeInt64(min, max);
        }
    }
}
