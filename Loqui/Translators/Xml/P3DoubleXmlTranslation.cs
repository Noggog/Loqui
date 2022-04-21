using Loqui.Internal;
using Noggog;

namespace Loqui.Xml;

public class P3DoubleXmlTranslation : PrimitiveXmlTranslation<P3Double>
{
    public static readonly P3DoubleXmlTranslation Instance = new P3DoubleXmlTranslation();

    protected override string GetItemStr(P3Double item)
    {
        return $"{item.X.ToString("R")}, {item.Y.ToString("R")}, {item.Z.ToString("R")}";
    }

    protected override bool Parse(string str, out P3Double value, ErrorMaskBuilder? errorMask)
    {
        if (P3Double.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}