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

        public GetResponse<string> Parse(XElement root, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!root.Name.LocalName.Equals(ElementName))
            {
                return GetResponse<string>.Failure($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
            }
            if (root.TryGetAttribute("value", out XAttribute val))
            {
                return GetResponse<string>.Success(val.Value);
            }
            return GetResponse<string>.Success(null);
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
