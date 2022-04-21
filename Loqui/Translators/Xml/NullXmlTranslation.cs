using System.Xml.Linq;
using Loqui.Internal;

namespace Loqui.Xml;

public class NullXmlTranslation : IXmlTranslation<object?>
{
    public string ElementName => "Null";

    public bool Parse(
        XElement root, 
        out object? item, 
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask)
    {
        item = null;
        return true;
    }

    public void Write(
        XElement node,
        string? name,
        object? item,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask)
    {
        node.Add(
            new XElement(name!));
    }
}