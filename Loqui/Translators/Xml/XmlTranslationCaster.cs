using Loqui.Internal;
using Loqui.Translators;
using Noggog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class XmlTranslationCaster<T> : IXmlTranslation<Object>, ITranslationCaster<T>
    {
        public IXmlTranslation<T> Source { get; }
        ITranslation<T> ITranslationCaster<T>.Source => this.Source;

        public string ElementName => Source.ElementName;

        public XmlTranslationCaster(IXmlTranslation<T> src)
        {
            this.Source = src;
        }

        void IXmlTranslation<object>.Write(
            XElement node, 
            string name,
            object item, 
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
        {
            Source.Write(node, name, (T)item, errorMask, translationMask);
        }

        bool IXmlTranslation<object>.Parse(
            XElement root,
            [MaybeNullWhen(false)] out object item, 
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
        {
            if (Source.Parse(root, out T sourceItem, errorMask, translationMask))
            {
                item = sourceItem!;
                return item != null;
            }
            item = default!;
            return false;
        }
    }
}
