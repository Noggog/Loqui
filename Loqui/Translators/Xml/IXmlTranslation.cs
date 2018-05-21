using Noggog;
using Loqui.Xml;
using System;
using System.Xml;
using System.Xml.Linq;
using Loqui.Translators;
using Loqui.Internal;

namespace Loqui.Xml
{
    public delegate void XmlSubWriteDelegate<in T, M>(XElement node, T item, bool doMasks, out M maskObj);
    public delegate bool XmlSubParseDelegate<T>(XElement root, out T item, ErrorMaskBuilder errMask);

    public interface IXmlTranslation<T, M> : ITranslation<T, M>
    {
        string ElementName { get; }
        void Write(XElement node, string name, T item, bool doMasks, out M maskObj);
        bool Parse(XElement root, out T item, ErrorMaskBuilder errMask);
    }
}
