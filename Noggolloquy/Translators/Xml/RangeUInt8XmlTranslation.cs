using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class RangeUInt8XmlTranslation : TypicalXmlTranslation<RangeUInt8>
    {
        public readonly static RangeUInt8XmlTranslation Instance = new RangeUInt8XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override bool WriteValue(XmlWriter writer, string name, RangeUInt8? item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!item.HasValue) return true;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
            return true;
        }

        protected override string GetItemStr(RangeUInt8 item)
        {
            throw new NotImplementedException();
        }

        protected override RangeUInt8 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override TryGet<RangeUInt8?> ParseValue(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            maskObj = null;
            byte? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (byte.TryParse(val.Value, out var i))
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
                if (byte.TryParse(val.Value, out var i))
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
            if (!min.HasValue && !max.HasValue) return TryGet<RangeUInt8?>.Succeed(null);
            return TryGet<RangeUInt8?>.Succeed(
                new RangeUInt8(min, max));
        }
    }
}
