using Loqui.Internal;
using System.Xml.Linq;

namespace Loqui.Xml;

public interface IXmlWriteTranslator
{
    void Write(
        XElement node,
        object item,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask,
        string? name);
}