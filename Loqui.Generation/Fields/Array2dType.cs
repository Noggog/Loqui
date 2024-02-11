using System.Xml.Linq;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class Array2dType : ListType
{
    public override bool HasDefault => false;
    public override bool CopyNeedsTryCatch => true;

    public P2Int? FixedSize;
    private P2Int32Type PointTypeGen = new();

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        if (FixedSize != null)
        {
            sb.AppendLine($"public static readonly {nameof(P2Int)} {Name}FixedSize = new {nameof(P2Int)}({FixedSize.Value.X}, {FixedSize.Value.Y});");
        }
        await base.GenerateForClass(sb);
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        var width = node.GetAttribute(Constants.FIXED_WIDTH, default(int?));
        var height = node.GetAttribute(Constants.FIXED_HEIGHT, default(int?));
        if (width == null || height == null)
        {
            throw new ArgumentException("Must supply fixedWidth and fixedHeight");
        }

        FixedSize = new P2Int(width.Value, height.Value);
        await base.Load(node, requireName);
    }
    
    public override string ListTypeName(bool getter, bool internalInterface)
    {
        string itemTypeName = ItemTypeName(getter: getter);
        if (SubTypeGeneration is LoquiType loqui)
        {
            itemTypeName = loqui.TypeNameInternal(getter: getter, internalInterface: internalInterface);
        }
        if (ReadOnly || getter)
        {
            return $"IReadOnlyArray2d<{itemTypeName}{SubTypeGeneration.NullChar}>";
        }
        else
        {
            return $"IArray2d<{itemTypeName}{SubTypeGeneration.NullChar}>";
        }
    }
    protected override string GetActualItemClass(bool ctor = false)
    {
        return $"new Array2d<{ItemTypeName(getter: false)}>{(ctor ? $"({FixedSize.Value.X}, {FixedSize.Value.Y}, {SubTypeGeneration.GetDefault(getter: false)})" : null)}";
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessor)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessor} = null;");
        }
        else
        {
            sb.AppendLine($"{accessor}.SetAllTo({SubTypeGeneration.GetDefault(getter: false)});");
        }
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            if (SubTypeGeneration is LoquiType subLoq
                && subLoq.TargetObjectGeneration != null)
            {
                sb.AppendLine($"if (!{accessor.Access}.{(Nullable ? nameof(ICollectionExt.SequenceEqualNullable) : nameof(ICollectionExt.SequenceEqual))}({rhsAccessor.Access}, (l, r) => {subLoq.TargetObjectGeneration.CommonClassSpeccedInstance("l.Value", LoquiInterfaceType.IGetter, CommonGenerics.Class, subLoq.GenericSpecification)}.Equals(l.Value, r.Value, {maskAccessor}?.GetSubCrystal({IndexEnumInt})))) return false;");
            }
            else
            {
                sb.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
            }
        }
    }

    protected override void TypicalSetTo(StructuredStringBuilder sb)
    {
        sb.AppendLine($"rhs.{Name}");
        var ret = SubTypeGeneration.ReturnForCopySetToConverter("b.Value");
        if (ret != null)
        {
            using (sb.IncreaseDepth())
            {
                sb.AppendLine($".Select(b => new KeyValuePair<P2Int, {SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>(b.Key, {ret}))"); 
            }
            sb.AppendLine($", {SubTypeGeneration.GetDefault(getter: false)}");
        }
    }

    protected override void GenerateLoquiDeepCopy(
        Accessor rhs, Accessor copyMaskAccessor, bool deepCopy,
        StructuredStringBuilder sb,
        LoquiType loqui)
    {
        sb.AppendLine(rhs.ToString());
        sb.AppendLine(".Select(r =>");
        using (new CurlyBrace(sb) { AppendParenthesis = true })
        {
            loqui.GenerateTypicalMakeCopy(
                sb,
                retAccessor: $"var item = ",
                rhsAccessor: Accessor.FromType(loqui, "r.Value"),
                copyMaskAccessor: copyMaskAccessor,
                deepCopy: deepCopy,
                doTranslationMask: false);
            sb.AppendLine($"return new KeyValuePair<P2Int, {SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>(r.Key, item);");
        }
        sb.AppendLine($", () => {SubTypeGeneration.GetDefault(getter: false)}");
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
        using (var args = sb.Call(
                   $"ret.{Name} = item.{Name}.Array2dEqualsHelper"))
        {
            args.Add($"rhs.{Name}");
            args.Add(funcStr);
            args.Add($"include");
        }
    }

    public override void WrapSet(StructuredStringBuilder sb, Accessor accessor, Action<StructuredStringBuilder> a)
    {
        if (Nullable)
        {
            sb.AppendLine($"{accessor} = ");
            using (sb.IncreaseDepth())
            {
                a(sb);
                sb.AppendLine($".ShallowClone();");
            }
        }
        else
        {
            using (var args = sb.Call(
                       $"{accessor}.SetTo"))
            {
                args.Add(subFg => a(subFg));
            }
        }
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        sb.AppendLine($"{sbAccessor}.{nameof(StructuredStringBuilder.AppendLine)}(\"{name} =>\");");
        sb.AppendLine($"using ({sbAccessor}.Brace())");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"foreach (var subItem in {accessor.Access})");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"using ({sbAccessor}.Brace())");
                using (sb.CurlyBrace())
                {
                    PointTypeGen.GenerateToString(sb, "Index", "subItem.Key", sbAccessor);
                    SubTypeGeneration.GenerateToString(sb, "Value", new Accessor("subItem.Value"), sbAccessor);
                }
            }
        }
    }
}