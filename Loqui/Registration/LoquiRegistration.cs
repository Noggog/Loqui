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
        public delegate object UntypedCopyFunction(object item, object copy);
        private readonly static object _registersLock = new object();
        private readonly static Dictionary<ObjectKey, ILoquiRegistration> _registers = new Dictionary<ObjectKey, ILoquiRegistration>();
        private readonly static Dictionary<string, ILoquiRegistration?> _nameRegisters = new Dictionary<string, ILoquiRegistration?>();
        private readonly static Dictionary<Type, ILoquiRegistration?> _typeRegister = new Dictionary<Type, ILoquiRegistration?>();
        private readonly static Dictionary<Type, Type> _genericRegisters = new Dictionary<Type, Type>();
        private readonly static Dictionary<Type, object> _createFuncRegister = new Dictionary<Type, object>();
        private readonly static Dictionary<Type, object> _copyInFuncRegister = new Dictionary<Type, object>();
        private readonly static Dictionary<(Type TSource, Type TResult), object> _copyFuncRegister = new Dictionary<(Type TSource, Type TResult), object>();
        private readonly static Dictionary<(Type TSource, Type TResult), UntypedCopyFunction> _untypedCopyFuncRegister = new Dictionary<(Type TSource, Type TResult), UntypedCopyFunction>();
        private readonly static Dictionary<string, Type?> _cache = new Dictionary<string, Type?>();

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
            lock (_registersLock)
            {
                foreach (var regis in registrations)
                {
                    regis?.Register();
                }
            }
        }

        public static bool TryGetType(string name, [MaybeNullWhen(false)] out Type type)
        {
            if (!_cache.TryGetValue(name, out type))
            {
                try
                {
                    TypeStringNode node = Parse(name);
                    throw new NotImplementedException();
                }
                catch
                {
                    _cache[name] = null;
                    type = null;
                }
            }

            return type != null;
        }

        public static bool Instantiate(string name, object[] args, out object? obj)
        {
            if (!TryGetType(name, out Type? type))
            {
                obj = null;
                return false;
            }
            obj = Activator.CreateInstance(type, args);
            return true;
        }

        class TypeStringNode
        {
            public string Name;
            public TypeStringNode[]? Children;

            public TypeStringNode(string name)
            {
                this.Name = name;
            }
        }
        static char[] searchChars = new char[] { '<', '>', ',' };
        static char[] search2Chars = new char[] { '>', ',' };
        private static TypeStringNode Parse(string str)
        {
            str = str.Trim();
            int openIndex = str.IndexOfAny(searchChars);
            if (openIndex == -1)
            {
                return new TypeStringNode(str);
            }

            if (str[openIndex] != '<')
            { // Opened with something weird
                throw new ArgumentException($"{str} was malformed.");
            }

            int closeIndex = str.LastIndexOf('>', openIndex);
            if (closeIndex == -1)
            { // No closing index
                throw new ArgumentException($"{str} was malformed.");
            }

            if (closeIndex != str.Length - 1)
            { // Closing index had things afterwards
                throw new ArgumentException($"{str} was malformed.");
            }

            string content = str.Substring(openIndex + 1, closeIndex - openIndex - 1);
            if (content.Length == 0)
            { // No generic content
                throw new ArgumentException($"{str} was malformed.");
            }

            string[] commaSplit = content.Split(',');

            var ret = new TypeStringNode(str.Substring(0, openIndex).Trim());
            if (commaSplit.Length > 0)
            {
                ret.Children = commaSplit.Select((t) => Parse(str)).ToArray();
            }
            return ret;
        }

        private static Type? Parse(TypeStringNode nodes)
        {
            var typeStr = nodes.Name;
            TypeStringNode[]? children = nodes.Children;
            if (children == null) return Type.GetType(typeStr);
            if (children.Length == 0) return Type.GetType(typeStr);

            typeStr += "`" + children.Length;
            Type? mainType = Type.GetType(typeStr);
            if (mainType == null) return null;

            Type[] subTypes = children.Select((child) => Parse(child)!).ToArray();
            if (subTypes.Any((t) => t == null)) return null;

            return mainType.MakeGenericType(subTypes);
        }

        public static void Register(ILoquiRegistration reg)
        {
            lock (_registersLock)
            {
                if (_typeRegister.ContainsKey(reg.ClassType)) return;
                _registers.Add(reg.ObjectKey, reg);
                _nameRegisters.Add(reg.FullName, reg);
                var prefixStrs = reg.FullName.Split('.');
                _nameRegisters.Add(String.Join(".", prefixStrs[0..^1].And(reg.SetterType.Name)), reg);
                _nameRegisters.Add(String.Join(".", prefixStrs[0..^1].And(reg.GetterType.Name)), reg);
                _typeRegister.Add(reg.ClassType, reg);
                _typeRegister.Add(reg.SetterType, reg);
                _typeRegister.Add(reg.GetterType, reg);
                if (reg.InternalGetterType != null)
                {
                    _typeRegister.Add(reg.InternalGetterType, reg);
                }
                if (reg.InternalSetterType != null)
                {
                    _typeRegister.Add(reg.InternalSetterType, reg);
                }
                if (reg.GenericRegistrationType != null
                    && !reg.GenericRegistrationType.Equals(reg.GetType()))
                {
                    _genericRegisters[reg.ClassType] = reg.GenericRegistrationType;
                }
            }
        }

        public static bool IsLoquiType(Type t)
        {
            lock (_registersLock)
            {
                return _typeRegister.ContainsKey(t);
            }
        }

        public static ILoquiRegistration GetRegister(Type t)
        {
            if (TryGetRegister(t, out var regis)) return regis;
            throw new ArgumentException("Type was not a Loqui type: " + t);
        }

        public static ILoquiRegistration? TryGetRegister(Type t)
        {
            if (TryGetRegister(t, out var regis)) return regis;
            return null;
        }

        public static bool TryGetRegister(Type t, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            lock (_registersLock)
            {
                if (_typeRegister.TryGetValue(t, out regis!))
                {
                    return regis != null;
                }
                if (t.IsGenericType)
                {
                    Type genType = t.GetGenericTypeDefinition();
                    if (_genericRegisters.TryGetValue(genType, out var genRegisterType))
                    {
                        if (genRegisterType == null) return false;
                    }
                    else
                    {
                        if (!TryLocateRegistration(t, out var tRegis)) return false;
                        if (tRegis.GenericRegistrationType == null) return false;
                        genRegisterType = tRegis.GenericRegistrationType;
                        _genericRegisters[t] = genRegisterType;
                    }
                    regis = GetGenericRegistration(genRegisterType, t.GetGenericArguments())!;
                    _typeRegister[t] = regis;
                }
                else
                {
                    TryLocateRegistration(t, out regis);
                    _typeRegister[t] = regis;
                }
                return regis != null;
            }
        }

        public static bool TryLocateRegistration(Type t, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            if (t.GetInterface(nameof(ILoquiObject)) == null)
            {
                regis = null!;
                return false;
            }

            PropertyInfo? getRegistrationProperty(Type type)
            {
                return type.GetMembers(BindingFlags.Public | BindingFlags.Static)
                    .Where((m) => m.Name.Equals(nameof(ILoquiObject.Registration))
                        && m.MemberType == MemberTypes.Property)
                    .Select((m) => m as PropertyInfo)
                    .Where((m) => m != null)
                    .FirstOrDefault();
            }
            var regisField = getRegistrationProperty(t);
            if (regisField != null)
            {
                regis = (regisField.GetValue(null) as ILoquiRegistration)!;
                return regis != null;
            }
            else
            {
                regis = null;
                return false;
            }
        }

        private static ILoquiRegistration? GetGenericRegistration(Type genRegisterType, Type[] subTypes)
        {
            var customGenRegisterType = genRegisterType.MakeGenericType(subTypes);
            var instanceProp = customGenRegisterType.GetField("GenericInstance", BindingFlags.Static | BindingFlags.Public)!;
            return instanceProp.GetValue(null) as ILoquiRegistration;
        }

        public static ILoquiRegistration GetRegister(ObjectKey key)
        {
            if (TryGetRegister(key, out var regis)) return regis;
            throw new ArgumentException("Object Key was not a defined Loqui type: " + key);
        }

        public static bool TryGetRegister(ObjectKey key, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            lock (_registersLock)
            {
                return _registers.TryGetValue(key, out regis);
            }
        }

        public static ILoquiRegistration? GetRegisterByFullName(string str)
        {
            if (TryGetRegisterByFullName(str, out var regis)) return regis;
            return null;
        }

        public static bool TryGetRegisterByFullName(string str, [MaybeNullWhen(false)] out ILoquiRegistration regis)
        {
            lock (_registersLock)
            {
                if (_nameRegisters.TryGetValue(str, out regis!))
                {
                    return regis != null;
                }
                if (str == null) return false;
                int genIndex = str.IndexOf("<");
                int genEndIndex = str.LastIndexOf(">");
                if (genIndex == -1 || genEndIndex == -1) return false;
                if (!TryGetRegisterByFullName(str.Substring(0, genIndex), out var baseReg)) return false;
                var genRegisterType = baseReg.GenericRegistrationType;
                if (genRegisterType == null) throw new ArgumentException();
                str = str.Substring(genIndex + 1, genEndIndex - genIndex - 1);
                var subTypeStrings = str.Split(',');
                var subTypes = subTypeStrings.Select((tStr) => TypeExt.FindType(tStr.Trim())!).ToArray();
                if (subTypes.Any((t) => t == null))
                {
                    _nameRegisters[str] = null;
                    return false;
                }
                regis = GetGenericRegistration(genRegisterType, subTypes)!;
                if (regis != null)
                {
                    _typeRegister[regis.ClassType] = regis;
                }
                _nameRegisters[str] = regis;
                return regis != null;
            }
        }

        public static Func<IEnumerable<KeyValuePair<ushort, object>>, T>? GetCreateFunc<T>()
        {
            var t = typeof(T);
            if (_createFuncRegister.TryGetValue(t, out var createFunc))
            {
                return createFunc as Func<IEnumerable<KeyValuePair<ushort, object>>, T>;
            }
            var methodInfo = t.GetMethod(
                Constants.CREATE_FUNC_NAME,
                Constants.CREATE_FUNC_PARAM_ARRAY)!;
            var param = Expression.Parameter(Constants.CREATE_FUNC_PARAM, "fields");
            var tArgs = new List<Type>();
            foreach (var p in methodInfo.GetParameters())
            {
                tArgs.Add(p.ParameterType);
            }
            tArgs.Add(methodInfo.ReturnType);
            var del = Expression.Lambda(
                delegateType: Expression.GetDelegateType(tArgs.ToArray()),
                body: Expression.Call(methodInfo, param),
                parameters: param).Compile();
            _createFuncRegister[t] = del;
            return del as Func<IEnumerable<KeyValuePair<ushort, object>>, T>;
        }

        public static Action<IEnumerable<KeyValuePair<ushort, object>>, T>? GetCopyInFunc<T>()
        {
            var t = typeof(T);
            if (_copyInFuncRegister.TryGetValue(t, out var createFunc))
            {
                return createFunc as Action<IEnumerable<KeyValuePair<ushort, object>>, T>;
            }
            var register = GetRegister(t);
            var methodInfo = t.GetMethod(
                Constants.COPYIN_FUNC_NAME,
                new Type[]
                {
                    Constants.CREATE_FUNC_PARAM,
                    t,
                })!;
            var fields = Expression.Parameter(Constants.CREATE_FUNC_PARAM, "fields");
            var obj = Expression.Parameter(t, "obj");
            var tArgs = new List<Type>();
            foreach (var p in methodInfo.GetParameters())
            {
                tArgs.Add(p.ParameterType);
            }
            tArgs.Add(methodInfo.ReturnType);
            var del = Expression.Lambda(
                delegateType: Expression.GetDelegateType(tArgs.ToArray()),
                body: Expression.Call(methodInfo, fields, obj),
                parameters: new ParameterExpression[] { fields, obj }).Compile();
            _copyInFuncRegister[t] = del;
            return del as Action<IEnumerable<KeyValuePair<ushort, object>>, T>;
        }

        public static Func<TSource, object, TResult>? GetCopyFunc<TResult, TSource>()
            where TSource : notnull
        {
            return GetCopyFunc<TResult, TSource>(typeof(TSource), typeof(TResult));
        }

        public static Func<TSource, object, TResult>? GetCopyFunc<TResult, TSource>(Type tSource, Type tResult)
            where TSource : notnull
        {
            if (_copyFuncRegister.TryGetValue((tSource, tResult), out var copyFunc))
            {
                return copyFunc as Func<TSource, object, TResult>;
            }

            var untypedCopyFunc = GetCopyFunc(tSource, tResult);
            if (untypedCopyFunc == null) throw new ArgumentException();

            //var methodParams = methodInfo.GetParameters();
            //var item = Expression.Parameter(t, "item");
            //var copyMask = Expression.Parameter(typeof(object), "copyMask");
            //var copyCast = Expression.TypeAs(
            //    copyMask,
            //    methodParams[1].ParameterType);
            //var defaults = Expression.Parameter(typeof(object), "defaults");
            //var defaultsCast = Expression.TypeAs(
            //    defaults,
            //    methodParams[2].ParameterType);
            //var tArgs = new List<Type>();
            //foreach (var p in methodInfo.GetParameters())
            //{
            //    tArgs.Add(p.ParameterType);
            //}
            //tArgs.Add(methodInfo.ReturnType);
            //var del = Expression.Lambda(
            //    delegateType: Expression.GetDelegateType(tArgs.ToArray()),
            //    body: Expression.Call(methodInfo, item, copyCast, defaultsCast),
            //    parameters: new ParameterExpression[] { item, copyMask, defaults }).Compile();

            var f = new Func<TSource, object, TResult>(
                (item, copy) =>
                {
                    return (TResult)untypedCopyFunc(item, copy);
                });
            _copyFuncRegister[(tSource, tResult)] = f;
            return f;
        }

        public static UntypedCopyFunction GetCopyFunc(Type tSource, Type tResult)
        {
            if (_untypedCopyFuncRegister.TryGetValue((tSource, tResult), out var copyFunc))
            {
                return copyFunc;
            }
            var register = GetRegister(tResult);
            var methodInfo = tResult.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where((m) => m.IsGenericMethod)
                .Where((m) => m.Name.Equals(Constants.COPY_FUNC_NAME))
                .First();
            methodInfo = methodInfo.MakeGenericMethod(
                new Type[]
                {
                    tSource,
                    tResult,
                });

            var f = new UntypedCopyFunction(
                (item, copy) =>
                {
                    return methodInfo.Invoke(
                        null,
                        new object[]
                        {
                            item,
                            copy
                        })!;
                });
            _untypedCopyFuncRegister[(tSource, tResult)] = f;
            return f;
        }
    }
}
