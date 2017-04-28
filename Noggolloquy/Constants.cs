using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Internal
{
    public static class Constants
    {
        public const string CREATE_FUNC_NAME = "Create";
        public readonly static Type CREATE_FUNC_PARAM = typeof(IEnumerable<KeyValuePair<ushort, object>>);
        public readonly static Type[] CREATE_FUNC_PARAM_ARRAY = new Type[]
        {
            CREATE_FUNC_PARAM
        };
        public const BindingFlags CREATE_FUNC_FLAGS =
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.FlattenHierarchy;

        public const string COPYIN_FUNC_NAME = "CopyIn";
        
        public const string COPY_FUNC_NAME = "Copy";
    }
}
