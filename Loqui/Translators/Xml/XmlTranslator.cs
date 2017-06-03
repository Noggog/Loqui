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
        static Lazy<XmlTranslatorCache> Cache = new Lazy<XmlTranslatorCache>();

        public static bool TranslateElementName(string elementName, out INotifyingItemGetter<Type> t)
        {
            var ret = Cache.Value.elementNameTypeDict.TryGetValue(elementName, out NotifyingItem<Type> n);
            t = n;
            return ret;
        }

        public static bool Validate(Type t)
        {
            return Cache.Value.typeDict.ContainsKey(t);
        }

        public static INotifyingItemGetter<GetResponse<IXmlTranslation<Object>>> GetTranslator(Type t)
        {
            TryGetTranslator(t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object>>> not);
            return not;
        }

        public static bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object>>> not)
        {
            if (Cache.Value.typeDict.TryGetValue(t, out var item))
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
                var xmlCaster = Cache.Value.GetCaster(xmlConverterGenType, t);
                item = new NotifyingItem<GetResponse<IXmlTranslation<object>>>(
                    GetResponse<IXmlTranslation<object>>.Succeed(xmlCaster));
                Cache.Value.typeDict[t] = item;
                not = item;
                return true;
            }
            not = null;
            return false;
        }

        public static bool TryGetTranslator(Type t, out IXmlTranslation<object> transl)
        {
            if (!Cache.Value.typeDict.TryGetValue(t, out NotifyingItem<GetResponse<IXmlTranslation<object>>> not))
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

        internal static void SetTranslator<T>(IXmlTranslation<T> transl)
        {
            Cache.Value.SetTranslator(transl as IXmlTranslation<Object>, typeof(T));
        }
    }

    public class XmlTranslator<T>
    {
        private static NotifyingItem<GetResponse<IXmlTranslation<T>>> _translator = new NotifyingItem<GetResponse<IXmlTranslation<T>>>();
        public static INotifyingItemGetter<GetResponse<IXmlTranslation<T>>> Translator => _translator;

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
