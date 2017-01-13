using System;

namespace Noggolloquy
{
    public static class WildcardLink
    {
        public static object Validate(object o)
        {
            if (!WildcardXmlTranslation.Instance.Validate(o.GetType()))
            {
                throw new ArgumentException($"Type not supported by wildcard systems {o.GetType()}");
            }
            return o;
        }
    }
}
