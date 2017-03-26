using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Noggolloquy.Tests.XML
{
    public class XmlUtility
    {
        public static XElement GetBadlyNamedElement()
        {
            return new XElement(XName.Get("BadName"));
        }
    }
}
