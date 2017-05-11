using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Loqui.Generation
{
    public class StructGeneration : ObjectGeneration
    {
        public override bool Abstract => false;

        public override NotifyingOption NotifyingDefault => NotifyingOption.None;

        public override string ProtectedKeyword => "private";

        public StructGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        protected override void GenerateCtor(FileGeneration fg)
        {
            if (this.GeneratePublicBasicCtor)
            {
                fg.AppendLine($"public {this.Name}(");
                List<string> lines = new List<string>();
                foreach (var field in this.Fields)
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
                    foreach (var field in this.Fields)
                    {
                        fg.AppendLine($"this.{field.Name} = {field.Name};");
                    }
                    fg.AppendLine("CustomCtor();");
                }
                fg.AppendLine();
            }

            fg.AppendLine($"{(this.GeneratePublicBasicCtor ? "public" : "private")} {this.Name}({this.Getter_InterfaceStr} rhs)");
            using (new BraceWrapper(fg))
            {
                foreach (var field in this.Fields)
                {
                    fg.AppendLine($"this.{field.Name} = {field.GenerateACopy("rhs." + field.Name)};");
                }
            }
            fg.AppendLine();

            fg.AppendLine("partial void CustomCtor();");
            fg.AppendLine();
        }

        protected override void GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            using (new LineWrapper(fg))
            {
                fg.Append($"public partial struct {Name}{this.GenericTypes} : ");

                List<string> list = new List<string>
                {
                    this.Getter_InterfaceStr
                };
                list.AddRange(
                    this.Interfaces
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.Interfaces(this)))
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.GetWriterInterfaces(this)))
                        .Union(this.GenerationInterfaces
                            .SelectMany((tr) => tr.Interfaces(this))));
                list.Add($"IEquatable<{this.ObjectName}>");
                fg.Append(string.Join(", ", list));
            }
        }

        protected override void GenerateEqualsCode(FileGeneration fg)
        {
            fg.AppendLine($"if (!(obj is {this.ObjectName} rhs)) return false;");
            fg.AppendLine($"return Equals(rhs);");
        }

        public override void Load()
        {
            this.Interfaces.Add($"ILoquiWriterSerializer<{this.ErrorMask}>");

            base.Load();
            foreach (var field in this.Fields)
            {
                field.Notifying = NotifyingOption.None;
                field.Protected = true;
            }
        }

        protected override void GenerateLoquiSetterInterface(FileGeneration fg)
        {
        }

        protected override void GenerateSetterInterface(FileGeneration fg)
        {
        }

        protected override void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
        }

        protected override void GenerateClear(FileGeneration fg, bool classFile)
        {
        }

        public override void GenerateCopy(FileGeneration fg)
        {
            fg.AppendLine($"public static {this.ObjectName} Copy({this.Getter_InterfaceStr} item)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"return new {this.ObjectName}(");
                List<string> lines = new List<string>();
                foreach (var field in this.Fields)
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

        protected override void GenerateCopyFieldsFrom(FileGeneration fg)
        {
        }

        protected override void GenerateSetNthObjectHasBeenSet(FileGeneration fg, bool internalUse)
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
