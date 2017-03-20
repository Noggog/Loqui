using Noggog;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class WildcardXmlTranslation : IXmlTranslation<Object>
    {
        static Dictionary<string, Type> elementNameTypeDict = new Dictionary<string, Type>();
        static Dictionary<Type, IXmlTranslation<Object>> typeDict = new Dictionary<Type, IXmlTranslation<Object>>();
        public readonly static WildcardXmlTranslation Instance = new WildcardXmlTranslation();

        static WildcardXmlTranslation()
        {
            foreach (var kv in TypeExt.GetInheritingFromGenericInterface(typeof(IXmlTranslation<>)))
            {
                Type transItemType = kv.Key.GetGenericArguments()[0];
                object xmlTransl = Activator.CreateInstance(kv.Value);
                var xmlConverterGenType = typeof(XmlTranslationCaster<>).MakeGenericType(transItemType);
                IXmlTranslation<Object> transl = Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object>;
                if (string.IsNullOrEmpty(transl.ElementName)) continue;
                elementNameTypeDict.Add(transl.ElementName, transItemType);
                typeDict.Add(transItemType, transl);
            }
        }

        public string ElementName
        {
            get
            {
                return null;
            }
        }

        public IXmlTranslation<Object> GetTranslator(Type t)
        {
            return typeDict[t];
        }

        public bool Validate(Type t)
        {
            return typeDict.ContainsKey(t);
        }

        public TryGet<Object> Parse(XElement root)
        {
            Type t;
            if (!elementNameTypeDict.TryGetValue(root.Name.LocalName, out t))
            {
                return TryGet<Object>.Failure($"Could not match Element type {root.Name.LocalName} to an XML Translator.");
            }
            var xml = GetTranslator(t);
            return xml.Parse(root);
        }

        public void Write(XmlWriter writer, string name, Object item)
        {
            var xml = GetTranslator(item.GetType());
            xml.Write(writer, name, item);
        }
    }
}
