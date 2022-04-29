using System.Xml.Linq;

namespace Loqui.Generation;

public interface IDictType
{
    TypeGeneration KeyTypeGen { get; }
    TypeGeneration ValueTypeGen { get; }
    DictMode Mode { get; }
    void AddMaskException(StructuredStringBuilder sb, string errorMaskMemberAccessor, string exception, bool key);
}

public enum DictMode
{
    KeyValue,
    KeyedValue
}

public class DictType : TypeGeneration, IDictType
{
    private TypeGeneration subGenerator;
    private IDictType subDictGenerator;
    public DictMode Mode => subDictGenerator.Mode;
    public override bool CopyNeedsTryCatch => subGenerator.CopyNeedsTryCatch;
    public TypeGeneration KeyTypeGen => subDictGenerator.KeyTypeGen;
    public TypeGeneration ValueTypeGen => subDictGenerator.ValueTypeGen;
    public override string ProtectedName => subGenerator.ProtectedName;
    public override string TypeName(bool getter, bool needsCovariance = false) => subGenerator.TypeName(getter, needsCovariance);
    public override bool IsEnumerable => true;
    public override bool IsClass => true;
    public override bool HasDefault => false;

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy) => subGenerator.SkipCheck(copyMaskAccessor, deepCopy);

    public override string GetName(bool internalUse)
    {
        return subGenerator.GetName(internalUse);
    }

    public void AddMaskException(StructuredStringBuilder sb, string errorMaskMemberAccessor, string exception, bool key)
    {
        subDictGenerator.AddMaskException(sb, errorMaskMemberAccessor, exception, key);
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);

        var keyedValueNode = node.Element(XName.Get("KeyedValue", LoquiGenerator.Namespace));
        if (keyedValueNode != null)
        {
            var dictType = new DictType_KeyedValue();
            dictType.SetObjectGeneration(ObjectGen, setDefaults: false);
            subGenerator = dictType;
            await subGenerator.Load(node, requireName);
            subDictGenerator = dictType;
        }
        else
        {
            var dictType = new DictType_Typical();
            dictType.SetObjectGeneration(ObjectGen, setDefaults: false);
            subGenerator = dictType;
            await subGenerator.Load(node, requireName);
            subDictGenerator = dictType;
        }
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        subGenerator.GenerateUnsetNth(sb, identifier);
    }

    public override Task GenerateForClass(StructuredStringBuilder sb)
    {
        return subGenerator.GenerateForClass(sb);
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        subGenerator.GenerateForInterface(sb, getter, internalInterface);
    }

    public override void GenerateForCopy(
        StructuredStringBuilder sb,
        Accessor accessor,
        Accessor rhs, 
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy)
    {
        subGenerator.GenerateForCopy(sb, accessor, rhs, copyMaskAccessor, protectedMembers, deepCopy);
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        subGenerator.GenerateSetNth(sb, accessor, rhs, internalUse);
    }

    public override string NullableAccessor(bool getter, Accessor accessor = null)
    {
        return subGenerator.NullableAccessor(getter, accessor);
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        subGenerator.GenerateGetNth(sb, identifier);
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        subGenerator.GenerateClear(sb, accessorPrefix);
    }

    public override string GenerateACopy(string rhsAccessor)
    {
        return subGenerator.GenerateACopy(rhsAccessor);
    }

    public override async Task Resolve()
    {
        await subGenerator.Resolve();
    }

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
    {
        return subGenerator.GenerateEqualsSnippet(accessor, rhsAccessor, negate);
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        subGenerator.GenerateForEquals(sb, accessor, rhsAccessor, maskAccessor);
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        subGenerator.GenerateForEqualsMask(sb, accessor, rhsAccessor, retAccessor);
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        subGenerator.GenerateForHash(sb, accessor, hashResultAccessor);
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        subGenerator.GenerateToString(sb, name, accessor, sbAccessor);
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        subGenerator.GenerateForNullableCheck(sb, accessor, checkMaskAccessor);
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}