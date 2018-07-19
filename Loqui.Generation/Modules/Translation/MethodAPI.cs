using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class MethodAPI
    {
        public APILine[] MajorAPI { get; private set; }
        public CustomMethodAPI[] CustomAPI { get; private set; }
        public APILine[] OptionalAPI { get; private set; }

        public MethodAPI(
            APILine[] majorAPI,
            CustomMethodAPI[] customAPI,
            APILine[] optionalAPI)
        {
            this.MajorAPI = majorAPI ?? new APILine[] { };
            this.CustomAPI = customAPI ?? new CustomMethodAPI[] { };
            this.OptionalAPI = optionalAPI ?? new APILine[] { };
        }

        public MethodAPI(
            params APILine[] api)
        {
            this.MajorAPI = api;
            this.CustomAPI = new CustomMethodAPI[] { };
            this.OptionalAPI = new APILine[] { };
        }
        
        public IEnumerable<(string API, bool Public)> IterateAPI(ObjectGeneration obj, params string[] customLines)
        {
            foreach (var item in this.MajorAPI)
            {
                if (item.TryResolve(obj, out var line))
                {
                    yield return (line, true);
                }
            }
            foreach (var item in this.CustomAPI)
            {
                if (item.API.TryResolve(obj, out var line))
                {
                    yield return (line, item.Public);
                }
            }
            foreach (var item in customLines)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                yield return (item, true);
            }
            foreach (var item in this.OptionalAPI)
            {
                if (item.TryResolve(obj, out var line))
                {
                    yield return (line, true);
                }
            }
        }
    }
}
