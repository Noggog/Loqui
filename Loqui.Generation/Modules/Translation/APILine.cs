using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class APILine
    {
        public Func<ObjectGeneration, TryGet<string>> Resolver;

        public APILine(Func<ObjectGeneration, TryGet<string>> resolver)
        {
            this.Resolver = resolver;
        }

        public bool TryResolve(ObjectGeneration obj, out string line)
        {
            var ret = Resolver(obj);
            line = ret.Value;
            return ret.Succeeded;
        }

        public static implicit operator APILine(string str)
        {
            return new APILine((o) => TryGet<string>.Succeed(str));
        }
    }
}
