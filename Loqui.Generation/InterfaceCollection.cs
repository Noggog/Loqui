using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class InterfaceDeclaration : IEquatable<InterfaceDeclaration>
    {
        public LoquiInterfaceDefinitionType Type { get; private set; }
        public string Interface { get; private set; }
        public ObjectGeneration AssociatedObject;
        public string GetterInterface => $"{Interface}Getter";

        public InterfaceDeclaration(LoquiInterfaceDefinitionType type, string interfaceStr)
        {
            this.Type = type;
            this.Interface = interfaceStr;
        }

        public override bool Equals(object obj)
        {
            return obj is InterfaceDeclaration d && Equals(d);
        }

        public bool Equals(InterfaceDeclaration other)
        {
            if (this.Type != other.Type) return false;
            return string.Equals(this.Interface, other.Interface);
        }

        public override int GetHashCode()
        {
            var ret = new HashCode();
            ret.Add(Type);
            ret.Add(Interface);
            return ret.ToHashCode();
        }

        public async Task Resolve(ObjectGeneration parent)
        {
            foreach (var obj in parent.ProtoGen.Gen.ObjectGenerations)
            {
                await obj.LoadingCompleteTask.Task;
                if (obj.IsObjectInterface(Interface))
                {
                    this.AssociatedObject = obj;
                    return;
                }
                if (Type == LoquiInterfaceDefinitionType.Dual)
                {
                    if (obj.IsObjectInterface(GetterInterface))
                    {
                        this.AssociatedObject = obj;
                        return;
                    }
                }
            }
        }
    }

    public class InterfaceCollection : IEnumerable<InterfaceDeclaration>
    {
        private readonly HashSet<InterfaceDeclaration> _interfaces = new HashSet<InterfaceDeclaration>();

        public void Add(LoquiInterfaceDefinitionType type, string interfaceStr)
        {
            _interfaces.Add(new InterfaceDeclaration(type, interfaceStr));
        }

        public IEnumerable<string> Get(LoquiInterfaceType type)
        {
            foreach (var interf in _interfaces)
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
                            yield return interf.GetterInterface;
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
            return _interfaces
                .Where(i => i.Interface == interfaceStr)
                .Where(i => i.Type >= type)
                .Any();
        }

        public IEnumerator<InterfaceDeclaration> GetEnumerator()
        {
            return _interfaces.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
