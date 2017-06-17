using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class WildcardXmlTranslation : IXmlTranslation<object, object>
    {
        public static readonly WildcardXmlTranslation Instance = new WildcardXmlTranslation();

        public string ElementName => null;

        public IXmlTranslation<object, object> GetTranslator(Type t)
        {
            return XmlTranslator.GetTranslator(t).Item.Value;
        }

        public bool Validate(Type t)
        {
            return XmlTranslator.Validate(t);
        }

        public TryGet<Object> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!XmlTranslator.TranslateElementName(root.Name.LocalName, out INotifyingItemGetter<Type> t))
            {
                var ex = new ArgumentException($"Could not match Element type {root.Name.LocalName} to an XML Translator.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<Object>.Failure;
                }
                else
                {
                    throw ex;
                }
            }
            var xml = GetTranslator(t.Item);
            return xml.Parse(root, doMasks, out maskObj);
        }

        public void Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            var xml = GetTranslator(item?.GetType());
            xml.Write(writer, name, item, doMasks, out maskObj);
        }
    }
}
