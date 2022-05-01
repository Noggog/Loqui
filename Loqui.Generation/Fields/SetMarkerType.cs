using System.Xml.Linq;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class SetMarkerType : TypeGeneration
{
    public enum ExpandSets { False, FalseAndInclude, True, TrueAndInclude }

    public override string TypeName(bool getter, bool needsCovariance = false) => nameof(SetMarkerType);
    public override string ProtectedName => throw new ArgumentException();
    public override bool CopyNeedsTryCatch => throw new ArgumentException();
    public List<TypeGeneration> SubFields = new List<TypeGeneration>();
    public override bool IntegrateField => false;
    public override bool IsEnumerable => throw new ArgumentException();
    public override bool IsClass => throw new ArgumentException();
    public override bool HasDefault => throw new ArgumentException();

    public IEnumerable<(int Index, TypeGeneration Field)> IterateFields(
        bool nonIntegrated = false,
        ExpandSets expandSets = ExpandSets.True)
    {
        int i = 0;
        foreach (var field in SubFields)
        {
            if ((!field.IntegrateField && !nonIntegrated) || !field.Enabled) continue;
            yield return (i++, field);
        }
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        var fieldsNode = node.Element(XName.Get(Constants.FIELDS, LoquiGenerator.Namespace));
        if (fieldsNode != null)
        {
            foreach (var fieldNode in fieldsNode.Elements())
            {
                var typeGen = await ObjectGen.LoadField(fieldNode, requireName: true);
                if (typeGen.Succeeded)
                {
                    SubFields.Add(typeGen.Value);
                }
            }
        }
    }

    #region Abstract
    public override string GenerateACopy(string rhsAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        throw new NotImplementedException();
    }

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForCopy(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateGetNameIndex(StructuredStringBuilder sb)
    {
        throw new NotImplementedException();
    }

    public override void GenerateGetNthName(StructuredStringBuilder sb)
    {
        throw new NotImplementedException();
    }

    public override void GenerateGetNthType(StructuredStringBuilder sb)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        throw new NotImplementedException();
    }

    public override void GenerateGetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        throw new NotImplementedException();
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        throw new NotImplementedException();
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        throw new NotImplementedException();
    }

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        throw new NotImplementedException();
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
    #endregion
}