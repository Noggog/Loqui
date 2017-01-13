using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public class XmlTranslationCaster<T> : IXmlTranslation<Object>
    {
        public IXmlTranslation<T> Source { get; private set; }

        public string ElementName { get { return Source.ElementName; } }

        public XmlTranslationCaster(IXmlTranslation<T> src)
        {
            this.Source = src;
        }

        void IXmlTranslation<object>.Write(XmlWriter writer, string name, object item)
        {
            Source.Write(writer, name, (T)item);
        }

        TryGet<object> IXmlTranslation<object>.Parse(XElement root)
        {
            return Source.Parse(root).Bubble<object>((i) => i);
        }
    }
}
