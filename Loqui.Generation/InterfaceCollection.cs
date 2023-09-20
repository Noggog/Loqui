using System.Collections;

namespace Loqui.Generation;

public class InterfaceDeclaration : IEquatable<InterfaceDeclaration>
{
    public LoquiInterfaceDefinitionType Type { get; }
    public string Interface { get; private set; }
    public ObjectGeneration AssociatedObject;
    public string GetterInterface
    {
        get
        {
            switch (Type)
            {
                case LoquiInterfaceDefinitionType.IGetter:
                    return Interface;
                default:
                    return $"{Interface}Getter";
            }
        }
    }

    public InterfaceDeclaration(LoquiInterfaceDefinitionType type, string interfaceStr)
    {
        Type = type;
        Interface = interfaceStr;
    }

    public override bool Equals(object obj)
    {
        return obj is InterfaceDeclaration d && Equals(d);
    }

    public bool Equals(InterfaceDeclaration other)
    {
        if (Type != other.Type) return false;
        return string.Equals(Interface, other.Interface);
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
        foreach (var obj in parent.ProtoGen.ObjectGenerationsByName.Values)
        {
            await obj.LoadingCompleteTask.Task;
            if (obj.IsObjectInterface(Interface))
            {
                AssociatedObject = obj;
                return;
            }
            if (Type == LoquiInterfaceDefinitionType.Dual)
            {
                if (obj.IsObjectInterface(GetterInterface))
                {
                    AssociatedObject = obj;
                    return;
                }
            }
        }
    }
}

public class InterfaceCollection : IEnumerable<InterfaceDeclaration>
{
    private readonly HashSet<InterfaceDeclaration> _interfaces = new();

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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}