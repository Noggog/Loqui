using System.Globalization;
using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class EnumType : PrimitiveType
{
    public string EnumName;
    public bool Nullable;

    public override string TypeName(bool getter, bool needsCovariance = false) => $"{EnumName}{(Nullable ? "?" : string.Empty)}";
    public string NoNullTypeName => $"{EnumName}";
    public override Type Type(bool getter) => throw new NotImplementedException();
    public override bool IsIEquatable => false;
    public bool IsGeneric => ObjectGen.Generics.Keys.Contains(EnumName);

    public EnumType()
    {
    }

    public EnumType(bool nullable)
    {
        Nullable = nullable;
    }

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        if (!IsGeneric)
        {
            return base.GenerateEqualsSnippet(accessor, rhsAccessor, negate);
        }
        return $"{(negate ? "!" : null)}EqualityComparer<{EnumName}>.Default.Equals({accessor}, {rhsAccessor})";
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        if (node.TryGetAttribute<string>(
                Constants.ENUM_NAME,
                out var item,
                throwException: true,
                CultureInfo.InvariantCulture))
        {
            EnumName = item;
        }
        else
        {
            throw new KeyNotFoundException($"{Constants.ENUM_NAME} on {Name} was not specified");
        }
    }
}