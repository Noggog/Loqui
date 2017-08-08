using Noggog;
using Loqui.Xml;
using System;
using System.Xml;
using System.Xml.Linq;
using Loqui.Translators;

namespace Loqui.Xml
{
    public delegate TryGet<T> XmlSubParseDelegate<T, M>(XElement root, bool doMasks, out M maskObj);
    public delegate void XmlSubWriteDelegate<in T, M>(T item, bool doMasks, out M maskObj);

    public interface IXmlTranslation<T, M> : ITranslation<T, M>
    {
        string ElementName { get; }
        void Write(XmlWriter writer, string name, T item, bool doMasks, out M maskObj);
        TryGet<T> Parse(XElement root, bool doMasks, out M maskObj);
    }
}
