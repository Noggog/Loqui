using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class CustomMethodAPI
    {
        public bool Public { get; private set; }
        public APILine API { get; private set; }
        public string DefaultFallback { get; private set; }

        public CustomMethodAPI(
            bool isPublic,
            APILine api,
            string defaultFallback = null)
        {
            this.Public = isPublic;
            this.API = api;
            this.DefaultFallback = defaultFallback;
        }

        public CustomMethodAPI()
        {
        }

        public bool Applies(ObjectGeneration obj)
        {
            return this.API?.TryResolve(obj, out var line) ?? true;
        }

        public static CustomMethodAPI Private(
            APILine api,
            string defaultFallback)
        {
            return new CustomMethodAPI()
            {
                Public = false,
                API = api,
                DefaultFallback = defaultFallback
            };
        }
    }
}
