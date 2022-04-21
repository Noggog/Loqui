using Loqui.Internal;
using Noggog;
using System.Xml.Linq;

namespace Loqui.Xml;

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

    protected override bool Parse(string str, out RangeUInt16 value, ErrorMaskBuilder? errorMask)
    {
        throw new NotImplementedException();
    }

    public override bool Parse(XElement root, out RangeUInt16 value, ErrorMaskBuilder? errorMask)
    {
        ushort? min, max;
        if (root.TryGetAttribute(MIN, out XAttribute? val))
        {
            if (ushort.TryParse(val.Value, out var i))
            {
                min = i;
            }
            else
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException("Min value was malformed: " + val.Value));
                value = default;
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
                value = default;
                return false;
            }
        }
        else
        {
            max = null;
        }
        if (!min.HasValue && !max.HasValue)
        {
            value = default;
            return false;
        }
        value = new RangeUInt16(min, max);
        return true;
    }
}