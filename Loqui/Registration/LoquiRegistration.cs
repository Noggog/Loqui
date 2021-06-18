using Noggog;
using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Loqui
{
    public static class LoquiRegistration
    {
        public readonly static LoquiRegister StaticRegister = new();
        
        static LoquiRegistration()
        {
            if (!LoquiRegistrationSettings.AutomaticRegistration) return;
            foreach (var interf in TypeExt.GetInheritingFromInterface<IProtocolRegistration>(
                loadAssemblies: true))
            {
                IProtocolRegistration? regis = Activator.CreateInstance(interf) as IProtocolRegistration;
                regis?.Register();
            }
        }

        public static void SpinUp()
        {
            // Do nothing. Work is done in static ctor
        }

        public static void Register(params IProtocolRegistration[] registrations)
        {
            StaticRegister.Register(registrations);
        }

        public static bool TryGetType(string name, [MaybeNullWhen(false)] out Type type)
        {
            return StaticRegister.TryGetType(name, out type);
        }

        public static bool Instantiate(string name, object[] args, out object? obj)
        {
            return StaticRegister.Instantiate(name, args, out obj);
        }

        public static void Register(ILoquiRegistration reg)
        {
            StaticRegister.Register(reg);
        }

        public static bool IsLoquiType(Type t)
        {
            return StaticRegister.IsLoquiType(t);
        }

        public static ILoquiRegistration GetRegister(Type t)
        {
            return StaticRegister.GetRegister(t);
        }

        public static ILoquiRegistration? TryGetRegister(Type t)
        {
            return StaticRegister.TryGetRegister(t);
        }

        public static bool TryGetRegister(Type t, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            return StaticRegister.TryGetRegister(t, out regis);
        }

        public static bool TryLocateRegistration(Type t, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            return StaticRegister.TryLocateRegistration(t, out regis);
        }

        public static ILoquiRegistration GetRegister(ObjectKey key)
        {
            return StaticRegister.GetRegister(key);
        }

        public static bool TryGetRegister(ObjectKey key, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            return StaticRegister.TryGetRegister(key, out regis);
        }

        public static ILoquiRegistration? GetRegisterByFullName(string str)
        {
            return StaticRegister.GetRegisterByFullName(str);
        }

        public static bool TryGetRegisterByFullName(string str, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            return StaticRegister.TryGetRegisterByFullName(str, out regis);
        }

        public static Func<IEnumerable<KeyValuePair<ushort, object>>, T>? GetCreateFunc<T>()
        {
            return StaticRegister.GetCreateFunc<T>();
        }

        public static Action<IEnumerable<KeyValuePair<ushort, object>>, T>? GetCopyInFunc<T>()
        {
            return StaticRegister.GetCopyInFunc<T>();
        }

        public static Func<TSource, object, TResult>? GetCopyFunc<TResult, TSource>()
            where TSource : notnull
        {
            return StaticRegister.GetCopyFunc<TResult, TSource>();
        }

        public static Func<TSource, object, TResult>? GetCopyFunc<TResult, TSource>(Type tSource, Type tResult)
            where TSource : notnull
        {
            return StaticRegister.GetCopyFunc<TResult, TSource>(tSource, tResult);
        }

        public static LoquiRegister.UntypedCopyFunction GetCopyFunc(Type tSource, Type tResult)
        {
            return StaticRegister.GetCopyFunc(tSource, tResult);
        }
    }
}
