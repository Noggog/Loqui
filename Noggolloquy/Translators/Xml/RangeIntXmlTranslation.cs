using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class RangeIntXmlTranslation : TypicalXmlTranslation<RangeInt>
    {
        public readonly static RangeIntXmlTranslation Instance = new RangeIntXmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override bool WriteValue(XmlWriter writer, string name, RangeInt? item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!item.HasValue) return true;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
            return true;
        }

        protected override string GetItemStr(RangeInt item)
        {
            throw new NotImplementedException();
        }

        protected override RangeInt ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override TryGet<RangeInt?> ParseValue(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            maskObj = null;
            int? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (int.TryParse(val.Value, out var i))
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
                if (int.TryParse(val.Value, out var i))
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
            if (!min.HasValue && !max.HasValue) return TryGet<RangeInt?>.Succeed(null);
            return TryGet<RangeInt?>.Succeed(
                new RangeInt(min, max));
        }
    }
}
