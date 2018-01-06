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
        public string API { get; private set; }
        public string DefaultFallback { get; private set; }

        public static CustomMethodAPI Private(
            string api,
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
