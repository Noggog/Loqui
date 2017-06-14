using System;
using static Loqui.Generation.LoquiType;

namespace Loqui.Generation
{
    public class LoquiMaskFieldGeneration : MaskModuleField
    {
        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            return loqui.ErrorMaskItemString;
        }

        public static string GetObjectErrorMask(LoquiType loqui, string accessor)
        {
            return $"new MaskItem<Exception, {loqui.ErrorMaskItemString}>(null, {accessor})";
        }

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

        public bool IsUnknownGeneric(LoquiType type)
        {
            return type.RefType != LoquiRefType.Direct
                && type.TargetObjectGeneration == null;
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"{accessor}.ToString(fg);");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!eval(this.{field.Name}.Overall)) return false;");
                if (!IsUnknownGeneric(loqui))
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
                if (!IsUnknownGeneric(loqui))
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

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            LoquiType loqui = field as LoquiType;
            if (!IsUnknownGeneric(loqui))
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<Exception, {loqui.ErrorMaskItemString}>({accessor}.Overall.Combine({rhsAccessor}.Overall), {accessor}.Specific.Combine({rhsAccessor}.Specific));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<Exception, {loqui.ErrorMaskItemString}>({accessor}.Overall.Combine({rhsAccessor}.Overall), Loqui.Internal.CombineHelper.Combine({accessor}.Specific, {rhsAccessor}.Specific));");
            }
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor)
        {
            return $"{maskAccessor}?.{field.Name}?.Overall ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<T, {loqui.GenerateMaskString("T")}>({valueStr}, {(loqui.TargetObjectGeneration == null ? "null" : $"new {loqui.TargetObjectGeneration.GetMaskString("T")}({valueStr})")});");
        }
    }
}
