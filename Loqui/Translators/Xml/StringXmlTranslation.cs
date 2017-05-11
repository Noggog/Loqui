using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class StringXmlTranslation : IXmlTranslation<string>
    {
        public string ElementName => "String";
        public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

        public TryGet<string> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<string>.Failure;
                }
                else
                {
                    throw ex;
                }
            }
            maskObj = null;
            if (root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val))
            {
                return TryGet<string>.Succeed(val.Value);
            }
            return TryGet<string>.Succeed(null);
        }

        public void Write(XmlWriter writer, string name, string item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            Write(writer, name, item);
        }

        public void Write(XmlWriter writer, string name, string item)
        {
            using (new ElementWrapper(writer, "String"))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                if (item != null)
                {
                    writer.WriteAttributeString(XmlConstants.VALUE_ATTRIBUTE, item);
                }
            }
        }
    }
}
