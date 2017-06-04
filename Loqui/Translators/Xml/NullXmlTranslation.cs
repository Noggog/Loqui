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
    public class NullXmlTranslation : IXmlTranslation<object>
    {
        public string ElementName => "Null";

        public TryGet<object> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<object>.Failure;
                }
                throw ex;
            }
            maskObj = null;
            return TryGet<object>.Succeed(null);
        }

        public void Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString(XmlConstants.NAME_ATTRIBUTE, name);
                }
            }
        }
    }
}
