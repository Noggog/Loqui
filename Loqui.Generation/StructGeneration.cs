using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class StructGeneration : ObjectGeneration
    {
        public override bool Abstract => false;

        public override NotifyingType NotifyingDefault => NotifyingType.None;
        public override bool HasBeenSetDefault => false;
        public override bool ObjectCentralizedDefault => false;

        public override string ProtectedKeyword => "private";

        public StructGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        protected override async Task GenerateCtor(FileGeneration fg)
        {
            if (this.BasicCtorPermission == PermissionLevel.@public)
            {
                fg.AppendLine($"public {this.Name}(");
                List<string> lines = new List<string>();
                foreach (var field in this.IterateFields())
                {
                    lines.Add($"{field.TypeName} {field.Name} = default({field.TypeName})");
                }
                for (int i = 0; i < lines.Count; i++)
                {
                    using (new DepthWrapper(fg))
                    {
                        using (new LineWrapper(fg))
                        {
                            fg.Append(lines[i]);
                            if (i != lines.Count - 1)
                            {
                                fg.Append(",");
                            }
                            else
                            {
                                fg.Append(")");
                            }
                        }
                    }
                }

                using (new BraceWrapper(fg))
                {
                    foreach (var field in this.IterateFields())
                    {
                        fg.AppendLine($"this.{field.Name} = {field.Name};");
                    }
                    foreach (var mod in this.gen.GenerationModules)
                    {
                        await mod.GenerateInCtor(this, fg);
                    }
                    fg.AppendLine("CustomCtor();");
                }
                fg.AppendLine();
            }

            fg.AppendLine($"{this.BasicCtorPermission.ToStringFast_Enum_Only()} {this.Name}({this.Interface(getter: true)} rhs)");
            using (new BraceWrapper(fg))
            {
                foreach (var field in this.IterateFields())
                {
                    fg.AppendLine($"this.{field.Name} = {field.GenerateACopy("rhs." + field.Name)};");
                }
            }
            fg.AppendLine();

            fg.AppendLine("partial void CustomCtor();");
            fg.AppendLine();
        }

        protected override async Task GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            using (new LineWrapper(fg))
            {
                fg.Append($"public partial struct {Name}{this.GetGenericTypes(MaskType.Normal)} : ");

                List<string> list = new List<string>
                {
                    this.Interface(getter: true)
                };
                list.AddRange(this.Interfaces);
                list.AddRange((await Task.WhenAll(this.gen.GenerationModules
                            .Select((tr) => tr.Interfaces(this))))
                            .SelectMany(i => i));
                list.AddRange((await Task.WhenAll(this.gen.GenerationModules
                            .Select((tr) => tr.Interfaces(this))))
                            .SelectMany(i => i));
                list.Add($"IEquatable<{this.ObjectName}>");
                fg.Append(string.Join(", ", list.Distinct()));
            }
        }

        protected override void GenerateEqualsCode(FileGeneration fg)
        {
            fg.AppendLine($"if (!(obj is {this.ObjectName} rhs)) return false;");
            fg.AppendLine($"return Equals(rhs);");
        }

        public override async Task Load()
        {
            this.Interfaces.Add($"ILoquiWriterSerializer<{this.Mask(MaskType.Error)}>");

            await base.Load();
            foreach (var field in this.IterateFields())
            {
                field.NotifyingProperty.Item = NotifyingType.None;
                field.HasBeenSetProperty.Item = false;
                field.ReadOnly = true;
            }
        }

        protected override async Task GenerateLoquiReflectionSetterInterface(FileGeneration fg)
        {
        }

        protected override async Task GenerateSetterInterface(FileGeneration fg)
        {
        }

        protected override void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
        }

        protected override async Task GenerateClear(FileGeneration fg, bool classFile)
        {
        }

        public override void GenerateCopy(FileGeneration fg)
        {
            fg.AppendLine($"public static {this.ObjectName} Copy({this.Interface(getter: true)} item)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"return new {this.ObjectName}(");
                List<string> lines = new List<string>();
                foreach (var field in this.IterateFields())
                {
                    lines.Add($"{field.Name}: {field.GenerateACopy("item." + field.Name)}");
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    using (new DepthWrapper(fg))
                    {
                        using (new LineWrapper(fg))
                        {
                            fg.Append(lines[i]);
                            if (i != lines.Count - 1)
                            {
                                fg.Append(",");
                            }
                            else
                            {
                                fg.Append(");");
                            }
                        }
                    }
                }
            }
            fg.AppendLine();
        }

        protected override async Task GenerateCopyFieldsFrom(FileGeneration fg)
        {
        }

        protected override void GenerateSetNthObjectHasBeenSet(FileGeneration fg)
        {
        }

        public void GenerateCopyCtor(FileGeneration fg, string accessor, string rhs)
        {
        }

        protected override void GenerateStaticCopy_ToLoqui(FileGeneration fg)
        {
            fg.AppendLine($"return {this.ObjectName}.Copy(item);");
        }
    }
}
