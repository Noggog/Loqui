using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class ByteArrayType : ClassType
{
    public int? Length;
    public override Type Type(bool getter) => getter ? typeof(ReadOnlyMemorySlice<byte>) : typeof(MemorySlice<byte>);
    public override bool IsEnumerable => true;
    public override bool IsReference => true;

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
    {
        if (Nullable)
        {
            return $"{(negate ? "!" : null)}{nameof(MemorySliceExt)}.SequenceEqual({accessor.Access}, {rhsAccessor.Access})";
        }
        else
        {
            return $"{(negate ? "!" : null)}MemoryExtensions.SequenceEqual({accessor.Access}.Span, {rhsAccessor.Access}.Span)";
        }
    }

    public override string GetNewForNonNullable()
    {
        return $"new byte[{Length ?? 0}]";
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        if (!IntegrateField) return;
        // ToDo
        // Add Internal interface support
        if (InternalGetInterface) return;
        sb.AppendLine($"{sbAccessor}.AppendLine($\"{name} => {{{nameof(SpanExt)}.{nameof(SpanExt.ToHexString)}({accessor.Access})}}\");");
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        Length = node.GetAttribute<int?>("byteLength", null);

        if (Length != null
            && Nullable)
        {
            throw new ArgumentException($"Cannot have a byte array with a length that is nullable.  Doesn't apply. {ObjectGen.Name} {Name}");
        }
    }
        
    public override void GenerateForHash(StructuredStringBuilder sb, Accessor accessor, string hashResultAccessor)
    {
        if (!IntegrateField) return;
        if (Nullable)
        {
            sb.AppendLine($"if ({accessor} is {{}} {Name}Item)");
            accessor = $"{Name}Item";
        }
        using (sb.CurlyBrace(doIt: Nullable))
        {
            sb.AppendLine($"{hashResultAccessor}.Add({accessor});");
        }
    }

    public override void GenerateForCopy(
        StructuredStringBuilder sb,
        Accessor accessor,
        Accessor rhs, 
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            sb.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (sb.CurlyBrace(doIt: !AlwaysCopy))
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                sb,
                () =>
                {
                    if (Nullable)
                    {
                        sb.AppendLine($"if(rhs.{Name} is {{}} {Name}rhs)");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"{accessor.Access} = {Name}rhs{(deepCopy ? null : ".Value")}.ToArray();");
                        }
                        sb.AppendLine("else");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"{accessor.Access} = default;");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{accessor.Access} = {rhs}.ToArray();");
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor identifier)
    {
        if (ReadOnly || !IntegrateField) return;
        // ToDo
        // Add internal interface support
        if (InternalSetInterface) return;
        if (!Enabled) return;
        if (Nullable)
        {
            sb.AppendLine($"{identifier.Access} = default;");
        }
        else if (Length.HasValue)
        {
            sb.AppendLine($"{identifier.Access} = new byte[{Length.Value}];");
        }
        else
        {
            sb.AppendLine($"{identifier.Access} = new byte[0];");
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        throw new NotImplementedException();
    }

    public override void GenerateCopySetToConverter(StructuredStringBuilder sb)
    {
        using (sb.IncreaseDepth())
        {
            sb.AppendLine(".Select(b => new MemorySlice<byte>(b.ToArray()))");
        }
    }
}