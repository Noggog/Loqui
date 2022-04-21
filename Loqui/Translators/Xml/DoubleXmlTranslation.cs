using Loqui.Internal;

namespace Loqui.Xml;

public class DoubleXmlTranslation : PrimitiveXmlTranslation<double>
{
    public readonly static DoubleXmlTranslation Instance = new DoubleXmlTranslation();

    protected override string GetItemStr(double item)
    {
        return item.ToString("R");
    }

    protected override bool Parse(string str, out double value, ErrorMaskBuilder? errorMask)
    {
        if (double.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}