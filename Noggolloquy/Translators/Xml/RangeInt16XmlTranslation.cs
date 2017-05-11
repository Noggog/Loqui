using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class RangeInt16XmlTranslation : PrimitiveXmlTranslation<RangeInt16>
    {
        public readonly static RangeInt16XmlTranslation Instance = new RangeInt16XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override bool WriteValue(XmlWriter writer, string name, RangeInt16? item)
        {
            if (!item.HasValue) return true;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
            return true;
        }

        protected override string GetItemStr(RangeInt16 item)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt16 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override TryGet<RangeInt16?> ParseValue(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            maskObj = null;
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
            if (!min.HasValue && !max.HasValue) return TryGet<RangeInt16?>.Succeed(null);
            return TryGet<RangeInt16?>.Succeed(
                new RangeInt16(min, max));
        }
    }
}
