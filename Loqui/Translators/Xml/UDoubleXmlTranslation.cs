using Loqui.Internal;
using Noggog;

namespace Loqui.Xml;

public class UDoubleXmlTranslation : PrimitiveXmlTranslation<UDouble>
{
    public readonly static UDoubleXmlTranslation Instance = new UDoubleXmlTranslation();

    protected override string GetItemStr(UDouble item)
    {
        return item.Value.ToString("R");
    }

    protected override bool Parse(string str, out UDouble value, ErrorMaskBuilder? errorMask)
    {
        if (UDouble.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}