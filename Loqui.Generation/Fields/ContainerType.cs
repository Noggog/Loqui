using Noggog;

namespace Loqui.Generation;

public abstract class ContainerType : WrapperType
{
    public override bool IsEnumerable => true;
    public override bool IsClass => true;

    public void AddMaskException(StructuredStringBuilder sb, string errorMaskAccessor, string exception)
    {
        sb.AppendLine($"{errorMaskAccessor}?.{Name}.Specific.Value.Add({exception});");
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        if (!ReadOnly)
        {
            sb.AppendLine($"{identifier}.Unset();");
        }
        sb.AppendLine("break;");
    }

    public override string GetName(bool internalUse)
    {
        if (internalUse)
        {
            return $"_{Name}";
        }
        else
        {
            return Name;
        }
    }

    public override string GenerateACopy(string rhsAccessor)
    {
        throw new NotImplementedException();
    }

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
    {
        return $"{(negate ? "!" : null)}{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})";
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            if (SubTypeGeneration is LoquiType subLoq
                && subLoq.TargetObjectGeneration != null)
            {
                sb.AppendLine($"if (!{accessor.Access}.{(Nullable ? nameof(ICollectionExt.SequenceEqualNullable) : nameof(ICollectionExt.SequenceEqual))}({rhsAccessor.Access}, (l, r) => {subLoq.TargetObjectGeneration.CommonClassSpeccedInstance("l", LoquiInterfaceType.IGetter, CommonGenerics.Class, subLoq.GenericSpecification)}.Equals(l, r, {maskAccessor}?.GetSubCrystal({IndexEnumInt})))) return false;");
            }
            else
            {
                sb.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
            }
        }
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        string funcStr;
        if (SubTypeGeneration is LoquiType loqui)
        {
            funcStr = $"(loqLhs, loqRhs) => {(loqui.TargetObjectGeneration == null ? "(IMask<bool>)" : null)}loqLhs.{(loqui.TargetObjectGeneration == null ? nameof(IEqualsMask.GetEqualsMask) : "GetEqualsMask")}(loqRhs, include)";
        }
        else
        {
            funcStr = $"(l, r) => {SubTypeGeneration.GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))}";
        }
        using (var args = new ArgsWrapper(sb,
                   $"ret.{Name} = item.{Name}.CollectionEqualsHelper"))
        {
            args.Add($"rhs.{Name}");
            args.Add(funcStr);
            args.Add($"include");
        }
    }

    public void GenerateForEqualsMask(StructuredStringBuilder sb, string retAccessor, bool on)
    {
        var maskType = ObjectGen.ProtoGen.Gen.MaskModule.GetMaskModule(GetType()) as ContainerMaskFieldGeneration;
        sb.AppendLine($"{retAccessor} = new {maskType.GetMaskString(this, "bool")}();");
        sb.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
    }

    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        sb.AppendLine($"{hashResultAccessor}.Add({accessor});");
    }
}