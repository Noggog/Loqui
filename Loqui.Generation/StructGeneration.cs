using Noggog;

namespace Loqui.Generation;

public class StructGeneration : ObjectGeneration
{
    public override bool Abstract => false;

    public override bool NullableDefault => false;

    public override string ProtectedKeyword => "private";

    public StructGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FilePath sourceFile)
        : base(gen, protoGen, sourceFile)
    {
    }

    protected override async Task GenerateCtor(StructuredStringBuilder sb)
    {
        if (BasicCtorPermission == CtorPermissionLevel.noGeneration) return;
        if (BasicCtorPermission == CtorPermissionLevel.@public)
        {
            sb.AppendLine($"public {Name}(");
            List<string> lines = new List<string>();
            foreach (var field in IterateFields())
            {
                lines.Add($"{field.TypeName(getter: false)} {field.Name} = default({field.TypeName(getter: false)})");
            }
            for (int i = 0; i < lines.Count; i++)
            {
                using (sb.IncreaseDepth())
                {
                    using (sb.Line())
                    {
                        sb.Append(lines[i]);
                        if (i != lines.Count - 1)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            sb.Append(")");
                        }
                    }
                }
            }

            using (sb.CurlyBrace())
            {
                foreach (var field in IterateFields())
                {
                    sb.AppendLine($"this.{field.Name} = {field.Name};");
                }
                foreach (var mod in gen.GenerationModules)
                {
                    await mod.GenerateInCtor(this, sb);
                }
                sb.AppendLine("CustomCtor();");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"{BasicCtorPermission.ToStringFast_Enum_Only()} {Name}({Interface(getter: true)} rhs)");
        using (sb.CurlyBrace())
        {
            foreach (var field in IterateFields())
            {
                sb.AppendLine($"this.{field.Name} = {field.GenerateACopy("rhs." + field.Name)};");
            }
        }
        sb.AppendLine();

        sb.AppendLine("partial void CustomCtor();");
        sb.AppendLine();
    }

    protected override async Task GenerateClassLine(StructuredStringBuilder sb)
    {
        // Generate class header and interfaces
        using (sb.Line())
        {
            using (var args = sb.Class($"{Name}{GetGenericTypes(MaskType.Normal)}"))
            {
                args.Partial = true;
                args.Type = Class.ObjectType.@struct;
                args.Interfaces.Add(Interface(getter: true));
                args.Interfaces.Add(Interfaces.Get(LoquiInterfaceType.Direct));
                args.Interfaces.Add(await GetApplicableInterfaces(LoquiInterfaceType.Direct).ToListAsync());
                args.Interfaces.Add($"IEquatable<{ObjectName}>");
            }
        }
    }

    public override async Task Load()
    {
        await base.Load();
        foreach (var field in IterateFields())
        {
            field.NullableProperty.OnNext((false, true));
            field.ReadOnly = true;
        }
    }

    protected override async Task GenerateLoquiReflectionSetterInterface(StructuredStringBuilder sb)
    {
    }

    protected override async Task GenerateSetterInterface(StructuredStringBuilder sb)
    {
    }

    protected override async Task GenerateClearCommon(StructuredStringBuilder sb, MaskTypeSet maskCol)
    {
    }

    protected override async Task GenerateDeepCopyInExtensions(StructuredStringBuilder sb)
    {
    }

    public void GenerateCopyCtor(StructuredStringBuilder sb, string accessor, string rhs)
    {
    }
}