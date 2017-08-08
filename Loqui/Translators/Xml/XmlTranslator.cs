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
        public readonly static XmlTranslator Instance = new XmlTranslator();
        public NullXmlTranslation NullTranslation = new NullXmlTranslation();
        public NotifyingItem<GetResponse<IXmlTranslation<object, object>>> NullTranslationItem;
        public NotifyingItem<Type> NullType = new NotifyingItem<Type>(
            defaultVal: null,
            markAsSet: true);

        public Dictionary<string, NotifyingItem<Type>> elementNameTypeDict = new Dictionary<string, NotifyingItem<Type>>();
        public Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>> typeDict = new Dictionary<Type, NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>>();
        public HashSet<Type> GenericTypes = new HashSet<Type>();

        private XmlTranslator()
        {
            this.NullTranslationItem = new NotifyingItem<GetResponse<IXmlTranslation<object, object>>>(
                defaultVal: GetResponse<IXmlTranslation<object, object>>.Succeed(new XmlTranslationCaster<object, Exception>(NullTranslation)),
                markAsSet: true);
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
            return TryGetTranslator(t, out IXmlTranslation<object, object> not);
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
                var regis = LoquiRegistration.GetRegister(t);
                var loquiTypes = new Type[]
                {
                    regis.ClassType,
                    regis.ErrorMaskType
                };

                var xmlConverterGenType = typeof(LoquiXmlTranslation<,>).MakeGenericType(loquiTypes);
                var xmlCaster = GetCaster(xmlConverterGenType, regis.ClassType, LoquiRegistration.GetRegister(t).ErrorMaskType);
                item = new NotifyingItem<GetResponse<IXmlTranslation<object, object>>>(
                    GetResponse<IXmlTranslation<object, object>>.Succeed(xmlCaster));
                typeDict[t] = item;
                not = item;
                return true;
            }

            if (t.IsEnum
                || (Nullable.GetUnderlyingType(t)?.IsEnum ?? false))
            {
                var implType = typeof(EnumXmlTranslation<>).MakeGenericType(Nullable.GetUnderlyingType(t) ?? t);
                var caster = GetCaster(implType, t, typeof(Exception));
                not = SetTranslator(caster, t);
                return true;
            }

            foreach (var genType in GenericTypes)
            {
                var defs = genType.GetGenericArguments();
                if (defs.Length != 1) continue;
                var def = defs[0];
                if (t.InheritsFrom(def))
                {
                    var implType = genType.MakeGenericType(t);
                    var caster = GetCaster(implType, t, typeof(Exception));
                    not = SetTranslator(caster, t);
                    return true;
                }
            }
            not = null;
            return false;
        }

        public IXmlTranslation<object, object> GetCaster(Type xmlType, Type targetType, Type maskType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = typeof(XmlTranslationCaster<,>).MakeGenericType(targetType, maskType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as IXmlTranslation<Object, Object>;
        }

        internal NotifyingItem<GetResponse<IXmlTranslation<object, object>>> SetTranslator(IXmlTranslation<Object, Object> transl, Type t)
        {
            var resp = typeDict.TryCreateValue(
                t,
                () =>
                {
                    return new NotifyingItem<GetResponse<IXmlTranslation<Object, Object>>>();
                });
            resp.Item = GetResponse<IXmlTranslation<object, object>>.Succeed(transl);
            if (string.IsNullOrEmpty(transl.ElementName)) return resp;
            elementNameTypeDict.TryCreateValue(transl.ElementName, () => new NotifyingItem<Type>()).Item = t;
            return resp;
        }

        public INotifyingItemGetter<GetResponse<IXmlTranslation<Object, Object>>> GetTranslator(Type t)
        {
            TryGetTranslator(t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object, object>>> not);
            return not;
        }

        public bool TryGetTranslator(Type t, out IXmlTranslation<object, object> transl)
        {
            if (!TryGetTranslator(t, out INotifyingItemGetter<GetResponse<IXmlTranslation<object, object>>> not))
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

        internal void SetTranslator<T, M>(IXmlTranslation<T, M> transl)
        {
            SetTranslator(transl as IXmlTranslation<Object, Object>, typeof(T));
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

        public static void SetTranslator(IXmlTranslation<T, M> translator)
        {
            XmlTranslator.Instance.SetTranslator(translator);
        }
    }
}
