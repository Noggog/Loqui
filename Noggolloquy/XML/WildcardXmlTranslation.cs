using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class WildcardXmlTranslation : IXmlTranslation<Object>
    {
        public readonly static WildcardXmlTranslation Instance = new WildcardXmlTranslation();

        public string ElementName
        {
            get
            {
                return null;
            }
        }

        public IXmlTranslation<Object> GetTranslator(Type t)
        {
            return XmlTranslator.GetTranslator(t).Value;
        }

        public bool Validate(Type t)
        {
            return XmlTranslator.Validate(t);
        }

        public TryGet<Object> Parse(XElement root)
        {
            if (!XmlTranslator.TranslateElementName(root.Name.LocalName, out INotifyingItemGetter<Type> t))
            {
                return TryGet<Object>.Failure($"Could not match Element type {root.Name.LocalName} to an XML Translator.");
            }
            var xml = GetTranslator(t.Value);
            return xml.Parse(root);
        }

        public void Write(XmlWriter writer, string name, Object item)
        {
            var xml = GetTranslator(item.GetType());
            xml.Write(writer, name, item);
        }
    }
}
