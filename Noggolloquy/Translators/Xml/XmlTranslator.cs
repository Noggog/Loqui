using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Noggolloquy.Xml
{
    public class XmlTranslator
    {
        static Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();
        static Dictionary<Type, NotifyingItem<IXmlTranslation<Object>>> typeDict = new Dictionary<Type, NotifyingItem<IXmlTranslation<Object>>>();

        static XmlTranslator()
        {
            foreach (var kv in TypeExt.GetInheritingFromGenericInterface(typeof(IXmlTranslation<>)))
            {
                Type transItemType = kv.Key.GetGenericArguments()[0];
                object xmlTransl = Activator.CreateInstance(kv.Value);
                var xmlConverterGenType = typeof(XmlTranslationCaster<>).MakeGenericType(transItemType);
                IXmlTranslation<Object> transl = Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object>;
                SetTranslator(transl, transItemType);
            }
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
            typeDict.TryCreateValue(t, () => new NotifyingItem<IXmlTranslation<Object>>()).Value = transl;
            if (string.IsNullOrEmpty(transl.ElementName)) return;
            elementNameTypeDict.TryCreateValue(transl.ElementName, () => new NotifyingItem<Type>()).Value = t;
        }

        public static INotifyingItem<IXmlTranslation<Object>> GetTranslator(Type t)
        {
            return typeDict[t];
        }

        public static bool TryGetTranslator(Type t, out NotifyingItem<IXmlTranslation<object>> transl)
        {
            return typeDict.TryGetValue(t, out transl);
        }

        public static bool TryGetTranslator(Type t, out IXmlTranslation<object> transl)
        {
            if (!TryGetTranslator(t, out NotifyingItem<IXmlTranslation<object>> not))
            {
                transl = null;
                return false;
            }
            transl = not.Value;
            return transl != null;
        }
    }

    public class XmlTranslator<T>
    {
        private static NotifyingItem<IXmlTranslation<T>> _translator = new NotifyingItem<IXmlTranslation<T>>();
        public static INotifyingItemGetter<IXmlTranslation<T>> Translator { get { return _translator; } }

        static XmlTranslator()
        {
            var transl = XmlTranslator.GetTranslator(typeof(T));
            transl.Subscribe(
                _translator,
                (change) =>
                {
                    _translator.Value = change.New as IXmlTranslation<T>;
                });
        }

        public static void SetTranslator(IXmlTranslation<T> translator)
        {
            XmlTranslator.SetTranslator(translator);
        }
    }
}
