using System.Xml.Linq;

namespace Loqui.Xml;

public static class XmlConstants
{
    public const string NAME_ATTRIBUTE = "name";
    public const string TYPE_ATTRIBUTE = "type";
    public const string VALUE_ATTRIBUTE = "value";
}

public static class XmlConstants<T, M>
{
    delegate T CREATE_FUNC(XElement root, bool doMasks, out M errorMask);
}