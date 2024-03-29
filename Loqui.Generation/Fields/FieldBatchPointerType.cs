using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class FieldBatchPointerType : TypeGeneration
{
    public string BatchName { get; private set; }
    public string ProtocolID { get; private set; }
    public override bool IsEnumerable => throw new ArgumentException();
    public override bool IsClass => throw new ArgumentException();
    public override bool HasDefault => throw new ArgumentException();

    #region Type Generation Abstract
    public override string TypeName(bool getter, bool needsCovariance = false) => throw new NotImplementedException();

    public override string ProtectedName => throw new NotImplementedException();

    public override bool CopyNeedsTryCatch => throw new NotImplementedException();

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

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        throw new NotImplementedException();
    }

    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        throw new NotImplementedException();
    }
    #endregion

    public override async Task Load(XElement node, bool requireName = true)
    {
        _derivative = false;
        BatchName = node.GetAttribute<string>("name", throwException: true);
        ProtocolID = node.GetAttribute("protocol", throwException: false);
    }

    public override async Task Resolve()
    {
        await base.Resolve();
        var protoID = ProtocolID ?? ObjectGen.ProtoGen.Protocol.Namespace;
        if (!ObjectGen.ProtoGen.Gen.TryGetProtocol(new ProtocolKey(protoID), out var protoGen))
        {
            throw new ArgumentException($"Protocol did not exist {protoID}.");
        }
        if (!protoGen.FieldBatchesByName.TryGetValue(BatchName, out var batch))
        {
            throw new ArgumentException($"Field batch did not exist {BatchName} in protocol {protoGen.Protocol.Namespace}");
        }
        var index = ObjectGen.IterateFields().ToList().IndexOf(this);
        if (index == -1)
        {
            throw new ArgumentException("Could not find self in object's field list.");
        }
        foreach (var generic in batch.Generics)
        {
            ObjectGen.Generics[generic.Key] = generic.Value;
        }
        foreach (var field in batch.Fields)
        {
            var typeGen = await ObjectGen.LoadField(field.Node, requireName: true);
            if (typeGen.Succeeded)
            {
                ObjectGen.Fields.Insert(index++, typeGen.Value);
                await typeGen.Value.Resolve();
            }
        }
        if (!ObjectGen.Fields.Remove(this))
        {
            throw new ArgumentException("Could not remove self from object's field list.");
        }
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        throw new NotImplementedException();
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }
}