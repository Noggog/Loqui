using Noggog;
using Noggolloquy.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public interface IXmlTranslation<T>
    {
        string ElementName { get; }
        void Write(XmlWriter writer, string name, T item);
        TryGet<T> Parse(XElement root);
    }
}
