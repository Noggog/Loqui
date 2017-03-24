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
        bool Write(XmlWriter writer, string name, T item, bool doMasks, out object maskObj);
        TryGet<T> Parse(XElement root, bool doMasks, out object maskObj);
    }
}
