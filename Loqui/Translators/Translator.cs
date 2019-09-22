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
        public GetResponse<ObjTransl> NullTranslationItem;
        public Type NullType = default;

        public Dictionary<Type, GetResponse<ObjTransl>> typeDict = new Dictionary<Type, GetResponse<ObjTransl>>();
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

            var nullCasterType = genericCaster.MakeGenericType(typeof(Object));
            var nullTranslation = Activator.CreateInstance(nullTranslator);
            this.NullTranslationItem = GetResponse<ObjTransl>.Succeed((ObjTransl)Activator.CreateInstance(nullCasterType, new object[] { nullTranslation }));

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
                try
                {
                    SetTranslator(
                        GetCaster(kv.Value, transItemType),
                        transItemType);
                }
                catch (Exception ex)
                {
                    typeDict[transItemType] = GetResponse<ObjTransl>.Fail(ex);
                }
            }
        }
        public bool Validate(Type t)
        {
            return TryGetTranslator(t, out ObjTransl not);
        }

        public bool TryGetTranslator(Type t, out GetResponse<ObjTransl> not)
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
                    regis.ClassType
                };

                var xmlConverterGenType = loquiTranslation.MakeGenericType(loquiTypes);
                var xmlCaster = GetCaster(xmlConverterGenType, regis.ClassType);
                item = GetResponse<ObjTransl>.Succeed(xmlCaster);
                typeDict[t] = item;
                not = item;
                return true;
            }

            if (t.IsEnum
                || (Nullable.GetUnderlyingType(t)?.IsEnum ?? false))
            {
                var implType = enumTranslation.MakeGenericType(Nullable.GetUnderlyingType(t) ?? t);
                var caster = GetCaster(implType, t);
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
                    var caster = GetCaster(implType, t);
                    not = SetTranslator(caster, t);
                    return true;
                }
            }
            not = default;
            return false;
        }

        public ObjTransl GetCaster(Type xmlType, Type targetType)
        {
            object xmlTransl = Activator.CreateInstance(xmlType);
            var xmlConverterGenType = genericCaster.MakeGenericType(targetType);
            return Activator.CreateInstance(xmlConverterGenType, args: new object[] { xmlTransl }) as ObjTransl;
        }

        protected virtual GetResponse<ObjTransl> SetTranslator_Internal(ObjTransl transl, Type t)
        {
            var resp = typeDict.TryCreateValue(
                t,
                () =>
                {
                    return GetResponse<ObjTransl>.Succeed(transl);
                });
            return resp;
        }

        internal GetResponse<ObjTransl> SetTranslator(ObjTransl transl, Type t)
        {
            return SetTranslator_Internal(transl, t);
        }

        public GetResponse<ObjTransl> GetTranslator(Type t)
        {
            TryGetTranslator(t, out GetResponse<ObjTransl> not);
            return not;
        }

        public bool TryGetTranslator(Type t, out ObjTransl transl)
        {
            if (!TryGetTranslator(t, out GetResponse<ObjTransl> not))
            {
                transl = null;
                return false;
            }
            if (not.Failed)
            {
                transl = null;
                return false;
            }
            transl = not.Value;
            return transl != null;
        }
    }
}
