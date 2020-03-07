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
        public APIResult DefaultFallback { get; private set; }

        public CustomMethodAPI(
            bool isPublic,
            APILine api,
            string defaultFallback = null)
        {
            this.Public = isPublic;
            this.API = api;
            this.DefaultFallback = new APIResult(api, defaultFallback);
        }

        public CustomMethodAPI()
        {
        }

        public bool Applies(ObjectGeneration obj, TranslationDirection dir)
        {
            return this.API?.TryResolve(obj, dir, out var line) ?? true;
        }

        public static CustomMethodAPI FactoryPublic(
            APILine api)
        {
            return new CustomMethodAPI()
            {
                Public = true,
                API = api,
                DefaultFallback = null
            };
        }

        public static CustomMethodAPI FactoryPrivate(
            APILine api,
            string defaultFallback)
        {
            return new CustomMethodAPI()
            {
                Public = false,
                API = api,
                DefaultFallback = new APIResult(api, defaultFallback)
            };
        }

        public bool TryGetPassthrough(ObjectGeneration baseGen, ObjectGeneration obj, TranslationDirection dir, out string result)
        {
            var get = this.API.When(obj, dir);
            if (!get)
            {
                result = default;
                return false;
            }
            var name = this.API.GetParameterName(obj);
            if (this.API.When(baseGen, dir))
            {
                result = $"{name}: {name}";
            }
            else
            {
                result = $"{name}: {this.DefaultFallback}";
            }
            return true;
        }
    }
}
