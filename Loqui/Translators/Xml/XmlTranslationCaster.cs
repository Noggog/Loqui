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

        void IXmlTranslation<object, object>.Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            Source.Write(writer, name, (T)item, doMasks, out var subMaskObj);
            maskObj = subMaskObj;
        }

        TryGet<object> IXmlTranslation<object, object>.Parse(XElement root, bool doMasks, out object maskObj)
        {
            var ret = Source.Parse(root, doMasks, out var subMaskObj).Bubble<object>((i) => i);
            maskObj = subMaskObj;
            return ret;
        }
    }
}
