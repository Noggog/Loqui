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

        protected override bool WriteValue(XmlWriter writer, string name, RangeInt8? item)
        {
            if (!item.HasValue) return true;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
            return true;
        }

        protected override string GetItemStr(RangeInt8 item)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt8 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt8? ParseValue(XElement root)
        {
            sbyte? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (sbyte.TryParse(val.Value, out var i))
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
                if (sbyte.TryParse(val.Value, out var i))
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
            return new RangeInt8(min, max);
        }
    }
}
