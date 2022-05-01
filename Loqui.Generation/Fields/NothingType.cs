using System.Xml.Linq;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class NothingType : TypeGeneration
{
    public override bool IntegrateField => false;
    public override string TypeName(bool getter, bool needsCovariance = false) => null;
    public override string ProtectedName => null;
    public override bool CopyNeedsTryCatch => false;
    public override bool IsEnumerable => false;
    public override bool Namable => false;
    public override bool IsClass => throw new ArgumentException();
    public override bool HasDefault => throw new ArgumentException();

    public override string GenerateACopy(string rhsAccessor)
    {
        return null;
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        NullableProperty.OnNext((false, true));
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
    }

    public override void GenerateForCopy(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
    {
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
    }

    public override string ToString()
    {
        return "Nothing";
    }

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        return null;
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}