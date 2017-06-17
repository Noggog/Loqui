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
            return Cache.Value.TranslateElementName(elementName, out t);
        }

        public static bool Validate(Type t)
        {
            return Cache.Value.Validate(t);
        }

        public static INotifyingItemGetter<GetResponse<IXmlTranslation<Object, Object>>> GetTranslator(Type t)
        {
            TryGetTranslator(t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object, object>>> not);
            return not;
        }

        public static bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object, object>>> not)
        {
            return Cache.Value.TryGetTranslator(t, out not);
        }

        public static bool TryGetTranslator(Type t, out IXmlTranslation<object, object> transl)
        {
            if (!Cache.Value.TryGetTranslator(t, out var not))
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

        internal static void SetTranslator<T, M>(IXmlTranslation<T, M> transl)
        {
            Cache.Value.SetTranslator(transl as IXmlTranslation<Object, Object>, typeof(T));
        }
    }

    public class XmlTranslator<T, M>
    {
        private static NotifyingItem<GetResponse<IXmlTranslation<T, M>>> _translator = new NotifyingItem<GetResponse<IXmlTranslation<T, M>>>();
        public static INotifyingItemGetter<GetResponse<IXmlTranslation<T, M>>> Translator => _translator;

        static XmlTranslator()
        {
            var transl = XmlTranslator.GetTranslator(typeof(T));
            transl.Subscribe(
                _translator,
                (change) =>
                {
                    if (change.New.Failed)
                    {
                        _translator.Item = change.New.BubbleFailure<IXmlTranslation<T, M>>();
                        return;
                    }
                    var caster = change.New.Value as XmlTranslationCaster<T, M>;
                    _translator.Item = GetResponse<IXmlTranslation<T, M>>.Succeed(caster.Source);
                });
        }

        public static void SetTranslator(IXmlTranslation<T, M> translator)
        {
            XmlTranslator.SetTranslator(translator);
        }
    }
}
