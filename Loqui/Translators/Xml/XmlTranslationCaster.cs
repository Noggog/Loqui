using Loqui.Internal;
using Loqui.Translators;
using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class XmlTranslationCaster<T, M> : IXmlTranslation<Object, Object>, ITranslationCaster<T, M>
    {
        public IXmlTranslation<T, M> Source { get; }
        ITranslation<T, M> ITranslationCaster<T, M>.Source => this.Source;

        public string ElementName => Source.ElementName;

        public XmlTranslationCaster(IXmlTranslation<T, M> src)
        {
            this.Source = src;
        }

        void IXmlTranslation<object, object>.Write(XElement node, string name, object item, bool doMasks, out object maskObj)
        {
            Source.Write(node, name, (T)item, doMasks, out var subMaskObj);
            maskObj = subMaskObj;
        }

        bool IXmlTranslation<object, object>.Parse(XElement root, out object item, ErrorMaskBuilder errorMask)
        {
            if (Source.Parse(root, out T sourceItem, errorMask))
            {
                item = sourceItem;
                return true;
            }
            item = null;
            return false;
        }
    }
}
