using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class RangeDoubleXmlTranslation : TypicalXmlTranslation<RangeDouble>
    {
        public readonly static RangeDoubleXmlTranslation Instance = new RangeDoubleXmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override bool WriteValue(XmlWriter writer, string name, RangeDouble? item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!item.HasValue) return true;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
            return true;
        }

        protected override string GetItemStr(RangeDouble item)
        {
            throw new NotImplementedException();
        }

        protected override RangeDouble ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override TryGet<RangeDouble?> ParseValue(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            maskObj = null;
            double? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (double.TryParse(val.Value, out var d))
                {
                    min = d;
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
                if (double.TryParse(val.Value, out var d))
                {
                    max = d;
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
            if (!min.HasValue && !max.HasValue) return TryGet<RangeDouble?>.Succeed(null);
            return TryGet<RangeDouble?>.Succeed(
                new RangeDouble(min, max));
        }
    }
}
