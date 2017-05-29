using System;
using static Loqui.Generation.LoquiType;

namespace Loqui.Generation
{
    public class LoquiMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}> {field.Name} {{ get; set; }}");
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<Exception, {loqui.ErrorMaskItemString}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<Exception, {loqui.ErrorMaskItemString}>(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = (MaskItem<Exception, {loqui.ErrorMaskItemString}>)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            if (loqui.RefType == LoquiRefType.Direct)
            {
                if (loqui.SingletonType == SingletonLevel.Singleton)
                {
                    if (loqui.InterfaceType == LoquiInterfaceType.IGetter) return;
                    fg.AppendLine($"public MaskItem<bool, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
                }
            }
            else
            {
                if (loqui.TargetObjectGeneration == null)
                {
                    fg.AppendLine($"public {nameof(GetterCopyOption)} {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
                }
            }
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"if ({accessor}.Overall != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.Overall.ToString());");
            }
            fg.AppendLine($"if ({accessor}.Specific != null)");
            using (new BraceWrapper(fg))
            {
                if (loqui.RefType == LoquiRefType.Direct
                    || loqui.TargetObjectGeneration != null)
                {
                    fg.AppendLine($"{accessor}.Specific.ToString(fg);");
                }
                else
                {
                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.Specific.ToString());");
                }
            }
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!eval(this.{field.Name}.Overall)) return false;");
                if (loqui.RefType == LoquiRefType.Direct
                    || loqui.TargetObjectGeneration != null)
                {
                    fg.AppendLine($"if ({field.Name}.Specific != null && !{field.Name}.Specific.AllEqual(eval)) return false;");
                }
                else
                {
                    fg.AppendLine($"throw new {nameof(NotImplementedException)}();");
                }
            }
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor)
        {
            LoquiType loqui = field as LoquiType;

            fg.AppendLine($"if ({rhsAccessor} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<R, {loqui.GenerateMaskString("R")}>();");
                fg.AppendLine($"{retAccessor}.Overall = eval({rhsAccessor}.Overall);");
                if (loqui.RefType == LoquiRefType.Direct
                    || loqui.TargetObjectGeneration != null)
                {
                    fg.AppendLine($"if ({rhsAccessor}.Specific != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{retAccessor}.Specific = {rhsAccessor}.Specific.Translate(eval);");
                    }
                }
                else
                {
                    fg.AppendLine($"throw new {nameof(NotImplementedException)}();");
                }
            }
        }
    }
}
