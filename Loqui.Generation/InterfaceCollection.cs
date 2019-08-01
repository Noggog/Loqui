using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui.Generation
{
    public class InterfaceCollection
    {
        private HashSet<(LoquiInterfaceType Type, string Interface)> interfaces = new HashSet<(LoquiInterfaceType Type, string Interface)>();

        public void Add(LoquiInterfaceType type, string interfaceStr)
        {
            interfaces.Add((type, interfaceStr));
        }

        public IEnumerable<string> Get(LoquiInterfaceType type) => interfaces.Where(i => i.Type == type).Select(i => i.Interface);

        /// <summary>
        /// Returns whether interface collection contains the given interface string, at any level >= the one given
        /// </summary>
        /// <param name="interfaceStr">String to look for</param>
        /// <param name="type">Interface level the string has to be implemented at least.  Higher is allowed</param>
        /// <returns></returns>
        public bool ContainsAtLeast(string interfaceStr, LoquiInterfaceType type)
        {
            return interfaces
                .Where(i => i.Interface == interfaceStr)
                .Where(i => i.Type >= type)
                .Any();
        }
    }
}
