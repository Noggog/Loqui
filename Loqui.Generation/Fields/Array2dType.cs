using System.Xml.Linq;
using Noggog;

namespace Loqui.Generation;

public class Array2dType : ListType
{
    public override bool HasDefault => false;
    public override bool CopyNeedsTryCatch => true;

    public P2Int? FixedSize;

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
        return $"new Array2d<{ItemTypeName(getter: false)}>{(ctor ? $"({FixedSize.Value.X}, {FixedSize.Value.Y})" : null)}";
    }

    public override void GenerateClear(FileGeneration fg, Accessor accessor)
    {
        if (Nullable)
        {
            fg.AppendLine($"{accessor} = null;");
        }
        else
        {
            fg.AppendLine($"{accessor}.SetAllTo(default);");
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
                   $"ret.{Name} = item.{Name}.Array2dEqualsHelper"))
        {
            args.Add($"rhs.{Name}");
            args.Add(funcStr);
            args.Add($"include");
        }
    }

    public override void WrapSet(FileGeneration fg, Accessor accessor, Action<FileGeneration> a)
    {
        if (Nullable)
        {
            fg.AppendLine($"{accessor} = ");
            using (new DepthWrapper(fg))
            {
                a(fg);
                fg.AppendLine($".ShallowClone();");
            }
        }
        else
        {
            using (var args = new ArgsWrapper(fg,
                       $"{accessor}.SetTo"))
            {
                args.Add(subFg => a(subFg));
            }
        }
    }
}