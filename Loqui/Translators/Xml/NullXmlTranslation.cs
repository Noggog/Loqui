using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Loqui.Internal;
using Noggog;
using Noggog.Xml;

namespace Loqui.Xml
{
    public class NullXmlTranslation : IXmlTranslation<object>
    {
        public string ElementName => "Null";

        public bool Parse(XElement root, out object item, ErrorMaskBuilder errorMask)
        {
            item = null;
            return true;
        }

        public void Write(XElement node, string name, object item, ErrorMaskBuilder errorMask)
        {
            node.Add(
                new XElement(name));
        }
    }
}
