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
        public NotifyingItem<GetResponse<IXmlTranslation<object>>> NullTranslation = new NotifyingItem<GetResponse<IXmlTranslation<object>>>(
            defaultVal: GetResponse<IXmlTranslation<object>>.Succeed(new NullXmlTranslation()),
            markAsSet: true);
        public NotifyingItem<Type> NullType = new NotifyingItem<Type>(
            defaultVal: null,
            markAsSet: true);
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
            elementNameTypeDict["Null"] = NullType;
        }

        public bool Validate(Type t)
        {
            return TryGetTranslator(t, out var not);
        }

        public bool TranslateElementName(string elementName, out INotifyingItemGetter<Type> t)
        {
            var ret = elementNameTypeDict.TryGetValue(elementName, out NotifyingItem<Type> n);
            if (!ret)
            {
                var regis = LoquiRegistration.GetRegisterByFullName(elementName);
                if (regis != null)
                {
                    var not = elementNameTypeDict.TryCreateValue(elementName, () => new NotifyingItem<Type>());
                    not.Item = regis.ClassType;
                    t = not;
                    return true;
                }
                else
                {
                    elementNameTypeDict[elementName] = null;
                }
            }
            t = n;
            return ret && n != null;
        }

        public bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object>>> not)
        {
            if (t == null)
            {
                not = NullTranslation;
                return true;
            }
            if (typeDict.TryGetValue(t, out var item))
            {
                not = item;
                return true;
            }
            if (LoquiRegistration.IsLoquiType(t))
            {
                var loquiTypes = new Type[]
                {
                    t,
                    LoquiRegistration.GetRegister(t).ErrorMaskType
                };
                var xmlConverterGenType = typeof(LoquiXmlTranslation<,>).MakeGenericType(loquiTypes);
                var xmlCaster = GetCaster(xmlConverterGenType, t);
                item = new NotifyingItem<GetResponse<IXmlTranslation<object>>>(
                    GetResponse<IXmlTranslation<object>>.Succeed(xmlCaster));
                typeDict[t] = item;
                not = item;
                return true;
            }
            not = null;
            return false;
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
