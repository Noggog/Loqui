using Loqui.Internal;
using Noggog;

namespace Loqui.Xml;

public class P2Int16XmlTranslation : PrimitiveXmlTranslation<P2Int16>
{
    public readonly static P2Int16XmlTranslation Instance = new P2Int16XmlTranslation();

    protected override string GetItemStr(P2Int16 item)
    {
        return $"{item.X}, {item.Y}";
    }

    protected override bool Parse(string str, out P2Int16 value, ErrorMaskBuilder? errorMask)
    {
        if (P2Int16.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}