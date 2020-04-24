using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui.Generation
{
    public class InterfaceCollection
    {
        private HashSet<(LoquiInterfaceDefinitionType Type, string Interface)> interfaces = new HashSet<(LoquiInterfaceDefinitionType Type, string Interface)>();

        public void Add(LoquiInterfaceDefinitionType type, string interfaceStr)
        {
            interfaces.Add((type, interfaceStr));
        }

        public IEnumerable<string> Get(LoquiInterfaceType type)
        {
            foreach (var interf in interfaces)
            {
                switch (interf.Type)
                {
                    case LoquiInterfaceDefinitionType.Direct:
                        if (type == LoquiInterfaceType.Direct)
                        {
                            yield return interf.Interface;
                        }
                        break;
                    case LoquiInterfaceDefinitionType.ISetter:
                        if (type == LoquiInterfaceType.ISetter)
                        {
                            yield return interf.Interface;
                        }
                        break;
                    case LoquiInterfaceDefinitionType.IGetter:
                        if (type == LoquiInterfaceType.IGetter)
                        {
                            yield return interf.Interface;
                        }
                        break;
                    case LoquiInterfaceDefinitionType.Dual:
                        if (type == LoquiInterfaceType.IGetter)
                        {
                            yield return $"{interf.Interface}Getter";
                        }
                        if (type == LoquiInterfaceType.ISetter)
                        {
                            yield return interf.Interface;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Returns whether interface collection contains the given interface string, at any level >= the one given
        /// </summary>
        /// <param name="interfaceStr">String to look for</param>
        /// <param name="type">Interface level the string has to be implemented at least.  Higher is allowed</param>
        /// <returns></returns>
        public bool ContainsAtLeast(string interfaceStr, LoquiInterfaceDefinitionType type)
        {
            return interfaces
                .Where(i => i.Interface == interfaceStr)
                .Where(i => i.Type >= type)
                .Any();
        }
    }
}
