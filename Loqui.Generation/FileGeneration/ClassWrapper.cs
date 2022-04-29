using Noggog;

namespace Loqui.Generation;

public class ClassWrapper : IDisposable
{
    public enum ObjectType
    {
        @class,
        @struct,
        @interface
    }

    private StructuredStringBuilder sb;
    public string Name { get; }
    public PermissionLevel Public = PermissionLevel.@public;
    public bool Partial;
    public bool Abstract;
    public bool Static;
    public string BaseClass;
    public bool New;
    public ObjectType Type = ObjectType.@class;
    public HashSet<string> Interfaces = new();
    public List<string> Wheres = new();
    public List<string> Attributes = new();

    public ClassWrapper(StructuredStringBuilder sb, string name)
    {
        this.sb = sb;
        Name = name;
    }

    public void Dispose()
    {
        foreach (var attr in Attributes)
        {
            sb.AppendLine(attr);
        }
        var classLine = $"{EnumExt.ToStringFast_Enum_Only<PermissionLevel>(Public)} {(Static ? "static " : null)}{(New ? "new " : null)}{(Abstract ? "abstract " : null)}{(Partial ? "partial " : null)}{EnumExt.ToStringFast_Enum_Only<ObjectType>(Type)} {Name}";
        var toAdd = Interfaces.OrderBy(x => x).ToList();
        if (!string.IsNullOrWhiteSpace(BaseClass))
        {
            toAdd.Insert(0, BaseClass);
        }
        if (toAdd.Count > 1)
        {
            sb.AppendLine($"{classLine} :");
            sb.Depth++;
            toAdd.Last(
                each: (item, last) =>
                {
                    sb.AppendLine($"{item}{(last ? string.Empty : ",")}");
                });
            sb.Depth--;
        }
        else if (toAdd.Count == 1)
        {
            sb.AppendLine($"{classLine} : {toAdd.First()}");
        }
        else
        {
            sb.AppendLine(classLine);
        }
        if (Wheres.Count > 0)
        {
            using (new DepthWrapper(sb))
            {
                foreach (var where in Wheres)
                {
                    sb.AppendLine(where);
                }
            }
        }
    }
}