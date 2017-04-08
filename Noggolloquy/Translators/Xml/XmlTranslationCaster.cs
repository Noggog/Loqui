using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class XmlTranslationCaster<T> : IXmlTranslation<Object>
    {
        public IXmlTranslation<T> Source { get; private set; }

        public string ElementName => Source.ElementName;

        public XmlTranslationCaster(IXmlTranslation<T> src)
        {
            this.Source = src;
        }

        bool IXmlTranslation<object>.Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            return Source.Write(writer, name, (T)item, doMasks, out maskObj);
        }

        TryGet<object> IXmlTranslation<object>.Parse(XElement root, bool doMasks, out object maskObj)
        {
            return Source.Parse(root, doMasks, out maskObj).Bubble<object>((i) => i);
        }
    }
}
