namespace Loqui.Generation;

public class Accessor
{
    public string Access;
    public bool IsAssignment = true;

    public Accessor()
    {
    }

    public Accessor(string direct)
    {
        Access = direct;
    }

    public string Assign(string rhs)
    {
        return $"{Access}{AssignmentOperator}{rhs}";
    }

    public string AssignmentOperator => IsAssignment ? " = " : ": ";

    public static Accessor ConstructorParam(string path)
    {
        return new Accessor(path)
        {
            IsAssignment = false,
        };
    }

    public static Accessor FromType(
        TypeGeneration typeGen,
        string accessor,
        bool protectedAccess = false,
        bool nullable = false)
    {
        string process(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return accessor;
            return $"{accessor}{(nullable ? "?" : null)}.{name}";
        }
        Accessor ret = new Accessor();
        ret.Access = process(protectedAccess ? typeGen.ProtectedName : typeGen.Name);
        return ret;
    }

    public override string ToString()
    {
        return Access;
    }

    public static implicit operator Accessor(string str)
    {
        if (str == null) return null;
        return new Accessor(str);
    }
}