using Noggog;

namespace Loqui.Generation;

public abstract class ContainerType : WrapperType
{
    public override bool IsEnumerable => true;
    public override bool IsClass => true;

    public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
    {
        fg.AppendLine($"{errorMaskAccessor}?.{Name}.Specific.Value.Add({exception});");
    }

    public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
    {
        if (!ReadOnly)
        {
            fg.AppendLine($"{identifier}.Unset();");
        }
        fg.AppendLine("break;");
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

    public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        fg.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (new BraceWrapper(fg))
        {
            if (SubTypeGeneration is LoquiType subLoq
                && subLoq.TargetObjectGeneration != null)
            {
                fg.AppendLine($"if (!{accessor.Access}.{(Nullable ? nameof(ICollectionExt.SequenceEqualNullable) : nameof(ICollectionExt.SequenceEqual))}({rhsAccessor.Access}, (l, r) => {subLoq.TargetObjectGeneration.CommonClassSpeccedInstance("l", LoquiInterfaceType.IGetter, CommonGenerics.Class, subLoq.GenericSpecification)}.Equals(l, r, {maskAccessor}?.GetSubCrystal({IndexEnumInt})))) return false;");
            }
            else
            {
                fg.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
            }
        }
    }

    public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
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
        using (var args = new ArgsWrapper(fg,
                   $"ret.{Name} = item.{Name}.CollectionEqualsHelper"))
        {
            args.Add($"rhs.{Name}");
            args.Add(funcStr);
            args.Add($"include");
        }
    }

    public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
    {
        var maskType = ObjectGen.ProtoGen.Gen.MaskModule.GetMaskModule(GetType()) as ContainerMaskFieldGeneration;
        fg.AppendLine($"{retAccessor} = new {maskType.GetMaskString(this, "bool")}();");
        fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
    }

    public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
    {
        fg.AppendLine($"{hashResultAccessor}.Add({accessor});");
    }
}