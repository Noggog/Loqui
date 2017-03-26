using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class StringXmlTranslation : IXmlTranslation<string>
    {
        public string ElementName { get { return "String"; } }
        public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

        public TryGet<string> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                maskObj = new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                return TryGet<string>.Failure;
            }
            maskObj = null;
            if (root.TryGetAttribute("value", out XAttribute val))
            {
                return TryGet<string>.Succeed(val.Value);
            }
            return TryGet<string>.Succeed(null);
        }

        public bool Write(XmlWriter writer, string name, string item, bool doMasks, out object maskObj)
        {
            using (new ElementWrapper(writer, "String"))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                if (!string.IsNullOrEmpty(item))
                {
                    writer.WriteAttributeString("value", item);
                }
            }
            maskObj = null;
            return true;
        }
    }
}
