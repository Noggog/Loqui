using Noggog;
using Loqui.Xml;
using System;
using System.Xml;
using System.Xml.Linq;
using Loqui.Translators;
using Loqui.Internal;

namespace Loqui.Xml
{
    public delegate void XmlSubWriteDelegate<in T>(
        XElement node,
        T item,
        ErrorMaskBuilder errorMask, 
        TranslationCrystal translationMask);
    public delegate bool XmlSubParseDelegate<T>(
        XElement root,
        out T item, 
        ErrorMaskBuilder errorMask,
        TranslationCrystal translationMask);

    public interface IXmlTranslation<T> : ITranslation<T>
    {
        string ElementName { get; }
        void Write(
            XElement node, 
            string name, 
            T item, 
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask);
        bool Parse(
            XElement root, 
            out T item, 
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask);
    }
}
