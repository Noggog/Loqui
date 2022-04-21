using Loqui.Internal;
using Noggog;

namespace Loqui.Xml;

public class P3UInt16XmlTranslation : PrimitiveXmlTranslation<P3UInt16>
{
    public readonly static P3UInt16XmlTranslation Instance = new P3UInt16XmlTranslation();

    protected override string GetItemStr(P3UInt16 item)
    {
        return $"{item.X}, {item.Y}, {item.Z}";
    }

    protected override bool Parse(string str, out P3UInt16 value, ErrorMaskBuilder? errorMask)
    {
        if (P3UInt16.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}