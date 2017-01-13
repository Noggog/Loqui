using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public class StringXmlTranslation : IXmlTranslation<string>
    {
        public string ElementName { get { return "String"; } }
        public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

        public TryGet<string> Parse(XElement root)
        {
            XAttribute val;
            if (root.TryGetAttribute("value", out val))
            {
                return TryGet<string>.Success(val.Value);
            }
            return TryGet<string>.Success(null);
        }

        public TryGet<string> ParseNoNull(XElement root)
        {
            var ret = Parse(root);
            if (ret.Failed) return ret.BubbleFailure<string>();
            if (string.IsNullOrEmpty(ret.Value)) TryGet<string>.Failure("No content in value attribute.");
            return ret;
        }

        public void Write(XmlWriter writer, string name, string str)
        {
            using (new ElementWrapper(writer, "String"))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                if (!string.IsNullOrEmpty(str))
                {
                    writer.WriteAttributeString("value", str);
                }
            }
        }
    }
}
