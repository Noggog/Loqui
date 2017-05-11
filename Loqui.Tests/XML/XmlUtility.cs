using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Tests.XML
{
    public class XmlUtility
    {
        public const string TYPICAL_NAME = "SomeField";
        public const string NAME_ATTR = "name";

        public static XElement GetElementNoValue(string nodeName, string name = null)
        {
            var elem = new XElement(XName.Get(nodeName));
            if (!string.IsNullOrWhiteSpace(name))
            {
                elem.SetAttributeValue(XName.Get(XmlConstants.NAME_ATTRIBUTE), name);
            }
            return elem;
        }

        public static XElement GetBadlyNamedElement()
        {
            return new XElement(XName.Get("BadName"));
        }

        public static XmlWriteBundle GetWriteBundle()
        {
            return new XmlWriteBundle();
        }

        public class XmlWriteBundle : IDisposable
        {
            public MemoryStream MemStream;
            public XmlWriter Writer;

            public XmlWriteBundle()
            {
                this.MemStream = new MemoryStream();
                this.Writer = XmlWriter.Create(this.MemStream);
            }

            public XElement Resolve()
            {
                this.Writer.Flush();
                this.MemStream.Position = 0;
                XDocument xDoc = XDocument.Load(this.MemStream);
                return xDoc.Root;
            }

            public void Dispose()
            {
                this.Writer?.Dispose();
                this.MemStream.Dispose();
            }
        }
    }
}
