using System.Xml.Linq;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class ArrayType : ListType
{
    public override bool CopyNeedsTryCatch => false;
    public override bool IsClass => false;
    public override bool HasDefault => true;
    public override string TypeName(bool getter, bool needsCovariance = false)
    {
        if (getter)
        {
            return $"ReadOnlyMemorySlice<{ItemTypeName(getter)}>";
        }
        else
        {
            return $"{ItemTypeName(getter)}[]";
        }
    }

    public int? FixedSize;

    public override async Task Load(XElement node, bool requireName = true)
    {
        FixedSize = node.GetAttribute(Constants.FIXED_SIZE, default(int?));
        await base.Load(node, requireName);
    }

    protected override string GetActualItemClass(bool ctor = false)
    {
        if (!ctor)
        {
            return $"{ItemTypeName(getter: false)}[]";
        }
        if (Nullable)
        {
            return $"default";
        }
        else if (FixedSize.HasValue)
        {
            if (SubTypeGeneration is LoquiType loqui)
            {
                return $"ArrayExt.Create({FixedSize}, (i) => new {loqui.TargetObjectGeneration.ObjectName}())";
            }
            else if (SubTypeGeneration.IsNullable
                     || SubTypeGeneration.CanBeNullable(getter: false))
            {
                return $"new {ItemTypeName(getter: false)}[{FixedSize}]";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            return $"new {ItemTypeName(getter: false)}[0]";
        }
    }

    public override string ListTypeName(bool getter, bool internalInterface)
    {
        string itemTypeName = ItemTypeName(getter: getter);
        if (SubTypeGeneration is LoquiType loqui)
        {
            itemTypeName = loqui.TypeNameInternal(getter: getter, internalInterface: internalInterface);
        }
        if (getter)
        {
            return $"ReadOnlyMemorySlice<{itemTypeName}{SubTypeGeneration.NullChar}>";
        }
        else
        {
            return $"{itemTypeName}{SubTypeGeneration.NullChar}[]";
        }
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessorPrefix} = null;");
        }
        else
        {
            if (FixedSize.HasValue)
            {
                if (SubTypeGeneration.IsNullable
                    && SubTypeGeneration.Nullable)
                {
                    sb.AppendLine($"{accessorPrefix.Access}.ResetToNull();");
                }
                else if (SubTypeGeneration is StringType)
                {
                    sb.AppendLine($"Array.Fill({accessorPrefix.Access}, string.Empty);");
                }
                else if (SubTypeGeneration is LoquiType loqui)
                {
                    sb.AppendLine($"{accessorPrefix.Access}.Fill(() => new {loqui.TargetObjectGeneration.ObjectName}());");
                }
                else
                {
                    sb.AppendLine($"{accessorPrefix.Access}.Reset();");
                }
            }
            else
            {
                sb.AppendLine($"{accessorPrefix.Access}.Clear();");
            }
        }
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            if (SubTypeGeneration.IsIEquatable)
            {
                if (Nullable)
                {
                    sb.AppendLine($"if (!{nameof(ObjectExt)}.{nameof(ObjectExt.NullSame)}({accessor}, {rhsAccessor})) return false;");
                }
                sb.AppendLine($"if (!MemoryExtensions.SequenceEqual<{SubTypeGeneration.TypeName(getter: true, needsCovariance: true)}>({accessor.Access}{(Nullable ? "!.Value" : null)}.Span!, {rhsAccessor.Access}{(Nullable ? "!.Value" : null)}.Span!)) return false;");
            }
            else
            {
                sb.AppendLine($"if (!{accessor}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor})) return false;");
            }
        }
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        string funcStr;
        var loqui = SubTypeGeneration as LoquiType;
        if (loqui != null)
        {
            funcStr = $"(loqLhs, loqRhs) => loqLhs.{(loqui.TargetObjectGeneration == null ? nameof(IEqualsMask.GetEqualsMask) : "GetEqualsMask")}(loqRhs, include)";
        }
        else
        {
            funcStr = $"(l, r) => {SubTypeGeneration.GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))}";
        }
        using (var args = sb.Call(
                   $"ret.{Name} = {nameof(EqualsMaskHelper)}.SpanEqualsHelper<{SubTypeGeneration.TypeName(getter: true, needsCovariance: true)}{SubTypeGeneration.NullChar}{(loqui == null ? null : $", {loqui.GetMaskString("bool")}")}>"))
        {
            args.Add($"item.{Name}");
            args.Add($"rhs.{Name}");
            args.Add(funcStr);
            args.Add($"include");
        }
    }

    public override void GenerateForCopy(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
    {
        if (FixedSize.HasValue && SubTypeGeneration is not LoquiType)
        {
            if (!AlwaysCopy)
            {
                sb.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
            }
            using (sb.CurlyBrace(doIt: !AlwaysCopy))
            {
                if (Nullable)
                {
                    sb.AppendLine($"{accessor} = {rhs}?.ToArray();");
                }
                else
                {
                    var duplicate = SubTypeGeneration.GetDuplicate("x");
                    if (duplicate == null)
                    {
                        sb.AppendLine($"{rhs}.Span.CopyTo({accessor}.AsSpan());");
                    }
                    else
                    {
                        sb.AppendLine($"{accessor}.SetTo({rhs}.Select(x => {duplicate}));");
                    }
                }
            }
        }
        else
        {
            base.GenerateForCopy(sb, accessor, rhs, copyMaskAccessor, protectedMembers, deepCopy);
        }
    }

    public override void WrapSet(StructuredStringBuilder sb, Accessor accessor, Action<StructuredStringBuilder> a)
    {
        if (FixedSize.HasValue && SubTypeGeneration is not LoquiType)
        {
            sb.AppendLine($"{accessor} = ");
            using (sb.IncreaseDepth())
            {
                a(sb);
                sb.AppendLine($"{NullChar}.ToArray();");
            }
        }
        else
        {
            base.WrapSet(sb, accessor, a);
        }
    }

    public override string GetDefault(bool getter)
    {
        if (getter && Nullable) return $"default({TypeName(getter)}{NullChar})";
        return base.GetDefault(getter);
    }
}