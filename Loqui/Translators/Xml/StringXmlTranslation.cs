using Loqui.Internal;
using Noggog;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Loqui.Xml;

public class StringXmlTranslation : IXmlTranslation<string>
{
    public string ElementName => "String";

    public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

    public bool Parse(
        XElement node,
        [MaybeNullWhen(false)] out string item,
        ErrorMaskBuilder? errorMask)
    {
        if (node.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute? val))
        {
            item = val.Value;
            return true;
        }
        item = null;
        return false;
    }

    public string Parse(
        XElement node,
        ErrorMaskBuilder? errorMask)
    {
        if (node.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute? val))
        {
            return val.Value;
        }
        return string.Empty;
    }

    public bool Parse(
        XElement node,
        [MaybeNullWhen(false)] out string item,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask)
    {
        return Parse(
            node: node,
            item: out item,
            errorMask: errorMask);
    }

    public void Write(
        XElement node,
        string? name,
        string item)
    {
        var elem = new XElement(name ?? "String");
        node.Add(elem);
        if (item != null)
        {
            elem.SetAttributeValue(XmlConstants.VALUE_ATTRIBUTE, item);
        }
    }

    public void Write(
        XElement node,
        string? name,
        string? item,
        ErrorMaskBuilder? errorMask)
    {
        if (item == null) return;
        Write(node, name, item);
    }

    public void Write(
        XElement node,
        string? name,
        string item,
        int fieldIndex,
        ErrorMaskBuilder? errorMask)
    {
        errorMask.WrapAction(fieldIndex, () =>
        {
            Write(
                node: node,
                name: name,
                item: item);
        });
    }

    void IXmlTranslation<string>.Write(XElement node, string? name, string item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
    {
        Write(
            node: node,
            name: name,
            item: item);
    }

    bool IXmlTranslation<string>.Parse(XElement node, [MaybeNullWhen(false)] out string item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
    {
        return Parse(
            node: node,
            item: out item,
            errorMask: errorMask);
    }
}