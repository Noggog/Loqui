using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Notifying;

namespace Noggolloquy.Xml
{
    public abstract class ContainerXmlGeneration<T> : IXmlTranslation<IEnumerable<T>>
    {
        protected static INotifyingItemGetter<IXmlTranslation<T>> translator;

        static ContainerXmlGeneration()
        {
            translator = XmlTranslator<T>.Translator;
        }
        
        public abstract string ElementName { get; }

        public abstract TryGet<IEnumerable<T>> Parse(XElement root);

        public abstract void Write(XmlWriter writer, string name, IEnumerable<T> item);
    }
}
