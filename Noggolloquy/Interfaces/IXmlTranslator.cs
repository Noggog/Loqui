using Noggog.Notifying;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public interface IXmlWriter
    {
        void WriteXMLToStream(Stream stream);
        void WriteXML(XmlWriter writer);
        void WriteXML(XmlWriter writer, string name);
    }

    public interface IXmlWriter<M> : IXmlWriter
    {
        void WriteXML(XmlWriter writer, out M errorMask, string name);
    }

    public interface IXmlTranslator : IXmlWriter
    {
        void CopyInFromXML(XElement root, NotifyingFireParameters? cmds);
    }

    public interface IXmlTranslator<M> : IXmlTranslator, IXmlWriter<M>
    {
        void CopyInFromXML(XElement root, out M errorMask, NotifyingFireParameters? cmds);
    }
}
