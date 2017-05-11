using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Loqui.Xml
{
    public class XmlTranslator
    {
        static Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();
        static Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object>>>> typeDict = new Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object>>>>();
        static HashSet<Type> GenericTypes = new HashSet<Type>();

        static XmlTranslator()
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

        private static IXmlTranslation<object> GetCaster(Type xmlType, Type targetType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = typeof(XmlTranslationCaster<>).MakeGenericType(targetType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object>;
        }

        public static bool TranslateElementName(string elementName, out INotifyingItemGetter<Type> t)
        {
            var ret = elementNameTypeDict.TryGetValue(elementName, out NotifyingItem<Type> n);
            t = n;
            return ret;
        }

        public static bool Validate(Type t)
        {
            return typeDict.ContainsKey(t);
        }

        internal static void SetTranslator<T>(IXmlTranslation<T> transl)
        {
            SetTranslator(transl as IXmlTranslation<Object>, typeof(T));
        }

        private static void SetTranslator(IXmlTranslation<Object> transl, Type t)
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

        public static INotifyingItemGetter<GetResponse<IXmlTranslation<Object>>> GetTranslator(Type t)
        {
            TryGetTranslator(t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object>>> not);
            return not;
        }

        public static bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object>>> not)
        {
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

        public static bool TryGetTranslator(Type t, out IXmlTranslation<object> transl)
        {
            if (!typeDict.TryGetValue(t, out NotifyingItem<GetResponse<IXmlTranslation<object>>> not))
            {
                transl = null;
                return false;
            }
            if (not.Item.Failed)
            {
                transl = null;
                return false;
            }
            transl = not.Item.Value;
            return transl != null;
        }
    }

    public class XmlTranslator<T>
    {
        private static NotifyingItem<GetResponse<IXmlTranslation<T>>> _translator = new NotifyingItem<GetResponse<IXmlTranslation<T>>>();
        public static INotifyingItemGetter<GetResponse<IXmlTranslation<T>>> Translator { get { return _translator; } }

        static XmlTranslator()
        {
            var transl = XmlTranslator.GetTranslator(typeof(T));
            transl.Subscribe(
                _translator,
                (change) =>
                {
                    if (change.New.Failed)
                    {
                        _translator.Item = change.New.BubbleFailure<IXmlTranslation<T>>();
                        return;
                    }
                    var caster = change.New.Value as XmlTranslationCaster<T>;
                    _translator.Item = GetResponse<IXmlTranslation<T>>.Succeed(caster.Source);
                });
        }

        public static void SetTranslator(IXmlTranslation<T> translator)
        {
            XmlTranslator.SetTranslator(translator);
        }
    }
}
