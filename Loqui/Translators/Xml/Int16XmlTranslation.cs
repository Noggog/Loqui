using Loqui.Internal;

namespace Loqui.Xml;

public class Int16XmlTranslation : PrimitiveXmlTranslation<short>
{
    public readonly static Int16XmlTranslation Instance = new Int16XmlTranslation();

    protected override bool Parse(string str, out short value, ErrorMaskBuilder? errorMask)
    {
        if (short.TryParse(str, out value))
        {
            return true;
        }
        errorMask.ReportExceptionOrThrow(
            new ArgumentException($"Could not convert to {ElementName}"));
        return false;
    }
}