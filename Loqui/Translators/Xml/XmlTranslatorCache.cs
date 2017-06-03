using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Xml
{
    class XmlTranslatorCache
    {
        public Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();
        public Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object>>>> typeDict = new Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object>>>>();
        public HashSet<Type> GenericTypes = new HashSet<Type>();
        
        public XmlTranslatorCache()
        {
            foreach (var kv in TypeExt.GetInheritingFromGenericInterface(typeof(IXmlTranslation<>)))
            {
                if (kv.Value.IsAbstract) continue;
                if (kv.Value.Equals(typeof(XmlTranslationCaster<>))) continue;
                if (kv.Value.IsGenericTypeDefinition)
                {
                    GenericTypes.Add(kv.Value);
                    continue;
                }
                Type transItemType = kv.Key.GetGenericArguments()[0];
                try
                {
                    SetTranslator(
                        GetCaster(kv.Value, transItemType),
                        transItemType);
                }
                catch (Exception ex)
                {
                    var resp = typeDict.TryCreateValue(
                        transItemType,
                        () =>
                        {
                            return new NotifyingItem<GetResponse<IXmlTranslation<Object>>>();
                        }).Item = GetResponse<IXmlTranslation<object>>.Fail(ex);
                }
            }
        }

        public IXmlTranslation<object> GetCaster(Type xmlType, Type targetType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = typeof(XmlTranslationCaster<>).MakeGenericType(targetType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object>;
        }

        public void SetTranslator(IXmlTranslation<Object> transl, Type t)
        {
            var resp = typeDict.TryCreateValue(
                t,
                () =>
                {
                    return new NotifyingItem<GetResponse<IXmlTranslation<Object>>>();
                }).Item = GetResponse<IXmlTranslation<object>>.Succeed(transl);
            if (string.IsNullOrEmpty(transl.ElementName)) return;
            elementNameTypeDict.TryCreateValue(transl.ElementName, () => new NotifyingItem<Type>()).Item = t;
        }
    }
}
