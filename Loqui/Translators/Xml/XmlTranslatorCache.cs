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
        public static NullXmlTranslation NullTranslation = new NullXmlTranslation();
        public NotifyingItem<GetResponse<IXmlTranslation<object, object>>> NullTranslationItem = new NotifyingItem<GetResponse<IXmlTranslation<object, object>>>(
            defaultVal: GetResponse<IXmlTranslation<object, object>>.Succeed(new XmlTranslationCaster<object, Exception>(NullTranslation)),
            markAsSet: true);
        public NotifyingItem<Type> NullType = new NotifyingItem<Type>(
            defaultVal: null,
            markAsSet: true);
        public Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();
        public Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>> typeDict = new Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>>();
        public HashSet<Type> GenericTypes = new HashSet<Type>();

        public XmlTranslatorCache()
        {
            foreach (var kv in TypeExt.GetInheritingFromGenericInterface(typeof(IXmlTranslation<,>)))
            {
                if (kv.Value.IsAbstract) continue;
                if (kv.Value.Equals(typeof(XmlTranslationCaster<,>))) continue;
                if (kv.Value.IsGenericTypeDefinition)
                {
                    GenericTypes.Add(kv.Value);
                    continue;
                }
                Type transItemType = kv.Key.GetGenericArguments()[0];
                Type maskItemType = kv.Key.GetGenericArguments()[1];
                try
                {
                    SetTranslator(
                        GetCaster(kv.Value, transItemType, maskItemType),
                        transItemType);
                }
                catch (Exception ex)
                {
                    var resp = typeDict.TryCreateValue(
                        transItemType,
                        () =>
                        {
                            return new NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>();
                        }).Item = GetResponse<IXmlTranslation<object, object>>.Fail(ex);
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

        public bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object, object>>> not)
        {
            if (t == null)
            {
                not = NullTranslationItem;
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
                var xmlCaster = GetCaster(xmlConverterGenType, t, LoquiRegistration.GetRegister(t).ErrorMaskType);
                item = new NotifyingItem<GetResponse<IXmlTranslation<object, object>>>(
                    GetResponse<IXmlTranslation<object, object>>.Succeed(xmlCaster));
                typeDict[t] = item;
                not = item;
                return true;
            }
            not = null;
            return false;
        }

        public static IXmlTranslation<object, object> GetCaster(Type xmlType, Type targetType, Type maskType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = typeof(XmlTranslationCaster<,>).MakeGenericType(targetType, maskType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object, Object>;
        }

        public void SetTranslator(IXmlTranslation<Object, Object> transl, Type t)
        {
            var resp = typeDict.TryCreateValue(
                t,
                () =>
                {
                    return new NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>();
                }).Item = GetResponse<IXmlTranslation<object, object>>.Succeed(transl);
            if (string.IsNullOrEmpty(transl.ElementName)) return;
            elementNameTypeDict.TryCreateValue(transl.ElementName, () => new NotifyingItem<Type>()).Item = t;
        }
    }
}
