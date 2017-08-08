using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Translators
{
    public abstract class Translator<ObjTransl>
        where ObjTransl : class
    {
        public NotifyingItem<GetResponse<ObjTransl>> NullTranslationItem;
        public NotifyingItem<Type> NullType = new NotifyingItem<Type>(
            defaultVal: null,
            markAsSet: true);

        public Dictionary<Type, NotifyingItem<GetResponse<ObjTransl>>> typeDict = new Dictionary<Type, NotifyingItem<GetResponse<ObjTransl>>>();
        public HashSet<Type> GenericTypes = new HashSet<Type>();

        private Type genericCaster;
        private Type loquiTranslation;
        private Type enumTranslation;

        public Translator(
            Type nullTranslator,
            Type genericCaster,
            Type loquiTranslation,
            Type enumTranslation)
        {
            this.genericCaster = genericCaster;
            this.loquiTranslation = loquiTranslation;
            this.enumTranslation = enumTranslation;

            var nullCasterType = genericCaster.MakeGenericType(typeof(Object), typeof(Exception));
            var nullTranslation = Activator.CreateInstance(nullTranslator);
            this.NullTranslationItem = new NotifyingItem<GetResponse<ObjTransl>>(
                defaultVal: GetResponse<ObjTransl>.Succeed((ObjTransl)Activator.CreateInstance(nullCasterType, new object[] { nullTranslation })),
                markAsSet: true);

            var genInterfType = typeof(ObjTransl).GetGenericTypeDefinition();
            foreach (var kv in TypeExt.GetInheritingFromGenericInterface(genInterfType))
            {
                if (kv.Value.IsAbstract) continue;
                if (kv.Value.Equals(genericCaster)) continue;
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
                            return new NotifyingItem<GetResponse<ObjTransl>>();
                        }).Item = GetResponse<ObjTransl>.Fail(ex);
                }
            }
        }
        public bool Validate(Type t)
        {
            return TryGetTranslator(t, out ObjTransl not);
        }

        public bool TryGetTranslator(Type t, out INotifyingItemGetter<GetResponse<ObjTransl>> not)
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

                var xmlConverterGenType = loquiTranslation.MakeGenericType(loquiTypes);
                var xmlCaster = GetCaster(xmlConverterGenType, regis.ClassType, LoquiRegistration.GetRegister(t).ErrorMaskType);
                item = new NotifyingItem<GetResponse<ObjTransl>>(
                    GetResponse<ObjTransl>.Succeed(xmlCaster));
                typeDict[t] = item;
                not = item;
                return true;
            }

            if (t.IsEnum
                || (Nullable.GetUnderlyingType(t)?.IsEnum ?? false))
            {
                var implType = enumTranslation.MakeGenericType(Nullable.GetUnderlyingType(t) ?? t);
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

        public ObjTransl GetCaster(Type xmlType, Type targetType, Type maskType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = genericCaster.MakeGenericType(targetType, maskType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as ObjTransl;
        }

        protected virtual NotifyingItem<GetResponse<ObjTransl>> SetTranslator_Internal(ObjTransl transl, Type t)
        {
            var resp = typeDict.TryCreateValue(
                t,
                () =>
                {
                    return new NotifyingItem<GetResponse<ObjTransl>>();
                });
            resp.Item = GetResponse<ObjTransl>.Succeed(transl);
            return resp;
        }

        internal NotifyingItem<GetResponse<ObjTransl>> SetTranslator(ObjTransl transl, Type t)
        {
            return SetTranslator_Internal(transl, t);
        }

        public INotifyingItemGetter<GetResponse<ObjTransl>> GetTranslator(Type t)
        {
            TryGetTranslator(t, out INotifyingItemGetter<GetResponse<ObjTransl>> not);
            return not;
        }

        public bool TryGetTranslator(Type t, out ObjTransl transl)
        {
            if (!TryGetTranslator(t, out INotifyingItemGetter<GetResponse<ObjTransl>> not))
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
}
