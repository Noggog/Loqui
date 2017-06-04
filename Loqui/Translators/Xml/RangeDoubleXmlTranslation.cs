using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeDoubleXmlTranslation : PrimitiveXmlTranslation<RangeDouble>
    {
        public readonly static RangeDoubleXmlTranslation Instance = new RangeDoubleXmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override bool WriteValue(XmlWriter writer, string name, RangeDouble? item)
        {
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

        protected override RangeDouble? ParseValue(XElement root)
        {
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
            if (!min.HasValue && !max.HasValue) return null;
            return new RangeDouble(min, max);
        }
    }
}
