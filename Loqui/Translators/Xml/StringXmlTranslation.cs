using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class StringXmlTranslation : IXmlTranslation<string, Exception>
    {
        public string ElementName => "String";
        public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

        public TryGet<string> Parse(XElement root)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                throw new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
            }
            if (root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val))
            {
                return TryGet<string>.Succeed(val.Value);
            }
            return TryGet<string>.Succeed(null);
        }

        public TryGet<string> Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (doMasks)
                {
                    errorMask = ex;
                    return TryGet<string>.Failure;
                }
                throw ex;
            }
            errorMask = null;
            if (root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val))
            {
                return TryGet<string>.Succeed(val.Value);
            }
            return TryGet<string>.Succeed(null);
        }

        public void Write(XmlWriter writer, string name, string item, bool doMasks, out Exception errorMask)
        {
            try
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
                errorMask = null;
            }
            catch (Exception ex)
            {
                if (!doMasks) throw;
                errorMask = ex;
            }
        }
    }
}
