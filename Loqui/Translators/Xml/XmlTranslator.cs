using Loqui.Translators;
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
    public class XmlTranslator : Translator<IXmlTranslation<object, object>>
    {
        public readonly static XmlTranslator Instance = new XmlTranslator();

        public Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();

        private XmlTranslator()
            : base (
                  typeof(NullXmlTranslation),
                  typeof(XmlTranslationCaster<,>),
                  typeof(LoquiXmlTranslation<,>),
                  typeof(EnumXmlTranslation<>))
        {
            elementNameTypeDict["Null"] = NullType;
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

        protected override NotifyingItem<GetResponse<IXmlTranslation<object, object>>> SetTranslator_Internal(IXmlTranslation<object, object> transl, Type t)
        {
            var resp = base.SetTranslator_Internal(transl, t);
            if (string.IsNullOrEmpty(transl.ElementName)) return resp;
            elementNameTypeDict.TryCreateValue(transl.ElementName, () => new NotifyingItem<Type>()).Item = t;
            return resp;
        }
    }

    public class XmlTranslator<T, M>
    {
        private static NotifyingItem<GetResponse<IXmlTranslation<T, M>>> _translator = new NotifyingItem<GetResponse<IXmlTranslation<T, M>>>();
        public static INotifyingItemGetter<GetResponse<IXmlTranslation<T, M>>> Translator => _translator;

        static XmlTranslator()
        {
            var transl = XmlTranslator.Instance.GetTranslator(typeof(T));
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
    }
}
