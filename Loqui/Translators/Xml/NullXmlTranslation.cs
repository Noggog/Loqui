using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Noggog;
using Noggog.Xml;

namespace Loqui.Xml
{
    public class NullXmlTranslation : IXmlTranslation<object, Exception>
    {
        public string ElementName => "Null";

        public TryGet<object> Parse(XElement root, bool doMasks, out Exception maskObj)
        {
            maskObj = null;
            return TryGet<object>.Succeed(null);
        }

        public void Write(XElement node, string name, object item, bool doMasks, out Exception maskObj)
        {
            maskObj = null;
            node.Add(
                new XElement(name));
        }
    }
}
