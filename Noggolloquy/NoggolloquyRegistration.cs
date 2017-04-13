using Noggog;
using Noggolloquy.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Noggolloquy
{
    public static class NoggolloquyRegistration
    {
        static Dictionary<ObjectKey, NoggolloquyTypeRegister> Registers = new Dictionary<ObjectKey, NoggolloquyTypeRegister>();
        static Dictionary<string, NoggolloquyTypeRegister> NameRegisters = new Dictionary<string, NoggolloquyTypeRegister>();
        static Dictionary<Type, NoggolloquyTypeRegister> TypeRegister = new Dictionary<Type, NoggolloquyTypeRegister>();
        static Dictionary<Type, Delegate> CreateFuncRegister = new Dictionary<Type, Delegate>();
        static Dictionary<Type, Delegate> CopyInFuncRegister = new Dictionary<Type, Delegate>();
        static Dictionary<string, Type> cache = new Dictionary<string, Type>();

        static NoggolloquyRegistration()
        {
            foreach (var interf in TypeExt.GetInheritingFromInterface<IProtocolRegistration>(
                loadAssemblies: false))
            {
                IProtocolRegistration regis = Activator.CreateInstance(interf) as IProtocolRegistration;
                regis.Register();
            }
        }
        
        public static TryGet<Type> TryGetType(string name)
        {
            if (!cache.TryGetValue(name, out Type t))
            {
                try
                {
                    TypeStringNode node = Parse(name);
                    throw new NotImplementedException();
                }
                catch
                {
                    cache[name] = null;
                    return TryGet<Type>.Failure;
                }
            }

            return TryGet<Type>.Create(t != null, t);
        }

        public static TryGet<object> Instantiate(string name, object[] args)
        {
            TryGet<Type> tGet = TryGetType(name);
            if (tGet.Failed) return tGet.BubbleFailure<object>();
            return TryGet<object>.Succeed(Activator.CreateInstance(tGet.Value, args));
        }

        class TypeStringNode
        {
            public string Name;
            public TypeStringNode[] Children;
        }
        static char[] searchChars = new char[] { '<', '>', ',' };
        static char[] search2Chars = new char[] { '>', ',' };
        private static TypeStringNode Parse(string str)
        {
            str = str.Trim();
            int openIndex = str.IndexOfAny(searchChars);
            if (openIndex == -1)
            {
                return new TypeStringNode() { Name = str };
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

            var ret = new TypeStringNode()
            {
                Name = str.Substring(0, openIndex).Trim()
            };
            if (commaSplit.Length > 0)
            {
                ret.Children = commaSplit.Select((t) => Parse(str)).ToArray();
            }
            return ret;
        }

        private static Type Parse(TypeStringNode nodes)
        {
            string typeStr = nodes.Name;
            if (nodes.Children.Length == 0) return Type.GetType(typeStr);

            typeStr += "`" + nodes.Children.Length;
            Type mainType = Type.GetType(typeStr);
            if (mainType == null) return null;

            Type[] subTypes = nodes.Children.Select((child) => Parse(child)).ToArray();
            if (subTypes.Any((t) => t == null)) return null;

            return mainType.MakeGenericType(subTypes);
        }

        public static void Register(ObjectKey obj, NoggolloquyTypeRegister reg)
        {
            Registers.Add(obj, reg);
            NameRegisters.Add(reg.FullName, reg);
            TypeRegister.Add(reg.Class, reg);
        }

        public static bool IsNoggType(Type t)
        {
            return TypeRegister.ContainsKey(t);
        }
        
        public static NoggolloquyTypeRegister GetRegister(Type t)
        {
            return TypeRegister[t];
        }

        public static Func<IEnumerable<KeyValuePair<ushort, object>>, T> GetCreateFunc<T>()
        {
            var t = typeof(T);
            if (CreateFuncRegister.TryGetValue(t, out var createFunc))
            {
                return createFunc as Func<IEnumerable<KeyValuePair<ushort, object>>, T>;
            }
            var register = GetRegister(t);
            var methodInfo = t.GetMethod(
                Constants.CREATE_FUNC_NAME,
                Constants.CREATE_FUNC_PARAM_ARRAY);
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
            CreateFuncRegister[t] = del;
            return del as Func<IEnumerable<KeyValuePair<ushort, object>>, T>;
        }

        public static Action<IEnumerable<KeyValuePair<ushort, object>>, T> GetCopyInFunc<T>()
        {
            var t = typeof(T);
            if (CopyInFuncRegister.TryGetValue(t, out var createFunc))
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
                });
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
            CreateFuncRegister[t] = del;
            return del as Action<IEnumerable<KeyValuePair<ushort, object>>, T>;
        }
    }
}
