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
        public string[] MajorAPI { get; private set; }
        public CustomMethodAPI[] CustomAPI { get; private set; }
        public string[] OptionalAPI { get; private set; }

        public MethodAPI(
            string[] majorAPI,
            CustomMethodAPI[] customAPI,
            string[] optionalAPI)
        {
            this.MajorAPI = majorAPI ?? new string[] { };
            this.CustomAPI = customAPI ?? new CustomMethodAPI[] { };
            this.OptionalAPI = optionalAPI ?? new string[] { };
        }

        public MethodAPI(
            params string[] api)
        {
            this.MajorAPI = api;
            this.CustomAPI = new CustomMethodAPI[] { };
            this.OptionalAPI = new string[] { };
        }
        
        public IEnumerable<(string API, bool Public)> IterateAPI()
        {
            foreach (var item in this.MajorAPI)
            {
                yield return (item, true);
            }
            foreach (var item in this.CustomAPI)
            {
                yield return (item.API, item.Public);
            }
            foreach (var item in this.OptionalAPI)
            {
                yield return (item, true);
            }
        }
    }
}
