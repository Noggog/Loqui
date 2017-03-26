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

        public TryGet<Object> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!XmlTranslator.TranslateElementName(root.Name.LocalName, out INotifyingItemGetter<Type> t))
            {
                throw new ArgumentException($"Could not match Element type {root.Name.LocalName} to an XML Translator.");
            }
            var xml = GetTranslator(t.Value);
            return xml.Parse(root, doMasks, out maskObj);
        }

        public bool Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            var xml = GetTranslator(item.GetType());
            return xml.Write(writer, name, item, doMasks, out maskObj);
        }
    }
}
