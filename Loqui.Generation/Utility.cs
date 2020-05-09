using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public static class Utility
    {
        public static int? TimeoutMS = null;

        public static string MemberNameSafety(string str)
        {
            var ret = str.Replace(".", string.Empty);
            ret = ret.Replace("[]", "Arr");
            return ret;
        }

        public static string TaskWrap(string str, bool task = true)
        {
            if (!task) return str;
            return $"async Task<{str}>";
        }

        public static string TaskReturn(bool task = true)
        {
            if (!task) return "void";
            return $"async Task";
        }

        public static string Async(bool doIt = true)
        {
            return doIt ? "async " : null;
        }

        public static string Await(bool doIt = true)
        {
            return doIt ? "await " : null;
        }

        public static string Await(AsyncMode mode)
        {
            switch (mode)
            {
                case AsyncMode.Off:
                case AsyncMode.Direct:
                    return Await(false);
                case AsyncMode.Async:
                    return Await(true);
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ConfigAwait(bool doIt = true)
        {
            return doIt ? ".ConfigureAwait(false)" : null;
        }

        public static string ConfigAwait(AsyncMode mode)
        {
            switch (mode)
            {
                case AsyncMode.Off:
                case AsyncMode.Direct:
                    return ConfigAwait(false);
                case AsyncMode.Async:
                    return ConfigAwait(true);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
